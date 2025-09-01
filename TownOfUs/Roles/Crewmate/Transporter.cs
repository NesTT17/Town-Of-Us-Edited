using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class Transporter : RoleBase<Transporter>
    {
        private static CustomButton transporterButton;
        private static TMP_Text transporterButtonText;
        public static Color color = new Color(0f, 0.93f, 1f, 1f);
        private static Sprite buttonSprite;

        public static float cooldown { get => CustomOptionHolder.transporterCooldown.getFloat(); }

        public bool SwappingMenus;
        public PlayerControl target1;
        public PlayerControl target2;
        public int transportNumber = 5;
        public Transporter()
        {
            RoleType = roleId = RoleId.Transporter;
            target1 = null;
            target2 = null;
            SwappingMenus = false;
            transportNumber = Mathf.RoundToInt(CustomOptionHolder.transporterNumberOfTransports.getFloat());
        }

        public IEnumerator OpenSecondMenu()
        {
            try
            {
                Transporter.PlayerMenu.singleton.Menu.ForceClose();
            }
            catch
            {

            }
            yield return new WaitForSeconds(0.05f);
            SwappingMenus = false;
            if (MeetingHud.Instance || PlayerControl.LocalPlayer != player) yield break;
            List<byte> transportTargets = [];
            foreach (var player in PlayerControl.AllPlayerControls)
                if (!player.Data.Disconnected && player != target1)
                    if (!player.Data.IsDead) transportTargets.Add(player.PlayerId);
                    else
                        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                            if (body.ParentId == player.PlayerId) transportTargets.Add(player.PlayerId);
            byte[] transporttargetIDs = transportTargets.ToArray();
            var pk = new Transporter.PlayerMenu((x) =>
            {
                target2 = x;
                if (Helpers.checkSuspendAction(player, target1)) return;
                if (Helpers.checkSuspendAction(player, target2)) return;
                Helpers.politicianRpcCampaign(player, target1);
                Helpers.politicianRpcCampaign(player, target2);
                Helpers.plaguebearerRpcInfect(player, target1);
                Helpers.plaguebearerRpcInfect(player, target2);
                if (target1 != null && target2 != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TransporterTransport, SendOption.Reliable, -1);
                    writer.Write(target1.PlayerId);
                    writer.Write(target2.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    Coroutines.Start(Helpers.TransportPlayers(target1.PlayerId, target2.PlayerId));
                }
                target1 = null;
                target2 = null;
                transporterButton.Timer = transporterButton.MaxTimer;
                transportNumber--;
            }, (y) =>
            {
                return transporttargetIDs.Contains(y.PlayerId);
            });
            Coroutines.Start(pk.Open(0f, true));
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void makeButtons(HudManager hm)
        { 
            transporterButton = new CustomButton(
                () =>
                {
                    List<byte> transportTargets = [];
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.Data.Disconnected)
                        {
                            if (!player.Data.IsDead) transportTargets.Add(player.PlayerId);
                            else
                            {
                                foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                                {
                                    if (body.ParentId == player.PlayerId) transportTargets.Add(player.PlayerId);
                                }
                            }
                        }
                    }
                    byte[] transporttargetIDs = transportTargets.ToArray();
                    var pk = new Transporter.PlayerMenu((x) =>
                    {
                        local.target1 = x;
                        local.SwappingMenus = true;
                        Coroutines.Start(local.OpenSecondMenu());
                    }, (y) =>
                    {
                        return transporttargetIDs.Contains(y.PlayerId);
                    });
                    Coroutines.Start(pk.Open(0f, true));
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Transporter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (transporterButtonText != null) transporterButtonText.text = $"{local.transportNumber}";
                    
                    return local.transportNumber > 0 && PlayerControl.LocalPlayer.CanMove && !local.SwappingMenus;
                },
                () => { transporterButton.Timer = transporterButton.MaxTimer; },
                getButtonSprite(), CustomButton.ButtonPositions.lowerRowRight, hm, KeyCode.F
            );
            transporterButtonText = GameObject.Instantiate(transporterButton.actionButton.cooldownTimerText, transporterButton.actionButton.cooldownTimerText.transform.parent);
            transporterButtonText.text = "";
            transporterButtonText.enableWordWrapping = false;
            transporterButtonText.transform.localScale = Vector3.one * 0.5f;
            transporterButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        public static void setButtonCooldowns()
        {
            transporterButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<Transporter>();
        }

        public static Sprite getButtonSprite()
            => buttonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.TransportButton.png", 100f);

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