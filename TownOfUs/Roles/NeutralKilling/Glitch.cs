using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Glitch : RoleBase<Glitch>
    {
        private static CustomButton glitchKillButton;
        private static CustomButton glitchMimicButton;
        private static CustomButton glitchHackButton;
        public static Color color = Color.green;
        private static Sprite hackButtonSprite;
        private static Sprite mimicButtonSprite;

        public static float morphCooldown { get => CustomOptionHolder.glitchMimicCooldown.getFloat(); }
        public static float morphDuration { get => CustomOptionHolder.glitchMimicDuration.getFloat(); }
        public static float hackCooldown { get => CustomOptionHolder.glitchHackCooldown.getFloat(); }
        public static float hackDuration { get => CustomOptionHolder.glitchHackDuration.getFloat(); }
        public static float killCooldown { get => CustomOptionHolder.glitchKillCooldown.getFloat(); }
        public static bool canVent { get => CustomOptionHolder.glitchCanVent.getBool(); }
        public static bool hasImpostorVision { get => CustomOptionHolder.glitchHasImpostorVision.getBool(); }

        public static float morphTimer = 0f;
        public static PlayerControl morphPlayer;
        public static PlayerControl sampledPlayer;
        public static PlayerControl hackedPlayer;
        public static List<byte> mimicTargets = new List<byte>();
        public static Dictionary<PlayerControl, DateTime> hackedPlayers = new Dictionary<PlayerControl, DateTime>();

        public PlayerControl currentTarget;
        public Glitch()
        {
            RoleType = roleId = RoleId.Glitch;
            currentTarget = null;
        }

        public void resetMorph()
        {
            morphPlayer = null;
            morphTimer = 0f;
            if (player == null) return;
            player.setDefaultLook();
        }

        public override void OnMeetingStart()
        {
            resetMorph();
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = Helpers.setTarget();
                Helpers.setPlayerOutline(currentTarget, color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            resetMorph();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public override void ResetRole(bool isShifted)
        {
            resetMorph();
        }

        public static void makeButtons(HudManager hm)
        {
            // Glitch Kill
            glitchKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, local.currentTarget);
                    if (murder == MurderAttemptResult.SuppressKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        return;
                    }
                    if (murder == MurderAttemptResult.ReverseKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.checkMurderAttemptAndKill(local.currentTarget, PlayerControl.LocalPlayer);
                        return;
                    }
                    if (murder == MurderAttemptResult.BlankKill)
                    {
                        Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                        Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                        glitchKillButton.Timer = glitchKillButton.MaxTimer;
                        local.currentTarget = null;
                        return;
                    }

                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Helpers.MurderPlayer(PlayerControl.LocalPlayer, local.currentTarget, true);
                        glitchKillButton.Timer = glitchKillButton.MaxTimer;
                        local.currentTarget = null;
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Glitch) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { glitchKillButton.Timer = glitchKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite, CustomButton.ButtonPositions.upperRowRight, hm, KeyCode.Q
            );

            // Glitch Mimic
            glitchMimicButton = new CustomButton(
                () =>
                {
                    if (sampledPlayer == null)
                    {
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            if (player != PlayerControl.LocalPlayer && !player.Data.Disconnected)
                            {
                                if (!player.Data.IsDead) mimicTargets.Add(player.PlayerId);
                                else
                                {
                                    foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                                    {
                                        if (body.ParentId == player.PlayerId) mimicTargets.Add(player.PlayerId);
                                    }
                                }
                            }
                        }
                        byte[] mimictargetIDs = mimicTargets.ToArray();
                        var pk = new Glitch.PlayerMenu((x) =>
                        {
                            sampledPlayer = x;
                        }, (y) =>
                        {
                            return mimictargetIDs.Contains(y.PlayerId);
                        });
                        Coroutines.Start(pk.Open(0f, true));
                        glitchMimicButton.EffectDuration = 1f;
                        glitchMimicButton.shakeOnEnd = false;
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchMimic, SendOption.Reliable, -1);
                        writer.Write(sampledPlayer.PlayerId);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.glitchMimic(sampledPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                        sampledPlayer = null;
                        glitchMimicButton.EffectDuration = morphDuration;
                    }
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Glitch) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    glitchMimicButton.Timer = glitchMimicButton.MaxTimer;
                    glitchMimicButton.isEffectActive = false;
                    glitchMimicButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    sampledPlayer = null;
                },
                getMimicButtonSprite(), new Vector3(0f, 1f, 0f), hm, KeyCode.F,
                true, morphDuration, () => { glitchMimicButton.Timer = glitchMimicButton.MaxTimer; }, true
            );

            // Glitch Hack
            glitchHackButton = new CustomButton(
                () =>
                {
                    Helpers.politicianRpcCampaign(PlayerControl.LocalPlayer, local.currentTarget);
                    Helpers.plaguebearerRpcInfect(PlayerControl.LocalPlayer, local.currentTarget);
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, local.currentTarget)) return;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GlitchHack, SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.glitchHack(local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.isRole(RoleId.Glitch) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return local.currentTarget && hackedPlayer == null && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    glitchHackButton.Timer = glitchHackButton.MaxTimer;
                    glitchHackButton.isEffectActive = false;
                    glitchHackButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    sampledPlayer = null;
                },
                getHackButtonSprite(), new Vector3(0f, 0f, 0f), hm, KeyCode.G,
                true, hackDuration, () => { glitchHackButton.Timer = glitchHackButton.MaxTimer; }, true
            );
        }
        public static void setButtonCooldowns()
        {
            glitchKillButton.MaxTimer = killCooldown;
            glitchMimicButton.MaxTimer = morphCooldown;
            glitchMimicButton.EffectDuration = morphDuration;
            glitchHackButton.MaxTimer = hackCooldown;
            glitchHackButton.EffectDuration = hackDuration;
        }

        public static void Clear()
        {
            sampledPlayer = null;
            hackedPlayer = null;
            mimicTargets = new List<byte>();
            hackedPlayers = new Dictionary<PlayerControl, DateTime>();
            players = new List<Glitch>();
        }

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public static Sprite getMimicButtonSprite()
            => mimicButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MimicButton.png", 100f);
        public static Sprite getHackButtonSprite()
            => hackButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.HackButton.png", 100f);
        
        private static float[] PanelAreaScale = [1f, 0.95f, 0.76f];
        private static (int x, int y)[] PanelAreaSize = [(3, 5), (3, 6), (4, 6)];
        private static Vector3[] PanelAreaOffset = [new(0.0f, 0.0f, -1f), new(0.1f, 0.145f, -1f), new(-0.355f, 0.0f, -1f)];
        private static (float x, float y)[] PanelAreaMultiplier = [(1f, 1f), (1f, 0.89f), (275f * (float)Math.PI / 887f, 1f)];
        private static Vector3 ToVoteAreaPos(ShapeshifterMinigame minigame, int index, int arrangeType) => Helpers.convertPos(index, arrangeType, PanelAreaSize, new Vector3(minigame.XStart, minigame.YStart, -1f), PanelAreaOffset, new Vector3(minigame.XOffset, minigame.YOffset), PanelAreaScale, PanelAreaMultiplier);

        public class PlayerMenu
        {
            public ShapeshifterMinigame Menu;
            public Select Click;
            public Include Inclusion;
            public List<PlayerControl> Targets;
            public static PlayerMenu singleton;
            public delegate void Select(PlayerControl player);
            public delegate bool Include(PlayerControl player);

            public PlayerMenu(Select click, Include inclusion)
            {
                Click = click;
                Inclusion = inclusion;
                if (singleton != null)
                {
                    singleton.Menu.DestroyImmediate();
                    singleton = null;
                }
                singleton = this;
            }

            public IEnumerator Open(float delay, bool includeDead = false)
            {
                yield return new WaitForSecondsRealtime(delay);
                while (ExileController.Instance != null) yield return 0; Targets = PlayerControl.AllPlayerControls.ToArray().Where(x => Inclusion(x) && (!x.Data.IsDead || includeDead) && !x.Data.Disconnected).ToList();
                if (Menu == null)
                {
                    if (Camera.main == null)
                        yield break;

                    Menu = UnityEngine.Object.Instantiate(GetShapeshifterMenu(), Camera.main.transform, false);
                }

                Menu.transform.SetParent(Camera.main.transform, false);
                Menu.transform.localPosition = new(0f, 0f, -50f);
                Menu.Begin(null);
            }

            private static ShapeshifterMinigame GetShapeshifterMenu()
            {
                var rolePrefab = RoleManager.Instance.AllRoles.First(r => r.Role == RoleTypes.Shapeshifter);
                return UnityEngine.Object.Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;
            }

            public void Clicked(PlayerControl player)
            {
                Click(player);
                Menu.Close();
            }

            [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
            public static class MenuPatch
            {
                public static bool Prefix(ShapeshifterMinigame __instance)
                {
                    PlayerControl.LocalPlayer.MyPhysics.ResetMoveState(false);
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    var menu = singleton;

                    if (menu == null)
                        return true;

                    __instance.potentialVictims = new();
                    var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

                    for (var i = 0; i < menu.Targets.Count; i++)
                    {
                        int displayType = Helpers.GetDisplayType(menu.Targets.Count);
                        var player = menu.Targets[i];
                        bool isDead = player.Data.IsDead;
                        player.Data.IsDead = false;
                        var num = i % 3;
                        var num2 = i / 3;
                        var panel = UnityEngine.Object.Instantiate(__instance.PanelPrefab, __instance.transform);
                        panel.transform.localScale *= PanelAreaScale[displayType];
                        panel.transform.localPosition = ToVoteAreaPos(__instance, i, displayType);
                        panel.SetPlayer(i, player.Data, (Action)(() => menu.Clicked(player)));
                        panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
                        panel.Background.gameObject.GetComponent<ButtonRolloverHandler>().OverColor = color;
                        __instance.potentialVictims.Add(panel);
                        list2.Add(panel.Button);
                        player.Data.IsDead = isDead;
                    }

                    var Phone = __instance.transform.Find("PhoneUI/Background").GetComponent<SpriteRenderer>();
                    if (Phone != null)
                    {
                        Phone.material?.SetColor(PlayerMaterial.BodyColor, color);
                        Phone.material?.SetColor(PlayerMaterial.BackColor, color - new Color(0.25f, 0.25f, 0.25f));
                    }
                    var PhoneButton = __instance.transform.Find("PhoneUI/UI_Phone_Button").GetComponent<SpriteRenderer>();
                    if (PhoneButton != null)
                    {
                        PhoneButton.material?.SetColor(PlayerMaterial.BodyColor, color);
                        PhoneButton.material?.SetColor(PlayerMaterial.BackColor, color - new Color(0.25f, 0.25f, 0.25f));
                    }

                    ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, list2);
                    return false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
            public static class StartMeeting
            {
                public static void Prefix(PlayerControl __instance)
                {
                    if (__instance == null) return;
                    try
                    {
                        singleton.Menu.Close();
                    }
                    catch { }
                }
            }
        }
    }
}