using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch]
    public static class TownOfUs
    {
        public static DateTime startTime = DateTime.UtcNow;
        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);

        public static void clearAndReloadRoles()
        {
            startTime = DateTime.UtcNow;
            PlayerGameInfo.clearAndReload();

            // Roles
            Sheriff.clearAndReload();
            Jester.clearAndReload();
            Survivor.clearAndReload();
            Juggernaut.clearAndReload();
            Seer.clearAndReload();
            Engineer.clearAndReload();
            Snitch.clearAndReload();
            Veteran.clearAndReload();
            Camouflager.clearAndReload();
            Morphling.clearAndReload();
            Pursuer.clearAndReload();
            Amnesiac.clearAndReload();
            Thief.clearAndReload();
            Lawyer.clearAndReload();
            Executioner.clearAndReload();
            Medic.clearAndReload();
            Swapper.clearAndReload();
            Investigator.clearAndReload();
            Spy.clearAndReload();
            Tracker.clearAndReload();
            Trapper.clearAndReload();
            Detective.clearAndReload();
            Mystic.clearAndReload();
            GuardianAngel.clearAndReload();
            Swooper.clearAndReload();
            Arsonist.clearAndReload();
            Werewolf.clearAndReload();
            Miner.clearAndReload();
            Janitor.clearAndReload();
            Undertaker.clearAndReload();
            Grenadier.clearAndReload();
            Blackmailer.clearAndReload();
            Politician.clearAndReload();
            Mayor.clearAndReload();
            Dracula.clearAndReload();
            Vampire.clearAndReload();
            Poisoner.clearAndReload();
            Venerer.clearAndReload();
            Plaguebearer.clearAndReload();
            Pestilence.clearAndReload();
            Doomsayer.clearAndReload();
            Glitch.clearAndReload();
            Cannibal.clearAndReload();
            Scavenger.clearAndReload();
            Escapist.clearAndReload();
            VampireHunter.clearAndReload();
            Oracle.clearAndReload();
            Lookout.clearAndReload();
            Plumber.clearAndReload();
            Deceiver.clearAndReload();

            // Modifiers
            Bait.clearAndReload();
            Blind.clearAndReload();
            ButtonBarry.clearAndReload();
            Shy.clearAndReload();
            Flash.clearAndReload();
            Mini.clearAndReload();
            Indomitable.clearAndReload();
            Lovers.clearAndReload();
            Multitasker.clearAndReload();
            Radar.clearAndReload();
            Sleuth.clearAndReload();
            Tiebreaker.clearAndReload();
            Torch.clearAndReload();
            Vip.clearAndReload();
            Drunk.clearAndReload();
            Immovable.clearAndReload();
            DoubleShot.clearAndReload();
            Ruthless.clearAndReload();
            Underdog.clearAndReload();
            Saboteur.clearAndReload();
            Frosty.clearAndReload();
            Satelite.clearAndReload();
            SixthSense.clearAndReload();
            Taskmaster.clearAndReload();
            Disperser.clearAndReload();
            Poucher.clearAndReload();

            // Ghost Roles
            Banshee.clearAndReload();
            Poltergeist.clearAndReload();

            HandleGuesser.clearAndReload();
        }
    }

    #region Roles
    public class Sheriff
    {
        public static readonly Dictionary<byte, Sheriff> sheriffs = new Dictionary<byte, Sheriff>();
        public static Color color = Color.yellow;
        public enum MissfireDeath { Target, Sheriff, Both }

        public readonly PlayerControl sheriff;
        public PlayerControl currentTarget = null;
        public Sheriff(PlayerControl player)
        {
            sheriff = player;
            sheriffs.Add(player.PlayerId, this);
        }

        public static float cooldown = 30f;
        public static bool canKillNeutralBenign = false;
        public static bool canKillNeutralEvil = false;
        public static bool canKillNeutralKilling = false;
        public static MissfireDeath whoDiesOnMissfire = MissfireDeath.Target;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ShootButton.png", 100f);
            return buttonSprite;
        }

        public static bool IsSheriff(byte playerId, out Sheriff sheriff)
        {
            return sheriffs.TryGetValue(playerId, out sheriff);
        }

        public static bool IsSheriff(PlayerControl player)
        {
            return sheriffs.ContainsKey(player.PlayerId);
        }

        public static void RemoveSheriff(byte playerId)
        {
            sheriffs.Remove(playerId);
        }

        public static void clearAndReload()
        {
            sheriffs.Clear();
            cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
            canKillNeutralBenign = CustomOptionHolder.sheriffCanKillNeutralBenign.getBool();
            canKillNeutralEvil = CustomOptionHolder.sheriffCanKillNeutralEvil.getBool();
            canKillNeutralKilling = CustomOptionHolder.sheriffCanKillNeutralKilling.getBool();
            whoDiesOnMissfire = (MissfireDeath)CustomOptionHolder.sheriffWhoDiesOnMissfire.getSelection();
        }
    }

    public class Jester
    {
        public static readonly Dictionary<byte, Jester> jesters = new Dictionary<byte, Jester>();
        public static Color color = new Color(1f, 0.75f, 0.8f, 1f);
        public static PlayerControl winningJesterPlayer = null;
        public static bool triggerJesterWin = false;

        public readonly PlayerControl jester;
        public bool votedOut;
        public Jester(PlayerControl player)
        {
            jester = player;
            votedOut = false;
            jesters.Add(player.PlayerId, this);
        }

        public static bool canCallEmergency = false;
        public static bool hasImpostorVision = false;
        public static bool canEnterVents = false;

        public static bool IsJester(byte playerId, out Jester jester)
        {
            return jesters.TryGetValue(playerId, out jester);
        }

        public static bool IsJester(PlayerControl player)
        {
            return jesters.ContainsKey(player.PlayerId);
        }

        public static void RemoveJester(byte playerId)
        {
            jesters.Remove(playerId);
        }

        public static void clearAndReload()
        {
            jesters.Clear();
            triggerJesterWin = false;
            winningJesterPlayer = null;
            canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
            hasImpostorVision = CustomOptionHolder.jesterHasImpostorVision.getBool();
            canEnterVents = CustomOptionHolder.jesterCanEnterVents.getBool();
        }
    }

    public class Survivor
    {
        public static readonly Dictionary<byte, Survivor> survivors = new Dictionary<byte, Survivor>();
        public static Color color = new Color(1f, 0.9f, 0.3f, 1f);

        public readonly PlayerControl survivor;
        public bool safeguardActive;
        public int remainingSafeguards;
        public Survivor(PlayerControl player)
        {
            survivor = player;
            safeguardActive = false;
            remainingSafeguards = CustomOptionHolder.survivorNumberOfSafeguards.getInt();
            survivors.Add(player.PlayerId, this);
        }

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float cooldownReset = 30f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.SafeguardButton.png", 100f);
            return buttonSprite;
        }

        public static bool IsSurvivor(byte playerId, out Survivor survivor)
        {
            return survivors.TryGetValue(playerId, out survivor);
        }

        public static bool IsSurvivor(PlayerControl player)
        {
            return survivors.ContainsKey(player.PlayerId);
        }

        public static void RemoveSurvivor(byte playerId)
        {
            survivors.Remove(playerId);
        }

        public static void clearAndReload()
        {
            survivors.Clear();
            cooldown = CustomOptionHolder.survivorSafeguardCooldown.getFloat();
            duration = CustomOptionHolder.survivorSafeguardDuration.getFloat();
            cooldownReset = CustomOptionHolder.survivorCooldownReset.getFloat();
        }
    }

    public static class Juggernaut
    {
        public static PlayerControl juggernaut;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.55f, 0f, 0.3f, 1f);

        public static int numberOfKills = 0;

        public static float cooldown = 30f;
        public static float cooldownDecrease = 30f;
        public static bool hasImpostorVision = false;
        public static bool canVent = false;

        public static void clearAndReload()
        {
            juggernaut = null;
            currentTarget = null;
            numberOfKills = 0;
            cooldown = CustomOptionHolder.juggernautCooldown.getFloat();
            cooldownDecrease = CustomOptionHolder.juggernautCooldownDecrease.getFloat();
            hasImpostorVision = CustomOptionHolder.juggernautHasImpostorVision.getBool();
            canVent = CustomOptionHolder.juggernautCanVent.getBool();
        }
    }

    public static class Seer
    {
        public static PlayerControl seer;
        public static PlayerControl currentTarget;
        public static List<byte> revealedIds = new List<byte>();
        public static Color color = new Color(1f, 0.8f, 0.5f, 1f);

        public static float cooldown = 30f;
        public static bool sheriffShowsEvil = false;
        public static bool veteranShowsEvil = false;
        public static bool benignNeutralsShowsEvil = false;
        public static bool evilNeutralsShowsEvil = false;
        public static bool killingNeutralsShowsEvil = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.RevealButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            seer = null;
            currentTarget = null;
            revealedIds = new List<byte>();
            foreach (PoolablePlayer p in playerIcons.Values)
            {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            cooldown = CustomOptionHolder.seerRevealCooldown.getFloat();
            sheriffShowsEvil = CustomOptionHolder.seerSheriffShowsEvil.getBool();
            veteranShowsEvil = CustomOptionHolder.seerVeteranShowsEvil.getBool();
            benignNeutralsShowsEvil = CustomOptionHolder.seerBenignNeutralsShowsEvil.getBool();
            evilNeutralsShowsEvil = CustomOptionHolder.seerEvilNeutralsShowsEvil.getBool();
            killingNeutralsShowsEvil = CustomOptionHolder.seerKillingNeutralsShowsEvil.getBool();
        }
    }

    public static class Engineer
    {
        public static PlayerControl engineer;
        public static Color color = new Color(1f, 0.65f, 0.04f, 1f);

        public static int remainingFixes = 1;
        public static bool canRecharge = false;
        public static float rechargeTasksNumber;
        public static float rechargedTasks;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.RepairButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            engineer = null;
            remainingFixes = CustomOptionHolder.engineerNumberOfRepairs.getInt();
            canRecharge = CustomOptionHolder.engineerCanRecharge.getBool();
            rechargeTasksNumber = CustomOptionHolder.engineerRechargeTasksNumber.getFloat();
            rechargedTasks = CustomOptionHolder.engineerRechargeTasksNumber.getFloat();
        }
    }

    public static class Snitch
    {
        public static PlayerControl snitch;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.83f, 0.69f, 0.22f, 1f);

        public enum Mode { Chat = 0, Map = 1, ChatAndMap = 2, Arrows = 3, }
        public enum Targets { EvilPlayers = 0, Killers = 1 }

        public static bool isRevealed = false;
        public static Dictionary<byte, byte> playerRoomMap = new Dictionary<byte, byte>();
        public static TMPro.TextMeshPro text = null;
        public static bool needsUpdate = true;

        public static int taskCountForReveal = 1;
        public static Mode mode = Mode.Chat;
        public static Targets targets = Targets.EvilPlayers;

        public static void clearAndReload()
        {
            snitch = null;
            isRevealed = false;
            playerRoomMap = new Dictionary<byte, byte>();
            if (text != null) UnityEngine.Object.Destroy(text);
            text = null;
            needsUpdate = true;
            taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat());
            mode = (Mode)CustomOptionHolder.snitchMode.getSelection();
            targets = (Targets)CustomOptionHolder.snitchTargets.getSelection();
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
        }
    }

    public class Veteran
    {
        public static readonly Dictionary<byte, Veteran> veterans = new Dictionary<byte, Veteran>();
        public static Color color = new Color(0.6f, 0.5f, 0.25f, 1f);

        public readonly PlayerControl veteran;
        public bool alertActive;
        public int remainingAlerts;
        public float rechargedTasks;
        public Veteran(PlayerControl player)
        {
            veteran = player;
            alertActive = false;
            remainingAlerts = CustomOptionHolder.veteranNumberOfAlerts.getInt();
            rechargedTasks = CustomOptionHolder.veteranRechargeTasksNumber.getFloat();
            veterans.Add(player.PlayerId, this);
        }

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool canRecharge = false;
        public static float rechargeTasksNumber = 2f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.AlertButton.png", 100f);
            return buttonSprite;
        }

        public static bool IsVeteran(byte playerId, out Veteran veteran)
        {
            return veterans.TryGetValue(playerId, out veteran);
        }

        public static bool IsVeteran(PlayerControl player)
        {
            return veterans.ContainsKey(player.PlayerId);
        }

        public static void RemoveVeteran(byte playerId)
        {
            veterans.Remove(playerId);
        }

        public static void clearAndReload()
        {
            veterans.Clear();
            cooldown = CustomOptionHolder.veteranAlertCooldown.getFloat();
            duration = CustomOptionHolder.veteranAlertDuration.getFloat();
            canRecharge = CustomOptionHolder.veteranCanRecharge.getBool();
            rechargeTasksNumber = CustomOptionHolder.veteranRechargeTasksNumber.getFloat();
        }
    }

    public static class Camouflager
    {
        public static PlayerControl camouflager;
        public static Color color = Palette.ImpostorRed;

        public static float camouflageTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool canVent = false;

        public static void resetCamouflage()
        {
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == Swooper.swooper && Swooper.isInvisible)
                    continue;
                p.setDefaultLook();
            }
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.CamoButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            resetCamouflage();
            camouflager = null;
            camouflageTimer = 0f;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
            duration = CustomOptionHolder.camouflagerDuration.getFloat();
            canVent = CustomOptionHolder.camouflagerCanVent.getBool();
        }
    }

    public static class Morphling
    {
        public static PlayerControl morphling;
        public static PlayerControl morphTarget;
        public static PlayerControl sampledPlayer;
        public static List<byte> morphTargets = new List<byte>();
        public static Color color = Palette.ImpostorRed;

        public static float morphTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool canVent = false;

        public static void resetMorph()
        {
            morphTarget = null;
            morphTimer = 0f;
            if (morphling == null) return;
            morphling.setDefaultLook();
        }

        public static void clearAndReload()
        {
            resetMorph();
            morphling = null;
            morphTarget = null;
            sampledPlayer = null;
            morphTimer = 0f;
            morphTargets = new List<byte>();
            cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
            duration = CustomOptionHolder.morphlingDuration.getFloat();
            canVent = CustomOptionHolder.morphlingCanVent.getBool();
        }

        private static Sprite sampleSprite;
        public static Sprite getSampleSprite()
        {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.SampleButton.png", 115f);
            return sampleSprite;
        }

        private static Sprite morphSprite;
        public static Sprite getMorphSprite()
        {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.MorphButton.png", 115f);
            return morphSprite;
        }

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
                while (ExileController.Instance != null) yield return 0;
                Targets = PlayerControl.AllPlayerControls.ToArray().Where(x => Inclusion(x) && (!x.Data.IsDead || includeDead) && !x.Data.Disconnected).ToList();
                TownOfUsPlugin.Logger.LogMessage($"Targets {Targets.Count}");
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
                var rolePrefab = RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == RoleTypes.Shapeshifter);
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

    public class Pursuer
    {
        public static readonly Dictionary<byte, Pursuer> pursuers = new Dictionary<byte, Pursuer>();
        public static List<PlayerControl> blankedList = new List<PlayerControl>();
        public static Color color = new Color32(201, 204, 63, byte.MaxValue);

        public readonly PlayerControl pursuer;
        public PlayerControl currentTarget;
        public int blanksNumber;
        public Pursuer(PlayerControl player)
        {
            pursuer = player;
            blanksNumber = CustomOptionHolder.pursuerBlanksNumber.getInt();
            pursuers.Add(player.PlayerId, this);
        }

        public static float cooldown = 30f;

        public static bool IsPursuer(byte playerId, out Pursuer pursuer)
        {
            return pursuers.TryGetValue(playerId, out pursuer);
        }

        public static bool IsPursuer(PlayerControl player)
        {
            return pursuers.ContainsKey(player.PlayerId);
        }

        public static void RemovePursuer(byte playerId)
        {
            pursuers.Remove(playerId);
        }

        public static void clearAndReload()
        {
            pursuers.Clear();
            blankedList = new List<PlayerControl>();
            cooldown = CustomOptionHolder.pursuerCooldown.getFloat();
        }

        public static Sprite buttonSprite;
        public static Sprite getTargetSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.PursuerButton.png", 115f);
            return buttonSprite;
        }
    }

    public class Amnesiac
    {
        public static readonly Dictionary<byte, Amnesiac> amnesiacs = new Dictionary<byte, Amnesiac>();
        public static Color color = new Color(0.5f, 0.7f, 1f, 1f);

        public readonly PlayerControl amnesiac;
        public DeadBody currentTarget;
        public List<Arrow> localArrows = new List<Arrow>();
        public Amnesiac(PlayerControl player)
        {
            amnesiac = player;
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            amnesiacs.Add(player.PlayerId, this);
        }

        public static bool showArrows = false;
        public static float delay = 3f;
        public static bool resetRole = false;
        public static bool rememberMeeting = false;

        public static bool IsAmnesiac(byte playerId, out Amnesiac amnesiac)
        {
            return amnesiacs.TryGetValue(playerId, out amnesiac);
        }

        public static bool IsAmnesiac(PlayerControl player)
        {
            return amnesiacs.ContainsKey(player.PlayerId);
        }

        public static void RemoveAmnesiac(byte playerId)
        {
            amnesiacs.Remove(playerId);
        }

        public static void clearAndReload()
        {
            amnesiacs.Clear();
            showArrows = CustomOptionHolder.amnesiacShowArrows.getBool();
            delay = CustomOptionHolder.amnesiacDelay.getFloat();
            resetRole = CustomOptionHolder.amnesiacResetRole.getBool();
            rememberMeeting = CustomOptionHolder.amnesiacCanRememberOnMeetingIfGuesser.getBool();
        }

        private static Sprite rememberButton;
        public static Sprite getButtonSprite()
        {
            if (rememberButton) return rememberButton;
            rememberButton = Helpers.loadSpriteFromResources("TownOfUs.Resources.RememberButton.png", 115f);
            return rememberButton;
        }

        private static Sprite rememberMeetingButton;
        public static Sprite getMeetingButtonSprite()
        {
            if (rememberMeetingButton) return rememberMeetingButton;
            rememberMeetingButton = Helpers.loadSpriteFromResources("TownOfUs.Resources.MeetingRemember.png", 115f);
            return rememberMeetingButton;
        }
    }

    public class Thief
    {
        public static readonly Dictionary<byte, Thief> thiefs = new Dictionary<byte, Thief>();
        public static List<PlayerControl> formerThieves = new List<PlayerControl>();
        public static Color color = new Color32(71, 99, 45, Byte.MaxValue);

        public readonly PlayerControl thief;
        public PlayerControl currentTarget;
        public bool suicideFlag;
        public Thief(PlayerControl player)
        {
            thief = player;
            suicideFlag = false;
            thiefs.Add(player.PlayerId, this);
        }

        public static float cooldown = 30f;
        public static bool hasImpostorVision;
        public static bool canUseVents;
        public static bool canKillSheriff;
        public static bool canKillVeteran;
        public static bool canStealWithGuess;

        public static bool IsThief(byte playerId, out Thief thief)
        {
            return thiefs.TryGetValue(playerId, out thief);
        }

        public static bool IsThief(PlayerControl player)
        {
            return thiefs.ContainsKey(player.PlayerId);
        }

        public static void RemoveThief(byte playerId)
        {
            thiefs.Remove(playerId);
        }

        public static bool isFailedThiefKill(PlayerControl target, PlayerControl killer, RoleInfo targetRole)
        {
            return killer.IsThief(out _) && !target.isKiller() && !new List<RoleInfo> { canKillSheriff ? RoleInfo.sheriff : null, canKillVeteran ? RoleInfo.veteran : null }.Contains(targetRole);
        }

        public static void clearAndReload()
        {
            thiefs.Clear();
            formerThieves = new List<PlayerControl>();
            hasImpostorVision = CustomOptionHolder.thiefHasImpVision.getBool();
            cooldown = CustomOptionHolder.thiefCooldown.getFloat();
            canUseVents = CustomOptionHolder.thiefCanUseVents.getBool();
            canKillSheriff = CustomOptionHolder.thiefCanKillSheriff.getBool();
            canKillVeteran = CustomOptionHolder.thiefCanKillVeteran.getBool();
            canStealWithGuess = CustomOptionHolder.thiefCanStealWithGuess.getBool();
        }
    }

    public static class Lawyer
    {
        public static PlayerControl lawyer;
        public static PlayerControl target;
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);

        public static bool targetWasGuessed = false;

        public static float vision = 1f;
        public static bool lawyerKnowsRole = false;
        public static bool targetCanBeJester = false;
        public static bool canCallEmergency = true;
        public static bool targetKnows = false;
        public static BecomeOptions becomeOnTargetDeath = BecomeOptions.Amnesiac;

        public static void clearAndReload(bool clearTarget = true)
        {
            lawyer = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            vision = CustomOptionHolder.lawyerVision.getFloat();
            lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
            targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
            canCallEmergency = CustomOptionHolder.lawyerCanCallEmergency.getBool();
            targetKnows = CustomOptionHolder.lawyerTargetKnows.getBool();
            becomeOnTargetDeath = (BecomeOptions)CustomOptionHolder.lawyerBecomeOnTargetDeath.getSelection();
        }

        public enum BecomeOptions { Amnesiac, Pursuer, Survivor, Thief, Crewmate }
    }

    public static class Executioner
    {
        public static PlayerControl executioner;
        public static PlayerControl target;
        public static Color color = new Color(0.55f, 0.25f, 0.02f, 1f);

        public static bool targetWasGuessed = false;
        public static bool triggerExecutionerWin = false;
        public static bool triggerExile = false;

        public static float vision = 1f;
        public static bool executionerKnowsRole = false;
        public static bool canCallEmergency = true;
        public static BecomeOptions becomeOnTargetDeath = BecomeOptions.Amnesiac;

        public static void clearAndReload(bool clearTarget = true)
        {
            executioner = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            triggerExecutionerWin = false;
            vision = CustomOptionHolder.executionerVision.getFloat();
            executionerKnowsRole = CustomOptionHolder.executionerKnowsRole.getBool();
            canCallEmergency = CustomOptionHolder.executionerCanCallEmergency.getBool();
            becomeOnTargetDeath = (BecomeOptions)CustomOptionHolder.executionerBecomeOnTargetDeath.getSelection();
        }

        public enum BecomeOptions { Amnesiac, Pursuer, Survivor, Thief, Crewmate }
    }

    public static class Medic
    {
        public static PlayerControl medic;
        public static PlayerControl shielded;
        public static PlayerControl exShielded;
        public static PlayerControl currentTarget;
        public static PlayerControl futureShielded;
        public static Color color = new Color(0f, 0.4f, 0f, 1f);
        public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);

        public static bool usedShield;

        public static int showShielded = 0;
        public static int getsNotification = 0;
        public static bool setShieldAfterMeeting = false;
        public static bool showShieldAfterMeeting = false;
        public static bool meetingAfterShielding = false;
        public static bool unbreakableShield = true;
        public static bool reportInfo = false;
        public static float reportNameDuration = 0f;
        public static float reportColorDuration = 20f;

        public static bool shieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
            bool isMorphedGlitch = target == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
            bool isMorphedVenerer = target == Venerer.venerer && Venerer.abilityTimer > 0f;

            if (shielded != null && ((target == shielded && !isMorphedMorphling && !isMorphedVenerer && !isMorphedGlitch) || (isMorphedMorphling && Morphling.morphTarget == shielded) || (isMorphedGlitch && Glitch.morphPlayer == shielded)))
            {
                hasVisibleShield = showShielded == 0
                    || (showShielded == 1 && (PlayerControl.LocalPlayer == medic || PlayerControl.LocalPlayer == shielded))
                    || (showShielded == 2 && PlayerControl.LocalPlayer == medic)
                    || (showShielded == 3 && PlayerControl.LocalPlayer == shielded);
                hasVisibleShield = hasVisibleShield && (meetingAfterShielding || !showShieldAfterMeeting || PlayerControl.LocalPlayer == medic || Helpers.shouldShowGhostInfo());
            }
            return hasVisibleShield;
        }

        public static bool exShieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
            bool isMorphedGlitch = target == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
            bool isMorphedVenerer = target == Venerer.venerer && Venerer.abilityTimer > 0f;

            if (exShielded != null && ((target == exShielded && !isMorphedMorphling && !isMorphedVenerer && !isMorphedGlitch) || (isMorphedMorphling && Morphling.morphTarget == exShielded) || (isMorphedGlitch && Glitch.morphPlayer == exShielded)))
            {
                hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo()
                    || (showShielded == 1 && (PlayerControl.LocalPlayer == medic || PlayerControl.LocalPlayer == exShielded))
                    || (showShielded == 2 && PlayerControl.LocalPlayer == medic)
                    || (showShielded == 3 && PlayerControl.LocalPlayer == exShielded);
            }
            return hasVisibleShield;
        }

        public static void clearAndReload(bool clearExShielded = true)
        {
            medic = null;
            shielded = null;
            futureShielded = null;
            currentTarget = null;
            usedShield = false;
            if (clearExShielded) exShielded = null;
            showShielded = CustomOptionHolder.medicShowShielded.getSelection();
            getsNotification = CustomOptionHolder.medicGetsNotification.getSelection();
            setShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 2;
            showShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 1;
            unbreakableShield = CustomOptionHolder.medicShieldUnbreakable.getBool();
            reportInfo = CustomOptionHolder.medicInfoReport.getBool();
            reportNameDuration = CustomOptionHolder.medicReportNameDuration.getFloat();
            reportColorDuration = CustomOptionHolder.medicReportColorDuration.getFloat();
            meetingAfterShielding = false;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Swapper
    {
        public static PlayerControl swapper;
        public static Color color = new Color(0.4f, 0.9f, 0.4f, 1f);

        public static byte playerId1 = Byte.MaxValue;
        public static byte playerId2 = Byte.MaxValue;

        public static bool canCallEmergency = false;
        public static bool canOnlySwapOthers = false;
        public static int charges;
        public static float rechargeTasksNumber;
        public static float rechargedTasks;

        public static void clearAndReload()
        {
            swapper = null;
            playerId1 = Byte.MaxValue;
            playerId2 = Byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
            canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
            charges = CustomOptionHolder.swapperSwapsNumber.getInt();
            rechargeTasksNumber = CustomOptionHolder.swapperRechargeTasksNumber.getFloat();
            rechargedTasks = CustomOptionHolder.swapperRechargeTasksNumber.getFloat();
        }

        private static Sprite spriteCheck;
        public static Sprite getCheckSprite()
        {
            if (spriteCheck) return spriteCheck;
            spriteCheck = Helpers.loadSpriteFromResources("TownOfUs.Resources.SwapperCheck.png", 150f);
            return spriteCheck;
        }
    }

    public static class Investigator
    {
        public static PlayerControl investigator;
        public static Color color = new Color(0f, 0.7f, 0.7f, 1f);

        public static float timer = 6.2f;

        public static float footprintIntervall = 1f;
        public static float footprintDuration = 1f;
        public static bool anonymousFootprints = false;

        public static void clearAndReload()
        {
            investigator = null;
            timer = 6.2f;
            anonymousFootprints = CustomOptionHolder.investigatorAnonymousFootprints.getBool();
            footprintIntervall = CustomOptionHolder.investigatorFootprintIntervall.getFloat();
            footprintDuration = CustomOptionHolder.investigatorFootprintDuration.getFloat();
        }
    }

    public static class Spy
    {
        public static PlayerControl spy;
        public static Minigame vitals = null;
        public static Minigame doorLog = null;
        public static Color color = new Color(0.8f, 0.64f, 0.8f, 1f);

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float toolsNumber = 5f;
        public static int rechargeTasksNumber = 2;
        public static int rechargedTasks = 2;
        public static int chargesVitals = 1;
        public static int chargesAdminTable = 1;
        public static bool cantMove = true;

        public static void clearAndReload()
        {
            spy = null;
            vitals = null;
            doorLog = null;
            adminSprite = null;
            cooldown = CustomOptionHolder.spyCooldown.getFloat();
            duration = CustomOptionHolder.spyDuration.getFloat();
            toolsNumber = CustomOptionHolder.spyToolsNumber.getFloat();
            rechargeTasksNumber = CustomOptionHolder.spyRechargeTasksNumber.getInt();
            rechargedTasks = CustomOptionHolder.spyRechargeTasksNumber.getInt();
            chargesVitals = CustomOptionHolder.spyToolsNumber.getInt() / 2;
            chargesAdminTable = CustomOptionHolder.spyToolsNumber.getInt() / 2;
            cantMove = CustomOptionHolder.spyNoMove.getBool();
        }

        private static Sprite vitalsSprite;
        public static Sprite getVitalsSprite()
        {
            if (vitalsSprite) return vitalsSprite;
            vitalsSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
            return vitalsSprite;
        }

        private static Sprite logSprite;
        public static Sprite getLogSprite()
        {
            if (logSprite) return logSprite;
            logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
            return logSprite;
        }

        private static Sprite adminSprite;
        public static Sprite getAdminSprite()
        {
            byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (Helpers.isSkeld() || mapId == 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
            else if (Helpers.isMira()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (Helpers.isAirship()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            else if (Helpers.isFungle()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton];  // Hacker can Access the Admin panel on Fungle
            adminSprite = button.Image;
            return adminSprite;
        }
    }

    public static class Vigilante
    {
        public static PlayerControl vigilante;
        public static Color color = new Color(1f, 1f, 0.6f, 1f);

        public static int numberOfKills = 0;
        public static bool multipleKill = false;
        public static bool canGuessCrewmateRoles = false;
        public static bool canGuessNeutralBenign = false;
        public static bool canGuessNeutralEvil = false;
        public static bool canGuessNeutralKilling = false;
        public static bool canKillsThroughShield = false;

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            int remainingShots = numberOfKills;
            if (vigilante != null && vigilante.PlayerId == playerId)
            {
                if (shoot) numberOfKills = Mathf.Max(0, numberOfKills - 1);
            }
            return remainingShots;
        }

        public static void clearAndReload()
        {
            vigilante = null;
            numberOfKills = CustomOptionHolder.vigilanteKills.getInt();
            multipleKill = CustomOptionHolder.vigilanteMultiKill.getBool();
            canGuessCrewmateRoles = CustomOptionHolder.vigilanteGuessCrewmateRoles.getBool();
            canGuessNeutralBenign = CustomOptionHolder.vigilanteGuessNeutralBenign.getBool();
            canGuessNeutralEvil = CustomOptionHolder.vigilanteGuessNeutralEvil.getBool();
            canGuessNeutralKilling = CustomOptionHolder.vigilanteGuessNeutralKilling.getBool();
            canKillsThroughShield = CustomOptionHolder.vigilanteKillsThroughShield.getBool();
        }
    }

    public static class Assassin
    {
        public static PlayerControl assassin;
        public static Color color = Palette.ImpostorRed;

        public static int numberOfKills = 0;
        public static bool multipleKill = false;
        public static bool canGuessNeutralBenign = false;
        public static bool canGuessNeutralEvil = false;
        public static bool canGuessNeutralKilling = false;
        public static bool canKillsThroughShield = false;
        public static bool canGuessImpostorRoles = false;
        public static bool cantGuessSnitch = false;

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            int remainingShots = numberOfKills;
            if (assassin != null && assassin.PlayerId == playerId)
            {
                if (shoot) numberOfKills = Mathf.Max(0, numberOfKills - 1);
            }
            return remainingShots;
        }

        public static void clearAndReload()
        {
            assassin = null;
            numberOfKills = CustomOptionHolder.assassinKills.getInt();
            multipleKill = CustomOptionHolder.assassinMultiKill.getBool();
            canGuessNeutralBenign = CustomOptionHolder.assassinGuessNeutralBenign.getBool();
            canGuessNeutralEvil = CustomOptionHolder.assassinGuessNeutralEvil.getBool();
            canGuessNeutralKilling = CustomOptionHolder.assassinGuessNeutralKilling.getBool();
            canKillsThroughShield = CustomOptionHolder.assassinKillsThroughShield.getBool();
            canGuessImpostorRoles = CustomOptionHolder.assassinGuessImpostorRoles.getBool();
            cantGuessSnitch = CustomOptionHolder.assassinCantGuessSnitch.getBool();
        }
    }

    public static class Guesser
    {
        public static bool isGuesser(byte playerId)
        {
            if (Vigilante.vigilante != null && Vigilante.vigilante.PlayerId == playerId) return true;
            if (Assassin.assassin != null && Assassin.assassin.PlayerId == playerId) return true;
            return false;
        }

        public static int remainingShots(byte playerId, bool shoot = false)
        {
            if (Vigilante.vigilante != null && Vigilante.vigilante.PlayerId == playerId) return Vigilante.remainingShots(playerId, shoot);
            if (Assassin.assassin != null && Assassin.assassin.PlayerId == playerId) return Assassin.remainingShots(playerId, shoot);
            return 0;
        }

        public static void clearAndReload()
        {
            Vigilante.clearAndReload();
            Assassin.clearAndReload();
        }
    }

    public static class Tracker
    {
        public static PlayerControl tracker;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> trackedPlayers = new List<PlayerControl>();
        public static readonly Dictionary<byte, Arrow> TrackedPlayerLocalArrows = new();
        public static Color color = new Color(0f, 0.6f, 0f, 1f);

        public static float timeUntilUpdate = 0f;

        public static float cooldown = 30f;
        public static int numberOfTracks = 3;
        public static float updateIntervall = 5f;
        public static bool resetTargetAfterMeeting = false;

        public static void resetTracked()
        {
            foreach (Arrow arrow in TrackedPlayerLocalArrows.Values)
                if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
            numberOfTracks = CustomOptionHolder.trackerNumberOfTracks.getInt();
            TrackedPlayerLocalArrows.Clear();
            trackedPlayers = new List<PlayerControl>();
        }

        public static void clearAndReload()
        {
            tracker = null;
            currentTarget = null;
            timeUntilUpdate = 0f;
            resetTracked();
            cooldown = CustomOptionHolder.trackerCooldown.getFloat();
            numberOfTracks = CustomOptionHolder.trackerNumberOfTracks.getInt();
            updateIntervall = CustomOptionHolder.trackerUpdateIntervall.getFloat();
            resetTargetAfterMeeting = CustomOptionHolder.trackerResetTargetAfterMeeting.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.TrackerButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Trapper
    {
        public static PlayerControl trapper;
        public static List<Trap> traps = new List<Trap>();
        public static List<RoleInfo> trappedPlayers = new List<RoleInfo>();
        public static Material trapMaterial = TownOfUsPlugin.bundledAssets.Get<Material>("trap");
        public static Color color = new Color(0.65f, 0.82f, 0.7f, 1f);

        public static float cooldown = 30f;
        public static bool removeOnNewRound = false;
        public static int maxTraps = 3;
        public static float timeInTrap = 1f;
        public static float trapSize = 0.25f;
        public static int playersInTrap = 3;

        public static void clearAndReload()
        {
            trapper = null;
            traps.ClearTraps();
            traps = new List<Trap>();
            trappedPlayers = new List<RoleInfo>();
            cooldown = CustomOptionHolder.trapperTrapCooldown.getFloat();
            removeOnNewRound = CustomOptionHolder.trapperTrapsRemoveOnNewRound.getBool();
            maxTraps = CustomOptionHolder.trapperMaxTraps.getInt();
            timeInTrap = CustomOptionHolder.trapperMinAmountOfTimeInTrap.getFloat();
            trapSize = CustomOptionHolder.trapperTrapSize.getFloat();
            playersInTrap = CustomOptionHolder.trapperMinAmountOfPlayersInTrap.getInt();
        }

        private static Sprite trapButtonSprite;
        public static Sprite getButtonSprite()
        {
            if (trapButtonSprite) return trapButtonSprite;
            trapButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.Trapper_Place_Button.png", 115f);
            return trapButtonSprite;
        }
    }

    public static class Detective
    {
        public static PlayerControl detective;
        public static PlayerControl currentTarget;
        public static DeadBody currentDeadBodyTarget;
        public static PlayerControl currentDetectedKiller;
        public static Color color = new Color(0.3f, 0.3f, 1f, 1f);

        public static float cooldown = 30f;
        public static bool reportInfo = false;
        public static float reportRoleDuration = 15f;
        public static float reportFactionDuration = 30f;

        public static void clearAndReload()
        {
            detective = null;
            currentTarget = null;
            currentDeadBodyTarget = null;
            currentDetectedKiller = null;
            cooldown = CustomOptionHolder.detectiveCooldown.getFloat();
            reportInfo = CustomOptionHolder.detectiveReportInfo.getBool();
            reportRoleDuration = CustomOptionHolder.detectiveReportRoleDuration.getFloat();
            reportFactionDuration = CustomOptionHolder.detectiveReportFactionDuration.getFloat();
        }

        private static Sprite examineButtonSprite;
        public static Sprite getExamineButtonSprite()
        {
            if (examineButtonSprite) return examineButtonSprite;
            examineButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ExamineButton.png", 100f);
            return examineButtonSprite;
        }

        private static Sprite inspectButtonSprite;
        public static Sprite getInspectButtonSprite()
        {
            if (inspectButtonSprite) return inspectButtonSprite;
            inspectButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.InspectButton.png", 100f);
            return inspectButtonSprite;
        }
    }

    public static class Mystic
    {
        public static PlayerControl mystic;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.3f, 0.6f, 0.9f, 1f);

        public static float duration = 0.5f;

        public static void clearAndReload()
        {
            mystic = null;
            duration = CustomOptionHolder.mysticArrowDuration.getFloat();
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
        }
    }

    public static class GuardianAngel
    {
        public static PlayerControl guardianAngel;
        public static PlayerControl target;
        public static Color color = new Color(0.7f, 1f, 1f, 1f);

        public static bool isProtectActive = false;
        public static bool targetWasGuessed = false;

        public static float vision = 1f;
        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float cooldownReset = 30f;
        public static int numberOfProtects = 5;
        public static int showProtected = 0;
        public static bool gaKnowsRole = false;
        public static bool canCallEmergency = true;
        public static bool targetKnows = false;
        public static BecomeOptions becomeOnTargetDeath = BecomeOptions.Amnesiac;

        public static bool protectVisible(PlayerControl player)
        {
            bool hasVisibleShield = false;
            bool isMorphedMorphling = player == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
            bool isMorphedGlitch = player == Glitch.glitch && Glitch.morphPlayer != null && Glitch.morphTimer > 0f;
            bool isMorphedVenerer = target == Venerer.venerer && Venerer.abilityTimer > 0f;

            if (isProtectActive && target != null && ((player == target && !isMorphedMorphling && !isMorphedVenerer && !isMorphedGlitch) || (isMorphedMorphling && Morphling.morphTarget == target) || (isMorphedGlitch && Glitch.morphPlayer == target)))
            {
                hasVisibleShield = showProtected == 0 || Helpers.shouldShowGhostInfo()
                    || (showProtected == 1 && (PlayerControl.LocalPlayer == guardianAngel || PlayerControl.LocalPlayer == target))
                    || (showProtected == 2 && PlayerControl.LocalPlayer == guardianAngel)
                    || (showProtected == 3 && PlayerControl.LocalPlayer == target);
            }
            return hasVisibleShield;
        }

        public static void clearAndReload(bool clearTarget = true)
        {
            guardianAngel = null;
            if (clearTarget)
            {
                target = null;
                targetWasGuessed = false;
            }
            vision = CustomOptionHolder.guardianAngelVision.getFloat();
            cooldown = CustomOptionHolder.guardianAngelCooldown.getFloat();
            duration = CustomOptionHolder.guardianAngelDuration.getFloat();
            cooldownReset = CustomOptionHolder.guardianAngelCooldownReset.getFloat();
            numberOfProtects = CustomOptionHolder.guardianAngelNumberOfProtects.getInt();
            showProtected = CustomOptionHolder.guardianAngelShowProtected.getSelection();
            gaKnowsRole = CustomOptionHolder.guardianAngelKnowsRole.getBool();
            canCallEmergency = CustomOptionHolder.guardianAngelCanCallEmergency.getBool();
            targetKnows = CustomOptionHolder.guardianAngelTargetKnows.getBool();
            becomeOnTargetDeath = (BecomeOptions)CustomOptionHolder.guardianAngelBecomeOnTargetDeath.getSelection();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ProtectButton.png", 100f);
            return buttonSprite;
        }

        public enum BecomeOptions { Amnesiac, Pursuer, Survivor, Thief, Crewmate }
    }

    public static class Swooper
    {
        public static PlayerControl swooper;
        public static Color color = Palette.ImpostorRed;

        public static bool isInvisible = false;
        public static float invisibleTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool canVent = true;

        public static void clearAndReload()
        {
            swooper = null;
            cooldown = CustomOptionHolder.swooperCooldown.getFloat();
            duration = CustomOptionHolder.swooperDuration.getFloat();
            canVent = CustomOptionHolder.swooperCanvent.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.SwoopButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Arsonist
    {
        public static PlayerControl arsonist;
        public static PlayerControl currentDouseTarget;
        public static PlayerControl currentIgniteTarget;
        public static List<PlayerControl> dousedPlayers = new List<PlayerControl>();
        public static Color color = new Color(1f, 0.3f, 0f);

        public static float douseCooldown = 30f;
        public static float igniteCooldown = 30f;
        public static bool triggerBothCooldowns = true;
        public static bool hasImpostorVision = true;
        public static bool canVent = true;

        public static void clearAndReload()
        {
            arsonist = null;
            currentDouseTarget = null;
            currentIgniteTarget = null;
            dousedPlayers = new List<PlayerControl>();
            douseCooldown = CustomOptionHolder.arsonistDouseCooldown.getFloat();
            igniteCooldown = CustomOptionHolder.arsonistIgniteCooldown.getFloat();
            triggerBothCooldowns = CustomOptionHolder.arsonistTriggerBoth.getBool();
            hasImpostorVision = CustomOptionHolder.arsonistHasImpostorVision.getBool();
            canVent = CustomOptionHolder.arsonistCanvent.getBool();
        }

        private static Sprite douseButtonSprite;
        public static Sprite getDouseButtonSprite()
        {
            if (douseButtonSprite) return douseButtonSprite;
            douseButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DouseButton.png", 100f);
            return douseButtonSprite;
        }

        private static Sprite igniteButtonSprite;
        public static Sprite getIgniteButtonSprite()
        {
            if (igniteButtonSprite) return igniteButtonSprite;
            igniteButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.IgniteButton.png", 100f);
            return igniteButtonSprite;
        }
    }

    public static class Werewolf
    {
        public static PlayerControl werewolf;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.66f, 0.4f, 0.16f, 1f);

        public static bool isRampageActive = false;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float killCooldown = 3f;
        public static bool hasImpostorVision = false;
        public static bool canVent = false;

        public static void clearAndReload()
        {
            werewolf = null;
            currentTarget = null;
            isRampageActive = false;
            cooldown = CustomOptionHolder.werewolfRampageCooldown.getFloat();
            duration = CustomOptionHolder.werewolfRampageDuration.getFloat();
            killCooldown = CustomOptionHolder.werewolfRampageKillCooldown.getFloat();
            hasImpostorVision = CustomOptionHolder.werewolfHasImpostorVision.getBool();
            canVent = CustomOptionHolder.werewolfCanVent.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.RampageButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Miner
    {
        public static PlayerControl miner;
        public static Color color = Palette.ImpostorRed;

        public static float placeVentCooldown = 30f;

        public static void clearAndReload()
        {
            miner = null;
            placeVentCooldown = CustomOptionHolder.minerPlaceVentCooldown.getFloat();
            MinerVent.UpdateStates();
        }

        private static Sprite placeVentButtonSprite;
        public static Sprite getPlaceBoxButtonSprite()
        {
            if (placeVentButtonSprite) return placeVentButtonSprite;
            placeVentButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.PlaceVentButton.png", 100f);
            return placeVentButtonSprite;
        }
    }

    public static class Janitor
    {
        public static PlayerControl janitor;
        public static DeadBody currentTarget;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;

        public static void clearAndReload()
        {
            janitor = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.janitorCooldown.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Undertaker
    {
        public static PlayerControl undertaker;
        public static DeadBody dragedBody;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;
        public static float velocity = 1f;
        public static bool canVent = true;
        public static bool canDragAndVent = true;

        public static void clearAndReload()
        {
            undertaker = null;
            dragedBody = null;
            cooldown = CustomOptionHolder.undertakerDragCooldown.getFloat();
            velocity = CustomOptionHolder.undertakerDragingAfterVelocity.getFloat();
            canVent = CustomOptionHolder.undertakerCanVent.getBool();
            canDragAndVent = CustomOptionHolder.undertakerCanDragAndVent.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getDragButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DragButton.png", 100f);
            return buttonSprite;
        }

        private static Sprite dropButtonSprite;
        public static Sprite getDropButtonSprite()
        {
            if (dropButtonSprite) return dropButtonSprite;
            dropButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DropButton.png", 100f);
            return dropButtonSprite;
        }
    }

    public static class Grenadier
    {
        public static PlayerControl grenadier;
        public static Color color = Palette.ImpostorRed;
        public static Color flash = new Color32(153, 153, 153, byte.MaxValue);
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> flashedPlayers = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

        public static float flashTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 8f;
        public static int radius = 0;
        public static bool indicateFlashedCrewmates = false;

        public static void clearAndReload()
        {
            grenadier = null;
            flashTimer = 0f;
            flashedPlayers = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            cooldown = CustomOptionHolder.grenadierCooldown.getFloat();
            duration = CustomOptionHolder.grenadierDuration.getFloat() + 0.5f;
            radius = CustomOptionHolder.grenadierFlashRadius.getSelection();
            indicateFlashedCrewmates = CustomOptionHolder.grenadierIndicateCrewmates.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.FlashButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Blackmailer
    {
        public static PlayerControl blackmailer;
        public static PlayerControl blackmailed;
        public static PlayerControl currentTarget;
        public static Color color = Palette.ImpostorRed;

        public static bool alreadyShook = false;

        public static float cooldown = 30f;
        public static bool blockTargetVote = false;
        public static bool blockTargetAbility = false;

        public static void clearAndReload()
        {
            blackmailer = null;
            blackmailed = null;
            currentTarget = null;
            alreadyShook = false;
            cooldown = CustomOptionHolder.blackmailerCooldown.getFloat();
            blockTargetVote = CustomOptionHolder.blackmailerBlockTargetVote.getBool();
            blockTargetAbility = CustomOptionHolder.blackmailerBlockTargetAbility.getBool();
        }

        private static Sprite overlaySprite;
        public static Sprite getBlackmailOverlaySprite()
        {
            if (overlaySprite) return overlaySprite;
            overlaySprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailOverlay.png", 100f);
            return overlaySprite;
        }

        private static Sprite blackmailButton;
        public static Sprite getBlackmailButtonSprite()
        {
            if (blackmailButton) return blackmailButton;
            blackmailButton = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlackmailButton.png", 100f);
            return blackmailButton;
        }
    }

    public static class Politician
    {
        public static PlayerControl politician;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> campaignedPlayers = new List<PlayerControl>();
        public static Color color = new Color(0.4f, 0f, 0.6f, 1f);

        public static float cooldown = 30f;

        public static void clearAndReload()
        {
            politician = null;
            currentTarget = null;
            campaignedPlayers = new List<PlayerControl>();
            cooldown = CustomOptionHolder.politicianCooldown.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.CampaignButton.png", 115);
            return buttonSprite;
        }
    }

    public static class Mayor
    {
        public static PlayerControl mayor;
        public static Color color = new Color(0.44f, 0.31f, 0.66f, 1f);

        public static int numberOfGuards = 0;
        public static bool isBodyguardActive = false;

        public static float cooldown = 30f;
        public static float duration = 10f;

        public static void clearAndReload()
        {
            mayor = null;
            numberOfGuards = 0;
            isBodyguardActive = false;
            cooldown = CustomOptionHolder.mayorCooldown.getFloat();
            duration = CustomOptionHolder.mayorDuration.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BodyguardButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Dracula
    {
        public static PlayerControl dracula;
        public static PlayerControl fakeVampire;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> formerDraculas = new List<PlayerControl>();
        public static Color color = new Color(0.15f, 0.15f, 0.15f, 1f);

        public static bool wasTeamRed;
        public static bool wasImpostor;
        public static int currentCreatedVampires = 0;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool hasImpostorVision = false;
        public static bool canCreateVampire = true;
        public static int maxNumOfVampires = 1;
        public static bool canCreateVampireFromNeutralBenign = true;
        public static bool canCreateVampireFromNeutralEvil = true;
        public static bool canCreateVampireFromNeutralKilling = true;
        public static bool canCreateVampireFromImpostor = true;

        public static void removeCurrentDracula()
        {
            if (!formerDraculas.Any(x => x.PlayerId == dracula.PlayerId)) formerDraculas.Add(dracula);
            dracula = null;
            currentTarget = null;
            fakeVampire = null;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
        }

        public static void clearAndReload()
        {
            dracula = null;
            fakeVampire = null;
            currentTarget = null;
            formerDraculas.Clear();
            formerDraculas = new List<PlayerControl>();
            currentCreatedVampires = 0;
            cooldown = CustomOptionHolder.draculaKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.draculaCanUseVents.getBool();
            hasImpostorVision = CustomOptionHolder.draculaHaveImpostorVision.getBool();
            canCreateVampire = CustomOptionHolder.draculaCanCreateVampire.getBool();
            maxNumOfVampires = CustomOptionHolder.draculaNumOfCreatedVampires.getInt();
            canCreateVampireFromNeutralBenign = CustomOptionHolder.draculaCanCreateVampireFromNeutralBenign.getBool();
            canCreateVampireFromNeutralEvil = CustomOptionHolder.draculaCanCreateVampireFromNeutralEvil.getBool();
            canCreateVampireFromNeutralKilling = CustomOptionHolder.draculaCanCreateVampireFromNeutralKilling.getBool();
            canCreateVampireFromImpostor = CustomOptionHolder.draculaCanCreateVampireFromImpostor.getBool();
            wasTeamRed = wasImpostor = false;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BiteButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Vampire
    {
        public static PlayerControl vampire;
        public static PlayerControl currentTarget;
        public static Color color = Dracula.color;

        public static bool wasTeamRed;
        public static bool wasImpostor;

        public static bool canKill = true;
        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload()
        {
            vampire = null;
            currentTarget = null;
            canKill = CustomOptionHolder.vampireCanKill.getBool();
            cooldown = CustomOptionHolder.vampireKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.vampireCanUseVents.getBool();
            hasImpostorVision = CustomOptionHolder.vampireHaveImpostorVision.getBool();
            wasTeamRed = wasImpostor = false;
        }
    }

    public static class Poisoner
    {
        public static PlayerControl poisoner;
        public static PlayerControl poisoned;
        public static PlayerControl currentTarget;
        public static List<byte> blindTrappedPlayers = new List<byte>();
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;
        public static float delay = 10f;
        public static float trapCooldown = 20f;
        public static float trapDuration = 10f;
        public static int charges = 1;
        public static bool canVent = false;

        public static void clearAndReload()
        {
            poisoner = null;
            poisoned = null;
            currentTarget = null;
            blindTrappedPlayers = new List<byte>();
            cooldown = CustomOptionHolder.poisonerCooldown.getFloat();
            delay = CustomOptionHolder.poisonerKillDelay.getFloat();
            trapCooldown = CustomOptionHolder.poisonerTrapCooldown.getFloat();
            trapDuration = CustomOptionHolder.poisonerTrapDuration.getFloat();
            charges = CustomOptionHolder.poisonerNumberOfTraps.getInt();
            canVent = CustomOptionHolder.poisonerCanVent.getBool();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.PoisonButton.png", 100f);
            return buttonSprite;
        }

        private static Sprite trapButtonSprite;
        public static Sprite getTrapButtonSprite()
        {
            if (trapButtonSprite) return trapButtonSprite;
            trapButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.Plant_BlindTraps.png", 115f);
            return trapButtonSprite;
        }
    }

    public static class Venerer
    {
        public static PlayerControl venerer;
        public static Color color = Palette.ImpostorRed;

        public static int numberOfKills = 0;
        public static float abilityTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float speedMultiplier = 1.25f;
        public static float freezeMultiplier = 0.5f;

        public static void resetMorph()
        {
            abilityTimer = 0f;
            if (venerer == null) return;
            venerer.setDefaultLook();
        }

        public static void clearAndReload()
        {
            resetMorph();
            venerer = null;
            numberOfKills = 0;
            abilityTimer = 0f;
            cooldown = CustomOptionHolder.venererCooldown.getFloat();
            duration = CustomOptionHolder.venererDuration.getFloat();
            speedMultiplier = CustomOptionHolder.venererSpeedMultiplier.getFloat();
            freezeMultiplier = CustomOptionHolder.venererFreezeMultiplier.getFloat();
        }

        private static Sprite noAbilitiesButton;
        public static Sprite getNoAbilitiesButton()
            => noAbilitiesButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.NoAbilitiesButton.png", 100f);

        private static Sprite camoButton;
        public static Sprite getCamoButton()
            => camoButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.CamouflageButton.png", 100f);

        private static Sprite speedButton;
        public static Sprite getSpeedButton()
            => speedButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.SpeedButton.png", 100f);

        private static Sprite freezeButton;
        public static Sprite getFreezeButton()
            => freezeButton ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.FreezeButton.png", 100f);
    }

    public static class Plaguebearer
    {
        public static PlayerControl plaguebearer;
        public static PlayerControl currentTarget;
        public static List<PlayerControl> infectedPlayers = new List<PlayerControl>();
        public static Color color = new Color(0.9f, 1f, 0.7f, 1f);

        public static float cooldown = 30f;

        public static void clearAndReload()
        {
            plaguebearer = null;
            currentTarget = null;
            infectedPlayers = new List<PlayerControl>();
            cooldown = CustomOptionHolder.plaguebearerCooldown.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.InfectButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Pestilence
    {
        public static PlayerControl pestilence;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.3f, 0.3f, 0.3f, 1f);

        public static float cooldown = 30f;
        public static bool hasImpostorVision = false;
        public static bool canVent = false;

        public static void clearAndReload()
        {
            pestilence = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.pestilenceKillCooldown.getFloat();
            hasImpostorVision = CustomOptionHolder.pestilenceHasImpostorVision.getBool();
            canVent = CustomOptionHolder.pestilenceCanVent.getBool();
        }
    }

    public static class Doomsayer
    {
        public static PlayerControl doomsayer;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0f, 1f, 0.5f, 1f);

        public static bool triggerGuessed = false;
        public static bool triggerDoomsayerWin = false;

        public static int guessedToWin = 0;
        public static bool canObserve = true;

        public static float cooldown = 30f;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool cantGuessSnitch = false;
        public static bool canKillsThroughShield = false;
        public static bool canGuessNeutralBenign = false;
        public static bool canGuessNeutralEvil = false;
        public static bool canGuessNeutralKilling = false;
        public static int guessesToWin = 3;
        public static bool canGuessImpostor = false;

        public static void clearAndReload()
        {
            doomsayer = null;
            currentTarget = null;
            triggerGuessed = false;
            triggerDoomsayerWin = false;
            guessedToWin = 0;
            canObserve = true;
            cooldown = CustomOptionHolder.doomsayerCooldown.getFloat();
            hasMultipleShotsPerMeeting = CustomOptionHolder.doomsayerHasMultipleShotsPerMeeting.getBool();
            cantGuessSnitch = CustomOptionHolder.doomsayerCantGuessSnitch.getBool();
            canKillsThroughShield = CustomOptionHolder.doomsayerCanKillsThroughShield.getBool();
            canGuessNeutralBenign = CustomOptionHolder.doomsayerCanGuessNeutralBenign.getBool();
            canGuessNeutralEvil = CustomOptionHolder.doomsayerCanGuessNeutralEvil.getBool();
            canGuessNeutralKilling = CustomOptionHolder.doomsayerCanGuessNeutralKilling.getBool();
            canGuessImpostor = CustomOptionHolder.doomsayerCanGuessImpostor.getBool();
            guessesToWin = CustomOptionHolder.doomsayerKillToWin.getInt();
        }

        public static string GetInfo(PlayerControl target)
        {
            try
            {
                var AllMessage = new List<string>();
                var crewRoleList = allRoleInfos().Where(x => x.factionId == FactionId.Crewmate).OrderBy(_ => rnd.Next()).ToList();
                var neutRoleList = allRoleInfos().Where(x => x.factionId == FactionId.NeutralBenign || x.factionId == FactionId.NeutralEvil || x.factionId == FactionId.NeutralKilling).OrderBy(_ => rnd.Next()).ToList();
                var impRoleList = allRoleInfos().Where(x => x.factionId == FactionId.Impostor).OrderBy(_ => rnd.Next()).ToList();

                var roleInfoTarget = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
                if (roleInfoTarget.factionId == FactionId.Crewmate) crewRoleList.Remove(roleInfoTarget);
                else if (roleInfoTarget.factionId == FactionId.NeutralBenign || roleInfoTarget.factionId == FactionId.NeutralEvil || roleInfoTarget.factionId == FactionId.NeutralKilling) neutRoleList.Remove(roleInfoTarget);
                else if (roleInfoTarget.factionId == FactionId.Impostor) impRoleList.Remove(roleInfoTarget);
                neutRoleList.Remove(RoleInfo.doomsayer);
                neutRoleList.Remove(RoleInfo.pestilence);
                crewRoleList.Remove(RoleInfo.mayor);

                var formation = 3;
                var x = rnd.Next(0, formation);
                var message = new StringBuilder();

                var tempCrewNumList = Enumerable.Range(0, crewRoleList.Count).ToList();
                var crewTemp = (tempCrewNumList.Count > formation ? tempCrewNumList.Take(formation) : tempCrewNumList).OrderBy(_ => rnd.Next()).ToList();

                var tempNeutNumList = Enumerable.Range(0, neutRoleList.Count).ToList();
                var neutTemp = (tempNeutNumList.Count > formation ? tempNeutNumList.Take(formation) : tempNeutNumList).OrderBy(_ => rnd.Next()).ToList();

                var tempImpNumList = Enumerable.Range(0, impRoleList.Count).ToList();
                var impTemp = (tempImpNumList.Count > formation ? tempImpNumList.Take(formation) : tempImpNumList).OrderBy(_ => rnd.Next()).ToList();

                message.AppendLine($"{target?.Data?.PlayerName}'s Observe Info:\n");
                // Crewmate Roles
                for (int num = 0, tempCrewNum = 0; num < formation; num++, tempCrewNum++)
                {
                    var info = crewRoleList[crewTemp[tempCrewNum]];

                    message.Append(roleInfoTarget.factionId == FactionId.Crewmate ? (num == x ? roleInfoTarget.name : info.name) : info.name);
                    message.Append(", ");
                }
                // Neutral Roles
                for (int num = 0, tempNeutNum = 0; num < formation; num++, tempNeutNum++)
                {
                    var info = neutRoleList[neutTemp[tempNeutNum]];

                    message.Append((roleInfoTarget.factionId == FactionId.NeutralBenign || roleInfoTarget.factionId == FactionId.NeutralEvil || roleInfoTarget.factionId == FactionId.NeutralKilling) ? (num == x ? roleInfoTarget.name : info.name) : info.name);
                    message.Append(", ");
                }
                // Impostor Roles
                for (int num = 0, tempImpNum = 0; num < formation; num++, tempImpNum++)
                {
                    var info = impRoleList[impTemp[tempImpNum]];

                    message.Append(roleInfoTarget.factionId == FactionId.Impostor ? (num == x ? roleInfoTarget.name : info.name) : info.name);
                    message.Append(", ");
                }
                message.Remove(message.Length - 2, 2);
                AllMessage.Add(message.ToString());

                return $"{message}";
            }
            catch { return "Doomsayer Error"; }
        }

        public static List<RoleInfo> allRoleInfos()
        {
            var allRoleInfo = new List<RoleInfo>();
            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            foreach (var role in RoleInfo.allRoleInfos)
            {
                if (role.factionId == FactionId.Modifier || role.factionId == FactionId.Ghost || role.roleId == RoleId.Mayor || role.roleId == RoleId.Pestilence) continue;
                else if (roleData.crewSettings.ContainsKey((byte)role.roleId) && roleData.crewSettings[(byte)role.roleId] == 0) continue;
                else if ((role.roleId != RoleId.Amnesiac || role.roleId != RoleId.Survivor || role.roleId != RoleId.Pursuer || role.roleId != RoleId.Thief) && roleData.neutralSettings.ContainsKey((byte)role.roleId) && roleData.neutralSettings[(byte)role.roleId] == 0) continue;
                else if (roleData.neutralKillingSettings.ContainsKey((byte)role.roleId) && roleData.neutralKillingSettings[(byte)role.roleId] == 0) continue;
                else if (roleData.impSettings.ContainsKey((byte)role.roleId) && roleData.impSettings[(byte)role.roleId] == 0) continue;
                else if (role.roleId == RoleId.Amnesiac)
                {
                    if (CustomOptionHolder.amnesiacSpawnRate.getSelection() == 0 && Lawyer.becomeOnTargetDeath != Lawyer.BecomeOptions.Amnesiac && Executioner.becomeOnTargetDeath != Executioner.BecomeOptions.Amnesiac && GuardianAngel.becomeOnTargetDeath != GuardianAngel.BecomeOptions.Amnesiac) continue;
                }
                else if (role.roleId == RoleId.Survivor)
                {
                    if (CustomOptionHolder.survivorSpawnRate.getSelection() == 0 && Lawyer.becomeOnTargetDeath != Lawyer.BecomeOptions.Survivor && Executioner.becomeOnTargetDeath != Executioner.BecomeOptions.Survivor && GuardianAngel.becomeOnTargetDeath != GuardianAngel.BecomeOptions.Survivor) continue;
                }
                else if (role.roleId == RoleId.Pursuer)
                {
                    if (CustomOptionHolder.pursuerSpawnRate.getSelection() == 0 && Lawyer.becomeOnTargetDeath != Lawyer.BecomeOptions.Pursuer && Executioner.becomeOnTargetDeath != Executioner.BecomeOptions.Pursuer && GuardianAngel.becomeOnTargetDeath != GuardianAngel.BecomeOptions.Pursuer) continue;
                }
                else if (role.roleId == RoleId.Thief)
                {
                    if (CustomOptionHolder.thiefSpawnRate.getSelection() == 0 && Lawyer.becomeOnTargetDeath != Lawyer.BecomeOptions.Thief && Executioner.becomeOnTargetDeath != Executioner.BecomeOptions.Thief && GuardianAngel.becomeOnTargetDeath != GuardianAngel.BecomeOptions.Thief) continue;
                }
                else if (HandleGuesser.isGuesserGm && (role.roleId == RoleId.Vigilante || role.roleId == RoleId.Assassin)) continue;
                allRoleInfo.Add(role);
            }
            return allRoleInfo;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ObserveButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Glitch
    {
        public static PlayerControl glitch;
        public static PlayerControl morphPlayer;
        public static PlayerControl hackedPlayer;
        public static PlayerControl sampledPlayer;
        public static PlayerControl currentTarget;
        public static List<byte> mimicTargets = new List<byte>();
        public static Dictionary<PlayerControl, DateTime> hackedPlayers = new Dictionary<PlayerControl, DateTime>();
        public static Color color = Color.green;

        public static float morphTimer = 0f;

        public static float morphCooldown = 30f;
        public static float morphDuration = 10f;
        public static float hackCooldown = 30f;
        public static float hackDuration = 10f;
        public static float killCooldown = 30f;
        public static bool canVent = false;
        public static bool hasImpostorVision = false;

        public static void resetMorph()
        {
            morphPlayer = null;
            morphTimer = 0f;
            if (glitch == null) return;
            glitch.setDefaultLook();
        }

        public static void clearAndReload()
        {
            resetMorph();
            glitch = null;
            morphPlayer = null;
            hackedPlayer = null;
            sampledPlayer = null;
            currentTarget = null;
            morphTimer = 0f;
            mimicTargets = new List<byte>();
            hackedPlayers = new Dictionary<PlayerControl, DateTime>();
            morphCooldown = CustomOptionHolder.glitchMimicCooldown.getFloat();
            morphDuration = CustomOptionHolder.glitchMimicDuration.getFloat();
            hackCooldown = CustomOptionHolder.glitchHackCooldown.getFloat();
            hackDuration = CustomOptionHolder.glitchHackDuration.getFloat();
            killCooldown = CustomOptionHolder.glitchKillCooldown.getFloat();
            canVent = CustomOptionHolder.glitchCanVent.getBool();
            hasImpostorVision = CustomOptionHolder.glitchHasImpostorVision.getBool();
        }

        private static Sprite hackButtonSprite;
        public static Sprite getHackButtonSprite()
            => hackButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.HackButton.png", 100f);

        private static Sprite mimicButtonSprite;
        public static Sprite getMimicButtonSprite()
            => mimicButtonSprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MimicButton.png", 100f);

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
                var rolePrefab = RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == RoleTypes.Shapeshifter);
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

    public static class Cannibal
    {
        public static PlayerControl cannibal;
        public static DeadBody currentTarget;
        public static List<Arrow> localArrows = new List<Arrow>();
        public static Color color = new Color(0.3f, 0.48f, 0.11f, 1f);

        public static int eatenBodies = 0;
        public static bool triggerEaten = false;
        public static bool triggerCannibalWin = false;

        public static float cooldown = 30f;
        public static int cannibalNumberToWin = 4;
        public static bool canVent = true;
        public static bool showArrows = true;
        public static bool canEatWithGuess = true;

        public static void clearAndReload()
        {
            cannibal = null;
            currentTarget = null;
            eatenBodies = 0;
            triggerEaten = false;
            triggerCannibalWin = false;
            cooldown = CustomOptionHolder.cannibalCooldown.getFloat();
            cannibalNumberToWin = CustomOptionHolder.cannibalNumberToWin.getInt();
            canVent = CustomOptionHolder.cannibalCanUseVents.getBool();
            showArrows = CustomOptionHolder.cannibalShowArrows.getBool();
            canEatWithGuess = CustomOptionHolder.cannibalEatWithGuess.getBool();
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.EatButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Scavenger
    {
        public static PlayerControl scavenger;
        public static PlayerControl bounty;
        public static Arrow arrow;
        public static Color color = Palette.ImpostorRed;

        public static float arrowUpdateTimer = 0f;
        public static float bountyUpdateTimer = 0f;
        public static TMPro.TextMeshPro cooldownText;

        public static float bountyDuration = 30f;
        public static float bountyKillCooldown = 0f;
        public static float punishmentTime = 15f;
        public static bool showArrow = true;
        public static float arrowUpdateIntervall = 10f;

        public static void clearAndReload()
        {
            arrow = new Arrow(color);
            scavenger = null;
            bounty = null;
            arrowUpdateTimer = 0f;
            bountyUpdateTimer = 0f;
            if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = null;
            if (cooldownText != null && cooldownText.gameObject != null) UnityEngine.Object.Destroy(cooldownText.gameObject);
            cooldownText = null;
            foreach (PoolablePlayer p in playerIcons.Values)
            {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            bountyDuration = CustomOptionHolder.scavengerBountyDuration.getFloat();
            bountyKillCooldown = CustomOptionHolder.scavengerReducedCooldown.getFloat();
            punishmentTime = CustomOptionHolder.scavengerPunishmentTime.getFloat();
            showArrow = CustomOptionHolder.scavengerShowArrow.getBool();
            arrowUpdateIntervall = CustomOptionHolder.scavengerArrowUpdateIntervall.getFloat();
        }
    }

    public static class Escapist
    {
        public static PlayerControl escapist;
        public static Color color = Palette.ImpostorRed;

        public static Vector3 jumpLocation;

        public static float cooldown = 30f;
        public static bool canVent = true;

        public static void clearAndReload()
        {
            escapist = null;
            jumpLocation = Vector3.zero;
            cooldown = CustomOptionHolder.escapistCooldown.getFloat();
            canVent = CustomOptionHolder.escapistCanVent.getBool();
        }

        private static Sprite markButtonSprite;
        public static Sprite getMarkButtonSprite()
        {
            if (markButtonSprite) return markButtonSprite;
            markButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.MarkButton.png", 100f);
            return markButtonSprite;
        }

        private static Sprite escapeButtonSprite;
        public static Sprite getEscapeButtonSprite()
        {
            if (escapeButtonSprite) return escapeButtonSprite;
            escapeButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.EscapeButton.png", 100f);
            return escapeButtonSprite;
        }
    }

    public static class VampireHunter
    {
        public static PlayerControl vampireHunter;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.7f, 0.7f, 0.9f, 1f);

        public static bool canStake = true;
        public static int failedStakes = 0;

        public static float cooldown = 30f;
        public static int maxFailedStakes = 3;
        public static bool canStakeRoundOne = false;
        public static bool selfKillOnFailedStakes = false;
        public static BecomeOptions becomeOnVampiresDeath = BecomeOptions.Sheriff;

        public static void clearAndReload()
        {
            vampireHunter = null;
            currentTarget = null;
            canStake = true;
            failedStakes = 0;
            cooldown = CustomOptionHolder.vampireHunterCooldown.getFloat();
            maxFailedStakes = CustomOptionHolder.vampireHunterMaxFailedStakes.getInt();
            canStakeRoundOne = CustomOptionHolder.vampireHunterCanStakeRoundOne.getBool();
            selfKillOnFailedStakes = CustomOptionHolder.vampireHunterSelfKillOnFailStakes.getBool();
            becomeOnVampiresDeath = (BecomeOptions)CustomOptionHolder.vampireHunterBecome.getSelection();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.StakeButton.png", 100f);
            return buttonSprite;
        }

        public enum BecomeOptions { Sheriff, Veteran, Crewmate }
    }

    public static class Oracle
    {
        public static PlayerControl oracle;
        public static PlayerControl confessor;
        public static FactionId revealedFactionId;
        public static PlayerControl currentTarget;
        public static Color color = new Color(0.75f, 0f, 0.75f, 1f);

        public static float accuracy = 0f;
        public static float cooldown = 30f;
        public static bool benignNeutralsShowsEvil = false;
        public static bool evilNeutralsShowsEvil = false;
        public static bool killingNeutralsShowsEvil = false;

        public static void clearAndReload()
        {
            oracle = null;
            confessor = null;
            currentTarget = null;
            revealedFactionId = FactionId.Crewmate;
            accuracy = CustomOptionHolder.oracleRevealAccuracy.getSelection() * 10;
            cooldown = CustomOptionHolder.oracleConfessCooldown.getFloat();
            benignNeutralsShowsEvil = CustomOptionHolder.oracleBenignNeutralsShowsEvil.getBool();
            evilNeutralsShowsEvil = CustomOptionHolder.oracleEvilNeutralsShowsEvil.getBool();
            killingNeutralsShowsEvil = CustomOptionHolder.oracleKillingNeutralsShowsEvil.getBool();
        }

        public static string GetInfo(PlayerControl target)
        {
            var evilPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && (x.Data.Role.IsImpostor || x.isNeutralBenign() && benignNeutralsShowsEvil || x.isNeutralEvil() && evilNeutralsShowsEvil || x.isNeutralKilling() && killingNeutralsShowsEvil)).ToList();
            var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && x != oracle && x != target).ToList();

            if (target.Data.IsDead || target.Data.Disconnected) return "Your confessor failed to survive so you received no confession";
            else if (allPlayers.Count < 2) return "Too few people alive to receive a confessional";
            else if (evilPlayers.Count == 0) return $"{target.Data.PlayerName} confesses to knowing that there are no more evil players!";

            allPlayers.Shuffle();
            evilPlayers.Shuffle();

            var secondPlayer = allPlayers[0];
            var firstTwoEvil = false;

            foreach (var evilPlayer in evilPlayers)
            {
                if (evilPlayer == target || evilPlayer == secondPlayer) firstTwoEvil = true;
            }

            if (firstTwoEvil)
            {
                var thirdPlayer = allPlayers[1];
                return $"{target.Data.PlayerName} confesses to knowing that they, {secondPlayer.Data.PlayerName} and/or {thirdPlayer.Data.PlayerName} is evil!";
            }
            else if (!firstTwoEvil)
            {
                var thirdPlayer = evilPlayers[0];
                return $"{target.Data.PlayerName} confesses to knowing that they, {secondPlayer.Data.PlayerName} and/or {thirdPlayer.Data.PlayerName} is evil!";
            }

            return "Oracle Error";
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ConfessButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Lookout
    {
        public static PlayerControl lookout;
        public static Color color = new Color(0.2f, 1f, 0.4f, 1f);

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float orthographicSize = 4f;
        public static int remainingZooms = 3;
        public static bool canRecharge = false;
        public static float rechargeTasksNumber;
        public static float rechargedTasks;

        public static void clearAndReload()
        {
            lookout = null;
            zoomOutStatus = false;
            cooldown = CustomOptionHolder.lookoutCooldown.getFloat();
            duration = CustomOptionHolder.lookoutDuration.getFloat();
            orthographicSize = CustomOptionHolder.lookoutOrthographicSize.getFloat();
            remainingZooms = CustomOptionHolder.lookoutRemainingZooms.getInt();
            canRecharge = CustomOptionHolder.lookoutCanRecharge.getBool();
            rechargeTasksNumber = CustomOptionHolder.lookoutRechargeTasksNumber.getFloat();
            rechargedTasks = CustomOptionHolder.lookoutRechargeTasksNumber.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ZoomoutButton.png", 115f);
            return buttonSprite;
        }

        public static bool zoomOutStatus = false;
        public static IEnumerator zoomOut()
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            for (var ft = Camera.main!.orthographicSize; ft < orthographicSize; ft += 0.3f)
            {
                Camera.main.orthographicSize = MeetingHud.Instance ? 3f : ft;
                ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
                foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main.orthographicSize;
                yield return null;
            }
            foreach (var cam in Camera.allCameras) cam.orthographicSize = orthographicSize;
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        }

        public static IEnumerator zoomIn()
        {
            for (var ft = Camera.main!.orthographicSize; ft > 3f; ft -= 0.3f)
            {
                Camera.main.orthographicSize = MeetingHud.Instance ? 3f : ft;
                ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
                foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main.orthographicSize;
                yield return null;
            }
            foreach (var cam in Camera.allCameras) cam.orthographicSize = 3f;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        }

        public static void toggleZoom(bool reset = false)
        {
            if (reset)
            {
                if (zoomOutStatus) Coroutines.Start(zoomIn());
                zoomOutStatus = false;
                return;
            }

            if (!zoomOutStatus)
            {
                Coroutines.Start(zoomOut());
                zoomOutStatus = true;
            }
            else
            {
                Coroutines.Start(zoomIn());
                zoomOutStatus = false;
            }
        }
    }

    public static class Plumber
    {
        public static PlayerControl plumber;
        public static Vent ventTarget = null;
        public static Color color = new Color(0.8f, 0.4f, 0f, 1f);

        public static float flushCooldown = 30f;
        public static int remainingFlushs = 3;
        public static bool canRecharge = false;
        public static float rechargeTasksNumber;
        public static float rechargedTasks;

        public static float sealVentCooldown = 30f;
        public static int remainingSeals = 3;

        public static void clearAndReload()
        {
            plumber = null;
            ventTarget = null;
            flushCooldown = CustomOptionHolder.plumberFlushCooldown.getFloat();
            remainingFlushs = CustomOptionHolder.plumberNumberOfFlushs.getInt();
            canRecharge = CustomOptionHolder.plumberCanRecharge.getBool();
            rechargeTasksNumber = CustomOptionHolder.plumberRechargeTasksNumber.getFloat();
            rechargedTasks = CustomOptionHolder.plumberRechargeTasksNumber.getFloat();
            sealVentCooldown = CustomOptionHolder.plumberSealCooldown.getFloat();
            remainingSeals = CustomOptionHolder.plumberNumberOfVents.getInt();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.FlushButton.png", 115f);
            return buttonSprite;
        }

        private static Sprite closeVentButtonSprite;
        public static Sprite getCloseVentButtonSprite()
        {
            if (closeVentButtonSprite) return closeVentButtonSprite;
            closeVentButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.CloseVentButton.png", 115f);
            return closeVentButtonSprite;
        }

        private static Sprite animatedVentSealedSprite;
        private static float lastPPU;
        public static Sprite getAnimatedVentSealedSprite()
        {
            float ppu = 185f;
            if (lastPPU != ppu)
            {
                animatedVentSealedSprite = null;
                lastPPU = ppu;
            }
            if (animatedVentSealedSprite) return animatedVentSealedSprite;
            animatedVentSealedSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.AnimatedVentSealed.png", ppu);
            return animatedVentSealedSprite;
        }

        private static Sprite staticVentSealedSprite;
        public static Sprite getStaticVentSealedSprite()
        {
            if (staticVentSealedSprite) return staticVentSealedSprite;
            staticVentSealedSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.StaticVentSealed.png", 160f);
            return staticVentSealedSprite;
        }

        private static Sprite fungleVentSealedSprite;
        public static Sprite getFungleVentSealedSprite()
        {
            if (fungleVentSealedSprite) return fungleVentSealedSprite;
            fungleVentSealedSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.FungleVentSealed.png", 160f);
            return fungleVentSealedSprite;
        }
    }

    public static class Deceiver
    {
        public static PlayerControl deceiver;
        public static DeceiverDecoy decoy;
        public static Color color = Palette.ImpostorRed;

        public static float placeCooldown = 30f;
        public static float swapCooldown = 30f;
        public static int showDecoy = 0;

        public static void clearAndReload()
        {
            deceiver = null;
            decoy = null;
            placeCooldown = CustomOptionHolder.deceiverPlaceCooldown.getFloat();
            swapCooldown = CustomOptionHolder.deceiverSwapCooldown.getFloat();
            showDecoy = CustomOptionHolder.deceiverShowDecoy.getSelection();
        }

        private static Sprite buttonSprite;
        public static Sprite getDecoyButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DecoyButton.png", 115f);
            return buttonSprite;
        }

        private static Sprite swapButtonSprite;
        public static Sprite getDecoySwapButtonSprite()
        {
            if (swapButtonSprite) return swapButtonSprite;
            swapButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DecoySwapButton.png", 115f);
            return swapButtonSprite;
        }

        private static Sprite destroyButtonSprite;
        public static Sprite getDecoyDestroyButtonSprite()
        {
            if (destroyButtonSprite) return destroyButtonSprite;
            destroyButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DecoyDestroyButton.png", 115f);
            return destroyButtonSprite;
        }
    }
    #endregion

    #region Modifiers
    public static class Bait
    {
        public static List<PlayerControl> bait = new();
        public static Dictionary<DeadPlayer, float> active = new();
        public static Color color = Palette.CrewmateBlue;

        public static float reportDelayMin;
        public static float reportDelayMax;
        public static bool showKillFlash = true;

        public static void clearAndReload()
        {
            bait = new List<PlayerControl>();
            active = new Dictionary<DeadPlayer, float>();
            reportDelayMin = CustomOptionHolder.modifierBaitReportDelayMin.getFloat();
            reportDelayMax = CustomOptionHolder.modifierBaitReportDelayMax.getFloat();
            if (reportDelayMin > reportDelayMax) reportDelayMin = reportDelayMax;
            showKillFlash = CustomOptionHolder.modifierBaitShowKillFlash.getBool();
        }
    }

    public static class Blind
    {
        public static PlayerControl blind;
        public static Color color = Palette.CrewmateBlue;

        public static void clearAndReload()
        {
            blind = null;
        }
    }

    public static class ButtonBarry
    {
        public static PlayerControl buttonBarry;
        public static Color color = Color.yellow;

        public static bool isCalledEmergency = false;

        public static void clearAndReload()
        {
            buttonBarry = null;
            isCalledEmergency = false;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.EmergencyButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Shy
    {
        public static List<PlayerControl> shy = new();
        public static Dictionary<byte, float> lastMoved = new();
        public static Color color = Color.yellow;

        public static float minVisibility = 0.2f;
        public static float holdDuration = 1f;
        public static float fadeDuration = 0.5f;

        public static void clearAndReload()
        {
            shy = new List<PlayerControl>();
            lastMoved = new Dictionary<byte, float>();
            holdDuration = CustomOptionHolder.modifierShyHoldDuration.getFloat();
            fadeDuration = CustomOptionHolder.modifierShyFadeDuration.getFloat();
            minVisibility = CustomOptionHolder.modifierShyMinVisibility.getSelection() / 10f;
        }

        public static float visibility(byte playerId)
        {
            float visibility = 1f;
            if (lastMoved != null && lastMoved.ContainsKey(playerId))
            {
                var tStill = Time.time - lastMoved[playerId];
                if (tStill > holdDuration)
                {
                    if (tStill - holdDuration > fadeDuration) visibility = minVisibility;
                    else visibility = (1 - (tStill - holdDuration) / fadeDuration) * (1 - minVisibility) + minVisibility;
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsDead && visibility < 0.1f)
            {
                // Ghosts can always see!
                visibility = 0.1f;
            }
            return visibility;
        }

        public static void update()
        {
            foreach (var shyPlayer in shy)
            {
                if (shyPlayer == Swooper.swooper && Swooper.isInvisible) continue;  // Dont make Swooper visible...
                // check movement by animation
                PlayerPhysics playerPhysics = shyPlayer.MyPhysics;
                var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
                if (currentPhysicsAnim != playerPhysics.Animations.group.IdleAnim)
                {
                    lastMoved[shyPlayer.PlayerId] = Time.time;
                }
                // calculate and set visibility
                float visibility = Shy.visibility(shyPlayer.PlayerId);
                float petVisibility = visibility;
                if (shyPlayer.Data.IsDead)
                {
                    visibility = 0.5f;
                    petVisibility = 1f;
                }
                try
                {  // Sometimes renderers are missing for weird reasons. Try catch to avoid exceptions
                    shyPlayer.cosmetics.currentBodySprite.BodySprite.color = shyPlayer.cosmetics.currentBodySprite.BodySprite.color.SetAlpha(visibility);
                    if (DataManager.Settings.Accessibility.ColorBlindMode) shyPlayer.cosmetics.colorBlindText.color = shyPlayer.cosmetics.colorBlindText.color.SetAlpha(visibility);
                    shyPlayer.SetHatAndVisorAlpha(visibility);
                    shyPlayer.cosmetics.skin.layer.color = shyPlayer.cosmetics.skin.layer.color.SetAlpha(visibility);
                    shyPlayer.cosmetics.nameText.color = shyPlayer.cosmetics.nameText.color.SetAlpha(visibility);
                    foreach (var rend in shyPlayer.cosmetics.currentPet.renderers)
                        rend.color = rend.color.SetAlpha(petVisibility);
                    foreach (var shadowRend in shyPlayer.cosmetics.currentPet.shadows)
                        shadowRend.color = shadowRend.color.SetAlpha(petVisibility);
                }
                catch { }
            }
        }
    }

    public static class Flash
    {
        public static List<PlayerControl> flash = new();
        public static Color color = Color.yellow;

        public static float speed = 1.5f;

        public static void clearAndReload()
        {
            flash = new List<PlayerControl>();
            speed = CustomOptionHolder.modifierFlashSpeed.getFloat();
        }
    }

    public static class Mini
    {
        public static PlayerControl mini;
        public static Color color = Color.yellow;
        public const float defaultColliderRadius = 0.2233912f;
        public const float defaultColliderOffset = 0.3636057f;

        public static float growingUpDuration = 400f;
        public static bool isGrowingUpInMeeting = true;
        public static DateTime timeOfGrowthStart = DateTime.UtcNow;
        public static DateTime timeOfMeetingStart = DateTime.UtcNow;
        public static float ageOnMeetingStart = 0f;
        public static bool triggerMiniLose = false;

        public static void clearAndReload()
        {
            mini = null;
            triggerMiniLose = false;
            growingUpDuration = CustomOptionHolder.modifierMiniGrowingUpDuration.getFloat();
            isGrowingUpInMeeting = CustomOptionHolder.modifierMiniGrowingUpInMeeting.getBool();
            timeOfGrowthStart = DateTime.UtcNow;
        }

        public static float growingProgress()
        {
            float timeSinceStart = (float)(DateTime.UtcNow - timeOfGrowthStart).TotalMilliseconds;
            return Mathf.Clamp(timeSinceStart / (growingUpDuration * 1000), 0f, 1f);
        }

        public static bool isGrownUp()
        {
            return growingProgress() == 1f;
        }
    }

    public static class Indomitable
    {
        public static PlayerControl indomitable;
        public static Color color = Palette.CrewmateBlue;

        public static void clearAndReload()
        {
            indomitable = null;
        }
    }

    public static class Lovers
    {
        public static PlayerControl lover1;
        public static PlayerControl lover2;
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);

        public static bool notAckedExiledIsLover = false;

        public static bool bothDie = true;
        public static bool enableChat = true;

        public static bool existing()
        {
            return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
        }

        public static bool existingAndAlive()
        {
            return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static PlayerControl otherLover(PlayerControl oneLover)
        {
            if (!existingAndAlive()) return null;
            if (oneLover == lover1) return lover2;
            if (oneLover == lover2) return lover1;
            return null;
        }

        public static bool existingWithKiller()
        {
            return existing() && (lover1.isKiller() || lover2.isKiller());
        }

        public static bool hasAliveKillingLover(this PlayerControl player)
        {
            if (!Lovers.existingAndAlive() || !existingWithKiller())
                return false;
            return player != null && (player == lover1 || player == lover2);
        }

        public static void clearAndReload()
        {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
            enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
        }

        public static PlayerControl getPartner(this PlayerControl player)
        {
            if (player == null)
                return null;
            if (lover1 == player)
                return lover2;
            if (lover2 == player)
                return lover1;
            return null;
        }
    }

    public static class Multitasker
    {
        public static List<PlayerControl> multitasker = new();
        public static Color color = Palette.CrewmateBlue;

        public static void clearAndReload()
        {
            multitasker.Clear();
        }
    }

    public static class Radar
    {
        public static PlayerControl radar;
        public static Arrow localArrow;
        public static Color color = Color.yellow;

        public static void clearAndReload()
        {
            radar = null;
            if (localArrow?.arrow != null)
                UnityEngine.Object.Destroy(localArrow.arrow);
        }
    }

    public static class Sleuth
    {
        public static PlayerControl sleuth;
        public static List<PlayerControl> reported = new();
        public static Color color = Color.yellow;

        public static void clearAndReload()
        {
            sleuth = null;
            reported = new List<PlayerControl>();
        }
    }

    public static class Tiebreaker
    {
        public static PlayerControl tiebreaker;
        public static Color color = Color.yellow;

        public static bool isTiebreak = false;

        public static void clearAndReload()
        {
            tiebreaker = null;
            isTiebreak = false;
        }
    }

    public static class Torch
    {
        public static List<PlayerControl> torch = new();
        public static Color color = Palette.CrewmateBlue;

        public static float vision = 1;

        public static void clearAndReload()
        {
            torch = new List<PlayerControl>();
            vision = CustomOptionHolder.modifierTorchVision.getFloat();
        }
    }

    public static class Vip
    {
        public static List<PlayerControl> vip = new List<PlayerControl>();
        public static Color color = Palette.CrewmateBlue;

        public static bool showColor = true;

        public static void clearAndReload()
        {
            vip = new List<PlayerControl>();
            showColor = CustomOptionHolder.modifierVipShowColor.getBool();
        }
    }

    public static class Drunk
    {
        public static List<PlayerControl> drunk = new List<PlayerControl>();
        public static Color color = Color.yellow;

        public static int meetings = 3;

        public static void clearAndReload()
        {
            drunk = new List<PlayerControl>();
            meetings = CustomOptionHolder.modifierDrunkDuration.getInt();
        }
    }

    public static class Immovable
    {
        public static List<PlayerControl> immovable = new List<PlayerControl>();
        public static Color color = Color.yellow;

        public static Vector3 position;

        public static void clearAndReload()
        {
            immovable = new List<PlayerControl>();
            position = Vector3.zero;
        }

        public static void setPosition()
        {
            if (position == Vector3.zero) return;
            if (immovable.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
            }
        }
    }

    public static class DoubleShot
    {
        public static PlayerControl doubleShot;
        public static Color color = Palette.ImpostorRed;

        public static bool isUsedSecondLife = false;

        public static void clearAndReload()
        {
            doubleShot = null;
            isUsedSecondLife = false;
        }
    }

    public static class Ruthless
    {
        public static PlayerControl ruthless;
        public static Color color = Palette.ImpostorRed;

        public static void clearAndReload()
        {
            ruthless = null;
        }
    }

    public static class Underdog
    {
        public static PlayerControl underdog;
        public static Color color = Palette.ImpostorRed;

        public static float addition = 30f;
        public static bool increaseCooldown = false;

        public static void clearAndReload()
        {
            underdog = null;
            addition = CustomOptionHolder.modifierUnderdogCooldownAddition.getFloat();
            increaseCooldown = CustomOptionHolder.modifierUnderdogIncreaseCooldown.getBool();
        }
    }

    public static class Saboteur
    {
        public static PlayerControl saboteur;
        public static Color color = Palette.ImpostorRed;

        public static float timer = 0f;

        public static float reduceCooldown = 5f;

        public static void clearAndReload()
        {
            saboteur = null;
            timer = 0f;
            reduceCooldown = CustomOptionHolder.modifierSaboteurReduceSaboCooldown.getFloat();
        }
    }

    public static class Frosty
    {
        public static PlayerControl frosty;
        public static PlayerControl chilled;
        public static Color color = Palette.CrewmateBlue;

        public static bool isChilled = false;
        public static DateTime lastChilled { get; set; }

        public static float duration = 10f;
        public static float startSpeed = 0.25f;

        public static void clearAndReload()
        {
            frosty = null;
            chilled = null;
            isChilled = false;
            lastChilled = DateTime.UtcNow;
            duration = CustomOptionHolder.modifierFrostyDuration.getFloat();
            startSpeed = CustomOptionHolder.modifierFrostyStartSpeed.getFloat();
        }
    }

    public static class Satelite
    {
        public static PlayerControl satelite;
        public static List<Arrow> localArrows = new();
        public static List<Vector3> deadBodyPositions = new();
        public static Color color = Color.yellow;

        public static float corpsesTrackingTimer = 0f;

        public static float corpsesTrackingCooldown = 30f;
        public static float corpsesTrackingDuration = 5f;

        public static void clearAndReload()
        {
            satelite = null;
            if (localArrows != null)
            {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            }
            deadBodyPositions = new List<Vector3>();
            corpsesTrackingTimer = 0f;
            corpsesTrackingCooldown = CustomOptionHolder.modifierSateliteTrackingCooldown.getFloat();
            corpsesTrackingDuration = CustomOptionHolder.modifierSateliteTrackingDuration.getFloat();
        }

        private static Sprite trackCorpsesButtonSprite;
        public static Sprite getTrackCorpsesButtonSprite()
        {
            if (trackCorpsesButtonSprite) return trackCorpsesButtonSprite;
            trackCorpsesButtonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.PathfindButton.png", 115f);
            return trackCorpsesButtonSprite;
        }
    }

    public static class SixthSense
    {
        public static PlayerControl sixthSense;
        public static Color color = Color.yellow;

        public static void clearAndReload()
        {
            sixthSense = null;
        }
    }

    public static class Taskmaster
    {
        public static PlayerControl taskMaster;
        public static Color color = Palette.CrewmateBlue;

        public static void clearAndReload()
        {
            taskMaster = null;
        }
    }

    public static class Disperser
    {
        public static PlayerControl disperser;
        public static Color color = Palette.ImpostorRed;

        public static int numberOfKills = 0;

        public static int remainingDisperses = 1;
        public static bool disperseToVents = false;
        public static bool canRecharge = false;
        public static float rechargeKillsNumber;
        public static float rechargedKills;

        public static void clearAndReload()
        {
            disperser = null;
            numberOfKills = 0;
            remainingDisperses = CustomOptionHolder.modifierDisperserNumOfDisperses.getInt();
            disperseToVents = CustomOptionHolder.modifierDisperserDisperseToVents.getBool();
            canRecharge = CustomOptionHolder.modifierDisperserCanRecharge.getBool();
            rechargeKillsNumber = CustomOptionHolder.modifierDisperserRechargeKillsNumber.getFloat();
            rechargedKills = CustomOptionHolder.modifierDisperserRechargeKillsNumber.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DisperseButton.png", 100f);
            return buttonSprite;
        }
    }

    public static class Poucher
    {
        public static PlayerControl poucher;
        public static Color color = Palette.ImpostorRed;

        public static List<PlayerControl> killed = new();

        public static void clearAndReload()
        {
            poucher = null;
            killed = new();
        }
    }
    #endregion

    #region Ghost Roles
    public static class Banshee
    {
        public static PlayerControl banshee;
        public static PlayerControl currentTarget;
        public static PlayerControl scareVictim;
        public static Color color = Palette.ImpostorRed;

        public static float scareTimer = 0f;

        public static float cooldown = 30f;
        public static float duration = 10f;

        public static void clearAndReload()
        {
            banshee = null;
            scareTimer = 0f;
            cooldown = CustomOptionHolder.bansheeCooldown.getFloat();
            duration = CustomOptionHolder.bansheeDuration.getFloat();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.ScareButton.png", 115f);
            return buttonSprite;
        }
    }

    public static class Poltergeist
    {
        public static PlayerControl poltergeist;
        public static DeadBody targetBody;
        public static Color color = Palette.CrewmateBlue;

        public static float cooldown = 30f;
        public static float radius = 1f;

        public static void clearAndReload()
        {
            poltergeist = null;
            targetBody = null;
            cooldown = CustomOptionHolder.poltergeistCooldown.getFloat();
            radius = CustomOptionHolder.poltergeistRadius.getFloat();
        }

        public static void MoveDeadBody(byte targetId, Vector2 pos) => Coroutines.Start(CoMoveDeadBody(Helpers.GetDeadBody(targetId), pos));
        public static IEnumerator CoMoveDeadBody(DeadBody deadBody, Vector2 pos)
        {
            if (deadBody == null)
            {
                yield break;
            }

            float p = 0f;
            Vector2 beginPos = deadBody.transform.position;

            while (deadBody)
            {
                p += Time.deltaTime * 0.85f;
                if (!(p < 1f)) break;

                float pp = p * p;
                Vector3 currentPos = (beginPos * (1 - pp)) + (pos * pp);
                currentPos.z = currentPos.y / 1000f;
                deadBody.transform.position = currentPos;

                yield return null;
            }

            if (deadBody) deadBody.transform.position = AsVector3(pos, pos.y / 1000f);

            yield break;
            static Vector3 AsVector3(Vector2 vec, float z)
            {
                Vector3 result = vec;
                result.z = z;
                return result;
            }
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.DragButton.png", 100f);
            return buttonSprite;
        }
    }
    #endregion
}