using HarmonyLib;
using System;
using Hazel;
using UnityEngine;
using System.Linq;
using static TownOfUs.GameHistory;
using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using AmongUs.GameOptions;

namespace TownOfUs.Patches {

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            float num = float.MaxValue;
            PlayerControl @object = pc.Object;

            bool roleCouldUse = @object.roleCanUseVents();
            var usableDistance = __instance.UsableDistance;

            if (__instance.name.StartsWith("MinerVent_")) {
                if (Miner.miner != PlayerControl.LocalPlayer) {
                    canUse = false;
                    couldUse = false;
                    __result = num;
                    return false; 
                } else {
                    // Reduce the usable distance to reduce the risk of gettings stuck while trying to jump into the box if it's placed near objects
                    usableDistance = 0.4f; 
                }
            }

            couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
            canUse = couldUse;
            if (canUse)
            {
                Vector3 center = @object.Collider.bounds.center;
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(center, position);
                canUse &= (num <= usableDistance && (!PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false) || __instance.name.StartsWith("MinerVent_")));
            }
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    public static class VentUsePatch {
        public static bool Prefix(Vent __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            bool canUse;
            bool couldUse;
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
            bool canMoveInVents = true;
            if (!canUse) return false; // No need to execute the native method as using is disallowed anyways

            bool isEnter = !PlayerControl.LocalPlayer.inVent;

            if (__instance.name.StartsWith("MinerVent_")) {
                __instance.SetButtons(isEnter && canMoveInVents);
                MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UseUncheckedVent, Hazel.SendOption.Reliable);
                writer.WritePacked(__instance.Id);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(isEnter ? byte.MaxValue : (byte)0);
                writer.EndMessage();
                RPCProcedure.useUncheckedVent(__instance.Id, PlayerControl.LocalPlayer.PlayerId, isEnter ? byte.MaxValue : (byte)0);
                return false;
            }

            if (isEnter) {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
            } else {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
            }
            __instance.SetButtons(isEnter && canMoveInVents);
            return false;
        }
    }

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    class VentButtonDoClickPatch {
        static  bool Prefix(VentButton __instance) {
		    if (__instance.currentTarget != null && Glitch.hackedPlayer != PlayerControl.LocalPlayer) __instance.currentTarget.Use();
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class VentButtonVisibilityPatch {
        static void Postfix(PlayerControl __instance) {
            if (__instance.AmOwner && __instance.roleCanUseVents() && FastDestroyableSingleton<HudManager>.Instance.ReportButton.isActiveAndEnabled) {
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();
            }
        }
    }

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.SetTarget))]
    class VentButtonSetTargetPatch {
        static Sprite defaultVentSprite = null;
        static void Postfix(VentButton __instance) {
            if (Miner.miner != null && Miner.miner == PlayerControl.LocalPlayer) {
                if (defaultVentSprite == null) defaultVentSprite = __instance.graphic.sprite;
                __instance.graphic.sprite = defaultVentSprite;
                __instance.buttonLabelText.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch {
        public static bool Prefix(KillButton __instance) {
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove) {
                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill) {
                    PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    if (PlayerControl.LocalPlayer == Cleaner.cleaner)
                        Cleaner.cleaner.killTimer = HudManagerStartPatch.cleanerCleanButton.Timer = HudManagerStartPatch.cleanerCleanButton.MaxTimer;
                }
                __instance.SetTarget(null);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch {
        static void Postfix(EmergencyMinigame __instance) {
            var roleCanCallEmergency = true;
            var statusText = "";
            
            // Deactivate emergency button for Swapper
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer && !Swapper.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Swapper can't start an emergency meeting";
            }

            // Potentially deactivate emergency button for Jester
            if (Jester.jester != null && Jester.jester == PlayerControl.LocalPlayer && !Jester.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Jester can't start an emergency meeting";
            }

            // Potentially deactivate emergency button for Executioner
            if (Executioner.executioner != null && Executioner.executioner == PlayerControl.LocalPlayer && !Executioner.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Executioner can't start an emergency meeting";
            }

            // Potentially deactivate emergency button for Lawyer
            if (Lawyer.lawyer != null && Lawyer.lawyer == PlayerControl.LocalPlayer && !Lawyer.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Lawyer can't start an emergency meeting";
            }

            if (!roleCanCallEmergency) {
                __instance.StatusText.text = statusText;
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }
        }
    }


    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse) {
            canUse = couldUse = false;
            if (__instance.AllowImpostor) return true;
            if (!Helpers.hasFakeTasks(pc.Object)) return true;
            __result = float.MaxValue;
            return false;
        }
    }

    [HarmonyPatch]
    class VitalsMinigamePatch {
        private static List<TMPro.TextMeshPro> spyTexts = new List<TMPro.TextMeshPro>();
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch {
            public static bool Prefix(VitalsMinigame __instance)
            {
                if (TimeLord.timeLord != null && TimeLord.timeLord == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    return false;
                }
                return true;
            }
            
            static void Postfix(VitalsMinigame __instance)
            {
                if (Spy.spy != null && PlayerControl.LocalPlayer == Spy.spy)
                {
                    spyTexts = new List<TMPro.TextMeshPro>();
                    foreach (VitalsPanel panel in __instance.vitals)
                    {
                        TMPro.TextMeshPro text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                        spyTexts.Add(text);
                        UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                        text.gameObject.SetActive(false);
                        text.transform.localScale = Vector3.one * 0.75f;
                        text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                    }
                }

                //Fix Visor in Vitals
                foreach (VitalsPanel panel in __instance.vitals)
                {
                    if (panel.PlayerIcon != null && panel.PlayerIcon.cosmetics.skin != null)
                    {
                        panel.PlayerIcon.cosmetics.skin.transform.position = new Vector3(0, 0, 0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch {
            static void Postfix(VitalsMinigame __instance) {
                // Hacker show time since death
                if (Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer) {
                    for (int k = 0; k < __instance.vitals.Length; k++) {
                        VitalsPanel vitalsPanel = __instance.vitals[k];
                        NetworkedPlayerInfo player = vitalsPanel.PlayerInfo;

                        // Hacker update
                        if (vitalsPanel.IsDead) {
                            DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                            if (deadPlayer != null && k < spyTexts.Count && spyTexts[k] != null) {
                                float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                                spyTexts[k].gameObject.SetActive(true);
                                spyTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                            }
                        }
                    }
                } else {
                    foreach (TMPro.TextMeshPro text in spyTexts)
                        if (text != null && text.gameObject != null)
                            text.gameObject.SetActive(false);
                }
            }
        }
    }

    [HarmonyPatch]
    class AdminPanelPatch {
        static Dictionary<SystemTypes, List<Color>> players = new Dictionary<SystemTypes, System.Collections.Generic.List<Color>>();
        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        class MapCountOverlayUpdatePatch {
            static bool Prefix(MapCountOverlay __instance) {
                // Save colors for the Spy
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f)
                {
                    return false;
                }
                __instance.timer = 0f;
                players = new Dictionary<SystemTypes, List<Color>>();

                bool commsActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixComms) commsActive = true;

                if (!__instance.isSab && commsActive)
                {
                    __instance.isSab = true;
                    __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                    __instance.SabotageText.gameObject.SetActive(true);
                    return false;
                }
                if (__instance.isSab && !commsActive)
                {
                    __instance.isSab = false;
                    __instance.BackgroundColor.SetColor(Color.green);
                    __instance.SabotageText.gameObject.SetActive(false);
                }

                for (int i = 0; i < __instance.CountAreas.Length; i++)
                {
                    CounterArea counterArea = __instance.CountAreas[i];
                    List<Color> roomColors = new List<Color>();
                    players.Add(counterArea.RoomType, roomColors);

                    if (!commsActive)
                    {
                        PlainShipRoom plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];

                        if (plainShipRoom != null && plainShipRoom.roomArea) {


                            HashSet<int> hashSet = new HashSet<int>();
                            int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            int num2 = 0;
                            for (int j = 0; j < num; j++) {
                                Collider2D collider2D = __instance.buffer[j];
                                if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies) {
                                    num2++;
                                    DeadBody bodyComponent = collider2D.GetComponent<DeadBody>();
                                    if (bodyComponent) {
                                        NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(bodyComponent.ParentId);
                                        if (playerInfo != null) {
                                            var color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                                            roomColors.Add(color);
                                        }
                                    }
                                } else if (!collider2D.isTrigger) {
                                    PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                    if (component && component.Data != null && !component.Data.Disconnected && !component.Data.IsDead && (__instance.showLivePlayerPosition || !component.AmOwner) && hashSet.Add((int)component.PlayerId)) {
                                        num2++;
                                        if (component?.cosmetics?.currentBodySprite?.BodySprite?.material != null) {
                                            Color color = component.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
                                            roomColors.Add(color);
                                        }
                                    }
                                }
                            }

                            counterArea.UpdateCount(num2);
                        }
                        else
                        {
                            Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                        }
                    }
                    else
                    {
                        counterArea.UpdateCount(0);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
        class CounterAreaUpdateCountPatch {
            private static Material defaultMat;
            private static Material newMat;
            static void Postfix(CounterArea __instance) {
                // Spy display saved colors on the admin panel
                bool showSpyInfo = Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer;
                if (players.ContainsKey(__instance.RoomType)) {
                    List<Color> colors = players[__instance.RoomType];
                    int i = -1;
                    foreach (var icon in __instance.myIcons.GetFastEnumerator())
                    {
                        i += 1;
                        SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                        if (renderer != null) {
                            if (defaultMat == null) defaultMat = renderer.material;
                            if (newMat == null) newMat = UnityEngine.Object.Instantiate<Material>(defaultMat);
                            if (showSpyInfo && colors.Count > i) {
                                renderer.material = newMat;
                                var color = colors[i];
                                renderer.material.SetColor("_BodyColor", color);
                                var id = Palette.PlayerColors.IndexOf(color);
                                if (id < 0) {
                                    renderer.material.SetColor("_BackColor", color);
                                } else {
                                    renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                                }
                                renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                            } else {
                                renderer.material = defaultMat;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    class CameraPatch {
        [HarmonyPatch]
        public class SurveillanceMinigamePatch {
            private static int page = 0;
            private static float timer = 0f;

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
            class SurveillanceMinigameBeginPatch {
                public static void Postfix(SurveillanceMinigame __instance) {
                    // Add securityGuard cameras
                    page = 0;
                    timer = 0;
                    if (MapUtilities.CachedShipStatus.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0) {
                        __instance.textures = __instance.textures.ToList().Concat(new RenderTexture[MapUtilities.CachedShipStatus.AllCameras.Length - 4]).ToArray();
                        for (int i = 4; i < MapUtilities.CachedShipStatus.AllCameras.Length; i++) {
                            SurvCamera surv = MapUtilities.CachedShipStatus.AllCameras[i];
                            Camera camera = UnityEngine.Object.Instantiate<Camera>(__instance.CameraPrefab);
                            camera.transform.SetParent(__instance.transform);
                            camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                            camera.orthographicSize = 2.35f;
                            RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                            __instance.textures[i] = temporary;
                            camera.targetTexture = temporary;
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
            class SurveillanceMinigameUpdatePatch {
                public static bool Prefix(SurveillanceMinigame __instance) {
                    // Update normal and securityGuard cameras
                    timer += Time.deltaTime;
                    int numberOfPages = Mathf.CeilToInt(MapUtilities.CachedShipStatus.AllCameras.Length / 4f);

                    bool update = false;

                    if (timer > 3f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                        update = true;
                        timer = 0f;
                        page = (page + 1) % numberOfPages;
                    } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
                        page = (page + numberOfPages - 1) % numberOfPages;
                        update = true;
                        timer = 0f;
                    }

                    if ((__instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer)) {
                        __instance.isStatic = false;
                        for (int i = 0; i < __instance.ViewPorts.Length; i++) {
                            __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                            __instance.SabText[i].gameObject.SetActive(false);
                            if (page * 4 + i < __instance.textures.Length)
                                __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[page * 4 + i]);
                            else
                                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                        }
                    } else if (!__instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer)) {
                        __instance.isStatic = true;
                        for (int j = 0; j < __instance.ViewPorts.Length; j++) {
                            __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                            __instance.SabText[j].gameObject.SetActive(true);
                        }
                    }
                    return false;
                }
            }
        }

        [HarmonyPatch]
        public class PlanetSurveillanceMinigamePatch
        {
            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
            class PlanetSurveillanceMinigameUpdatePatch {
                public static void Postfix(PlanetSurveillanceMinigame __instance) {
                    if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                        __instance.NextCamera(1);
                    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                        __instance.NextCamera(-1);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    class MedScanMinigameFixedUpdatePatch {
        static void Prefix(MedScanMinigame __instance) {
            if (TOUMapOptions.allowParallelMedBayScans) {
                __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
                __instance.medscan.UsersList.Clear();
            }
        }
    }
}