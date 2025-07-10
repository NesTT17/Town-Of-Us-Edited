using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs
{
    public enum MurderAttemptResult {
        PerformKill,
        SuppressKill,
        BlankKill,
        DelayPoisonerKill,
        ReverseKill,
    }

    public enum CustomGamemodes {
        Classic,
        Guesser,
    }

    public enum SabatageTypes {
        Comms,
        O2,
        Reactor,
        Lights,
        None
    }

    public static class Helpers {
        public static string previousEndGameSummary = "";
        public static Dictionary<string, Sprite> CachedSprites = new();
        public static Color impAbilityTargetColor = new Color(0.3f, 0f, 0f);
        public static List<byte> revealedTimeLords = new();
        public static List<byte> knownTimeLordKillers = new();
        public static bool localPlayerCanSeeOthersRoles = false;

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit, bool cache = true)
        {
            try
            {
                if (cache && CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
                Texture2D texture = loadTextureFromResources(path);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                if (cache) sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                if (!cache) return sprite;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static unsafe Texture2D loadTextureFromResources(string path) {
            try {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int) length));
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            } catch {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path) {
            try {          
                if (File.Exists(path))     {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                    ImageConversion.LoadImage(texture, byteTexture, false);
                    return texture;
                }
            } catch {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static void refreshRoleDescription(PlayerControl player) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player); 
            List<string> taskTexts = new(infos.Count); 

            foreach (var roleInfo in infos)
            {
                taskTexts.Add(getRoleString(roleInfo));
            }
            
            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks.GetFastEnumerator()) 
            {
                var textTask = t.TryCast<ImportantTextTask>();
                if (textTask == null) continue;
                
                var currentText = textTask.Text;
                
                if (taskTexts.Contains(currentText)) taskTexts.Remove(currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
                else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }   

            foreach (PlayerTask t in toRemove) {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (string title in taskTexts) {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = title;
                task.Id = 100;
                player.myTasks.Insert(0, task);
            }
        }

        internal static string getRoleString(RoleInfo roleInfo)
        {
            if (roleInfo.factionId == FactionId.Modifier)
                return cs(roleInfo.color, $"Modifier: {roleInfo.name}\n{roleInfo.shortDescription}");
            return cs(roleInfo.color, $"Role: {roleInfo.name}\n{roleInfo.shortDescription}");
        }

        public static bool isD(byte playerId) {
            return playerId % 2 == 0;
        }

        public static bool isLighterColor(PlayerControl target) {
            return isD(target.PlayerId);
        }

        public static bool hasFakeTasks(this PlayerControl player) {
            return player.isAnyNeutral();
        }

        public static bool shouldShowGhostInfo() {
            return PlayerControl.LocalPlayer != null && localPlayerCanSeeOthersRoles && PlayerControl.LocalPlayer.Data.IsDead && TOUMapOptions.ghostsSeeInformation || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;
        }

        public static void clearAllTasks(this PlayerControl player) {
            if (player == null) return;
            foreach (var playerTask in player.myTasks.GetFastEnumerator())
            {
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();
            
            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static void MurderPlayer(this PlayerControl player, PlayerControl target)
        {
            player.MurderPlayer(target, MurderResultFlags.Succeeded);
        }

        public static void RpcRepairSystem(this ShipStatus shipStatus, SystemTypes systemType, byte amount)
        {
            shipStatus.RpcUpdateSystem(systemType, amount);
        }

        public static bool isMira() {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
        }

        public static bool isAirship() {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
        }

        public static bool isSkeld() {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
        }

        public static bool isPolus() {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
        }

        public static bool isFungle() {           
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
        }

        public static bool MushroomSabotageActive() {
            return PlayerControl.LocalPlayer.myTasks.ToArray().Any((x) => x.TaskType == TaskTypes.MushroomMixupSabotage);
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value, float alpha=0.25f) {
            alpha = value ? alpha : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts) {
            return t.GetString(key, parts);
        }

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        public static int lineCount(string text) {
            return text.Count(c => c == '\n');
        }

        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie) {
            tie = true;
            KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }

        public static bool hidePlayerName(PlayerControl source, PlayerControl target) {
            if (Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Helpers.isActiveCamoComms()) return true; // No names are visible
            
            if (Swooper.isInvisble && target == Swooper.swooper) return true;
            else if (Phantom.isInvisble && target == Phantom.phantom) return true;
            else if (Venerer.morphTimer > 0f && target == Venerer.venerer) return true;
            else if (!TOUMapOptions.hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Vampire.vampire && Vampire.wasTeamRed || target == Dracula.dracula && Dracula.wasTeamRed)) return false; // Members of team Impostors see the names of Impostors
            else if ((source == Lovers.lover1 || source == Lovers.lover2) && (target == Lovers.lover1 || target == Lovers.lover2)) return false; // Members of team Lovers see the names of each other
            else if ((source == Dracula.dracula || source == Vampire.vampire) && (target == Dracula.dracula || target == Vampire.vampire || target == Dracula.fakeVampire)) return false; // Members of team Vampires see the names of each other
            else if (source == Blackmailer.blackmailer && target == Blackmailer.blackmailed) return false; // Blackmailer can see blackmailed player
            return true;
        }

        public static void setDefaultLook(this PlayerControl target) {
            if (Helpers.MushroomSabotageActive()) {
                var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
                MushroomMixupSabotageSystem.CondensedOutfit condensedOutfit = instance.currentMixups[target.PlayerId];
                NetworkedPlayerInfo.PlayerOutfit playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
                target.MixUpOutfit(playerOutfit);
            } else
                target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId) {
            target.RawSetColor(colorId);
            target.RawSetVisor(visorId, colorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);


            SkinViewData nextSkin = null;
            try {
                nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId);
            } catch { return; }
            
            PlayerPhysics playerPhysics = target.MyPhysics;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.myPlayer.cosmetics.skin.animator;
            var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();


            if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;
            float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playerPhysics.myPlayer.cosmetics.skin.skin = nextSkin;
            playerPhysics.myPlayer.cosmetics.skin.UpdateMaterial();

            spriteAnim.Play(clip, 1f);
            spriteAnim.m_animator.Play("a", 0, progress % 1);
            spriteAnim.m_animator.Update(0f);

            target.RawSetPet(petId, colorId);
        }

        public static void showFlash(Color color, float duration=1f, string message="") {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            // Message Text
            TMPro.TextMeshPro messageText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
            messageText.text = message;
            messageText.enableWordWrapping = false;
            messageText.transform.localScale = Vector3.one * 0.5f;
            messageText.transform.localPosition += new Vector3(0f, 2f, -69f);
            messageText.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) => {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                if (p < 0.5) {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                } else {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
                if (p == 1f) messageText.gameObject.Destroy();
            })));
        }

        public static bool roleCanUseVents(this PlayerControl player) {
            bool roleCouldUse = false;
            if (Engineer.engineer != null && Engineer.engineer == player)
                roleCouldUse = true;
            else if (Dracula.canUseVents && Dracula.dracula != null && Dracula.dracula == player)
                roleCouldUse = true;
            else if (Vampire.canUseVents && Vampire.vampire != null && Vampire.vampire == player)
                roleCouldUse = true;
            else if (Scavenger.canUseVents && Scavenger.scavenger != null && Scavenger.scavenger == player)
                roleCouldUse = true;
            else if (Juggernaut.canUseVents && Juggernaut.juggernaut != null && Juggernaut.juggernaut == player)
                roleCouldUse = true;
            else if (Werewolf.canVent && Werewolf.isRampageActive && Werewolf.werewolf != null && Werewolf.werewolf == player)
                roleCouldUse = true;
            else if (Glitch.canVent && Glitch.glitch != null && Glitch.glitch == player)
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
                roleCouldUse = true;
            return roleCouldUse;
        }

        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false, bool ignoreMedic = false) {
            var targetRole = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return MurderAttemptResult.PerformKill;

            // Handle first kill attempt
            if (TOUMapOptions.shieldFirstKill && TOUMapOptions.firstKillPlayer == target) return MurderAttemptResult.SuppressKill;

            // Block impostor shielded kill
            if (!ignoreMedic && Medic.shielded != null && Medic.shielded == target) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();
                return MurderAttemptResult.SuppressKill;
            }

            // Handle blank shot
            if (!ignoreBlank && Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId)) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setBlanked(killer.PlayerId, 0);
                return MurderAttemptResult.BlankKill;
            }

            // Block impostor protect kill
            if (GuardianAngel.target != null && GuardianAngel.guardianAngel != null && GuardianAngel.protectActive && GuardianAngel.target == target) {
                return MurderAttemptResult.BlankKill;
            }

            // Block impostor safeguard kill
            if (Survivor.survivor != null && Survivor.safeguardActive && Survivor.survivor == target) {
                return MurderAttemptResult.BlankKill;
            }

            if (TimeLord.shieldActive && TimeLord.timeLord != null && TimeLord.timeLord == target)
            {
                if (!blockRewind) // Only rewind the attempt was not called because a meeting started
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.TimeLordRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeLordRewindTime();
                    revealedTimeLords.Add(target.PlayerId);
                    knownTimeLordKillers.Add(killer.PlayerId);
                }
                return MurderAttemptResult.SuppressKill;
            }

            // Reverse veteran kill
            if (Veteran.veteran != null && Veteran.veteran == target && Veteran.isAlertActive)
            {
                if (Medic.shielded != null && Medic.shielded == target)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt();
                }
                if (Mercenary.shielded != null && Mercenary.shielded == target)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryAddMurder();
                }
                return MurderAttemptResult.ReverseKill;
            }

            // Block impostor mercenary shield kill
            if (Mercenary.shielded != null && Mercenary.shielded == target) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.mercenaryAddMurder();
                return MurderAttemptResult.BlankKill;
            }

            // Block Armored with armor kill
            if (checkArmored(target, true, killer == PlayerControl.LocalPlayer, Sheriff.sheriff == null || killer.PlayerId != Sheriff.sheriff.PlayerId || isNeutral(target) && Sheriff.canKillNeutrals || isNeutralKiller(target) && Sheriff.canKillKNeutrals || isKiller(target))) {
                return MurderAttemptResult.BlankKill;
            }

            // Reverse vh kill
            if (VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == target && (killer.PlayerId == Dracula.dracula.PlayerId || killer.PlayerId == Vampire.vampire.PlayerId)) {
                if (Medic.shielded != null && Medic.shielded == target) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt();
                }
                if (Mercenary.shielded != null && Mercenary.shielded == target) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryAddMurder();
                }
                return MurderAttemptResult.ReverseKill;
            }

            // Reverse vh veteran kill
            if (VampireHunter.veteran != null && VampireHunter.veteran == target && VampireHunter.isAlertActive) {
                if (Medic.shielded != null && Medic.shielded == target) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt();
                }
                if (Mercenary.shielded != null && Mercenary.shielded == target) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryAddMurder, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryAddMurder();
                }
                return MurderAttemptResult.ReverseKill;
            }

            if (TransportationToolPatches.isUsingTransportation(target) && !blockRewind && killer == Poisoner.poisoner) {
                return MurderAttemptResult.DelayPoisonerKill;
            } else if (TransportationToolPatches.isUsingTransportation(target))
                return MurderAttemptResult.SuppressKill;
            return MurderAttemptResult.PerformKill;
        }

        public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation) {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? Byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
        }

        public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false)  {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible
            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);

            if (murder == MurderAttemptResult.PerformKill) {
                MurderPlayer(killer, target, showAnimation);
            } else if (murder == MurderAttemptResult.ReverseKill) {
                checkMurderAttemptAndKill(target, killer);
            } else if (murder == MurderAttemptResult.DelayPoisonerKill) {
                HudManager.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) => { 
                    if (!TransportationToolPatches.isUsingTransportation(target) && Poisoner.poisoned != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                        writer.Write(byte.MaxValue);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue);
                        MurderPlayer(killer, target, showAnimation);
                    }
                })));
            }
            return murder;
        }

        public static bool isNeutral(this PlayerControl player) {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.Neutral;
            return false;
        }

        public static bool isNeutralKiller(this PlayerControl player) {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.NeutralKiller;
            return false;
        }

        public static bool isAnyNeutral(this PlayerControl player) {
            return player.isNeutral() || player.isNeutralKiller();
        }

        public static bool isKiller(this PlayerControl player) {
            return player.Data.Role.IsImpostor || player.isNeutralKiller();
        }

        public static bool isEvil(this PlayerControl player) {
            return player.Data.Role.IsImpostor || player.isAnyNeutral();
        }

        public static bool zoomOutStatus = false;
        public static void toggleZoom(bool reset=false) {
            float orthographicSize = reset || zoomOutStatus ? 3f : 12f;

            zoomOutStatus = !zoomOutStatus && !reset;
            Camera.main.orthographicSize = orthographicSize;
            foreach (var cam in Camera.allCameras) {
                if (cam != null && cam.gameObject.name == "UI Camera") cam.orthographicSize = orthographicSize;  // The UI is scaled too, else we cant click the buttons. Downside: map is super small.
            }

            var tzGO = GameObject.Find("TOGGLEZOOMBUTTON");
            if (tzGO != null) {
                var rend = tzGO.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                var rendActive = tzGO.transform.Find("Active").GetComponent<SpriteRenderer>();
                rend.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_Button.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_Button.png", 100f);
                rendActive.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_ButtonActive.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_ButtonActive.png", 100f);
            }

            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen); // This will move button positions to the correct position.
        }

        public static bool hasImpVision(NetworkedPlayerInfo player) {
            return player.Role.IsImpostor
                || (Jester.jester != null && Jester.jester.PlayerId == player.PlayerId && Jester.hasImpostorVision)
                || ((Dracula.dracula != null && Dracula.dracula.PlayerId == player.PlayerId || Dracula.formerDraculas.Any(x => x.PlayerId == player.PlayerId)) && Dracula.hasImpostorVision)
                || (Vampire.vampire != null && Vampire.vampire.PlayerId == player.PlayerId && Vampire.hasImpostorVision)
                || (Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == player.PlayerId && Juggernaut.hasImpostorVision)
                || (Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == player.PlayerId && Werewolf.isRampageActive)
                || (Glitch.glitch != null && Glitch.glitch.PlayerId == player.PlayerId && Glitch.hasImpostorVision);
        }

        public static SabatageTypes getActiveSabo()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
            {
                if (task.TaskType == TaskTypes.FixLights)
                {
                    return SabatageTypes.Lights;
                }
                else if (task.TaskType == TaskTypes.RestoreOxy)
                {
                    return SabatageTypes.O2;
                }
                else if (task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.StopCharles || task.TaskType == TaskTypes.StopCharles)
                {
                    return SabatageTypes.Reactor;
                }
                else if (task.TaskType == TaskTypes.FixComms)
                {
                    return SabatageTypes.Comms;
                }
            }
            return SabatageTypes.None;
        }

        public static bool isCommsActive()
        {
            return getActiveSabo() == SabatageTypes.Comms;
        }

        public static bool isCamoComms()
        {
            return isCommsActive() && TOUMapOptions.camoComms;
        }

        public static bool isActiveCamoComms()
        {
            return isCamoComms() && Camouflager.camoComms;
        }

        public static bool wasActiveCamoComms()
        {
            return !isCamoComms() && Camouflager.camoComms;
        }

        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }

        public static void handlePoisonerKillOnBodyReport() {
            Helpers.checkMurderAttemptAndKill(Poisoner.poisoner, Poisoner.poisoned, true, false);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
            writer.Write(byte.MaxValue);
            writer.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue);
        }

        public static void turnToImpostor(PlayerControl player) {
            player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
            player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

            foreach (var player2 in PlayerControl.AllPlayerControls)
                if (player2.Data.Role.IsImpostor && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                    player.cosmetics.nameText.color = Palette.ImpostorRed;
        }

        public static void checkWatchFlash(PlayerControl target) {
            if (PlayerControl.LocalPlayer == Investigator.watching) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.InvestigatorWatchFlash, SendOption.Reliable, -1);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.investigatorWatchFlash(target.PlayerId);
            }
        }

        public static bool checkAndDoVetKill(PlayerControl target) {
            bool shouldVetKill = Veteran.veteran == target && Veteran.isAlertActive;
            if (shouldVetKill) {
                checkMurderAttemptAndKill(Veteran.veteran, PlayerControl.LocalPlayer);
            }
            return shouldVetKill;
        }

        public static bool checkAndDoVHVetKill(PlayerControl target) {
            bool shouldVHVetKill = VampireHunter.veteran == target && VampireHunter.isAlertActive;
            if (shouldVHVetKill) {
                checkMurderAttemptAndKill(VampireHunter.veteran, PlayerControl.LocalPlayer);
            }
            return shouldVHVetKill;
        }

        public static IEnumerator BlackmailShhh() {
            yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0f, 0f, 0f, 0.98f));
            var TempPosition = HudManager.Instance.shhhEmblem.transform.localPosition;
            var TempDuration = HudManager.Instance.shhhEmblem.HoldDuration;
            HudManager.Instance.shhhEmblem.transform.localPosition = new Vector3(
                HudManager.Instance.shhhEmblem.transform.localPosition.x,
                HudManager.Instance.shhhEmblem.transform.localPosition.y,
                HudManager.Instance.FullScreen.transform.position.z + 1f);
            HudManager.Instance.shhhEmblem.TextImage.text = "YOU ARE BLACKMAILED";
            HudManager.Instance.shhhEmblem.HoldDuration = 2.5f;
            yield return HudManager.Instance.ShowEmblem(true);
            HudManager.Instance.shhhEmblem.transform.localPosition = TempPosition;
            HudManager.Instance.shhhEmblem.HoldDuration = TempDuration;
            yield return HudManager.Instance.CoFadeFullScreen(new Color(0f, 0f, 0f, 0.98f), Color.clear);
            yield return null;
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> getClosestPlayers(Vector2 truePosition, float radius, bool includeDead) {
            Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            float lightRadius = radius * ShipStatus.Instance.MaxLightRadius;
            Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index) {
                NetworkedPlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected && (!playerInfo.Object.Data.IsDead || includeDead)) {
                    Vector2 vector2 = new Vector2(playerInfo.Object.GetTruePosition().x - truePosition.x, playerInfo.Object.GetTruePosition().y - truePosition.y);
                    float magnitude = vector2.magnitude;
                    if (magnitude <= lightRadius) {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }

        public static void grenadierFlash(Color color, float duration = 10f, float alpha = 1f) {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;

            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            DestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.active = true;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>(p => {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;
                var fadeFraction = 0.5f / duration;

                if (MeetingHud.Instance) {
                    renderer.enabled = false;
                    if (PlayerControl.LocalPlayer.PlayerId == Grenadier.grenadier.PlayerId && Grenadier.flashedPlayers.Count > 0) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                        writer.Write(true);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        Grenadier.flashedPlayers.Clear();
                    }
                    return;
                }

                if (p < fadeFraction) {
                    var fadeInProgress = p / fadeFraction;
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(fadeInProgress * alpha));
                } else if (p > 1 - fadeFraction) {
                    var fadeOutProgress = (p - (1 - fadeFraction)) / fadeFraction;
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - fadeOutProgress) * alpha));

                    if (PlayerControl.LocalPlayer.PlayerId == Grenadier.grenadier.PlayerId && Grenadier.flashedPlayers.Count > 0) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                        writer.Write(true);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        Grenadier.flashedPlayers.Clear();
                    }
                } else {
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, alpha);
                }

                if (p == 1f && renderer != null) renderer.enabled = false;
            })));

            if (Grenadier.flashTimer > 0.5f) {
                try {
                    if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && MapBehaviour.Instance.infectedOverlay.sabSystem.Timer < 0.5f)
                        MapBehaviour.Instance.infectedOverlay.sabSystem.Timer = 0.5f;
                } catch {}
            }
        }

        public static bool isFlashedByGrenadier(this PlayerControl player) {
            return Grenadier.grenadier != null && Grenadier.flashTimer > 0f && Grenadier.flashedPlayers.Contains(player);
        }

        public static IEnumerator Hack(PlayerControl hackPlayer) {
            var lockImg = new GameObject[6];
            if (Glitch.hackedPlayers.ContainsKey(hackPlayer)) {
                Glitch.hackedPlayers[hackPlayer] = DateTime.UtcNow;
                yield break;
            }
            Glitch.hackedPlayers.Add(hackPlayer, DateTime.UtcNow);
            float savedReportDistance = PlayerControl.LocalPlayer.MaxReportDistance;

            bool useButtonEnabled = false;
            bool petButtonEnabled = false;
		    bool reportButtonEnabled = false;
		    bool killButtonEnabled = false;
		    bool saboButtonEnabled = false;
		    bool ventButtonEnabled = false;
            if (PlayerControl.LocalPlayer == hackPlayer) {
                useButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled;
                petButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled;
                reportButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled;
                killButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled;
                saboButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled;
                ventButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled;
            }

            while (true) {
                if (PlayerControl.LocalPlayer == hackPlayer) {
                    // Kill Button
                    if (FastDestroyableSingleton<HudManager>.Instance.KillButton != null && killButtonEnabled) {
                        if (lockImg[0] == null) {
                            lockImg[0] = new GameObject();
                            lockImg[0].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[0] != null) {
                        lockImg[0].layer = 5;
                        lockImg[0].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.position.y, -50f);
                    }

                    // Use Button
                    if (FastDestroyableSingleton<HudManager>.Instance.UseButton != null && useButtonEnabled) {
                        if (lockImg[1] == null) {
                            lockImg[1] = new GameObject();
                            lockImg[1].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[1] != null) {
                        lockImg[1].layer = 5;
                        lockImg[1].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.position.y, -50f);
                    }

                    // Pet Button
                    if (FastDestroyableSingleton<HudManager>.Instance.PetButton != null && petButtonEnabled) {
                        if (lockImg[2] == null) {
                            lockImg[2] = new GameObject();
                            lockImg[2].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[2] != null) {
                        lockImg[2].layer = 5;
                        lockImg[2].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.PetButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.PetButton.transform.position.y, -50f);
                    }

                    // Report Button
                    if (FastDestroyableSingleton<HudManager>.Instance.ReportButton != null && reportButtonEnabled) {
                        if (lockImg[3] == null) {
                            lockImg[3] = new GameObject();
                            lockImg[3].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[3] != null) {
                        lockImg[3].layer = 5;
                        lockImg[3].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.ReportButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.ReportButton.transform.position.y, -50f);
                    }

                    // Sabotage Button
                    if (FastDestroyableSingleton<HudManager>.Instance.SabotageButton != null && saboButtonEnabled) {
                        if (lockImg[4] == null) {
                            lockImg[4] = new GameObject();
                            lockImg[4].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[4] != null) {
                        lockImg[4].layer = 5;
                        lockImg[4].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.SabotageButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.SabotageButton.transform.position.y, -50f);
                    }

                    // Vent Button
                    if (PlayerControl.LocalPlayer.roleCanUseVents()) {
                        if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton != null && ventButtonEnabled) {
                            if (lockImg[5] == null) {
                                lockImg[5] = new GameObject();
                                lockImg[5].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                            }
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.color = Palette.DisabledClear;
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.material.SetFloat("_Desat", 1f);
                        }
                        if (lockImg[5] != null) {
                            lockImg[5].layer = 5;
                            lockImg[5].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.transform.position.y, -50f);
                        }
                    }

                    // Custom Buttons
                    if (PlayerControl.LocalPlayer) {
                        CustomButton.buttons.ForEach(x => x.Hack());
                    }

                    if (Minigame.Instance) {
                        Minigame.Instance.Close();
                        Minigame.Instance.ForceClose();
                    }

                    if (MapBehaviour.Instance) {
                        MapBehaviour.Instance.Close();
                        MapBehaviour.Instance.Close();
                    }

                    PlayerControl.LocalPlayer.MaxReportDistance = 0f;
                }
                double totalHacktime = (DateTime.UtcNow - Glitch.hackedPlayers[hackPlayer]).TotalMilliseconds / 1000;
                double remaining = Math.Round((double)(float)Glitch.hackDuration - totalHacktime);
                if (MeetingHud.Instance || totalHacktime > (double)(float)Glitch.hackDuration || hackPlayer == null || hackPlayer.Data.IsDead)
                    break;
                yield return null;
            }

            GameObject[] array = lockImg;
            foreach (GameObject obj in array)
                if (obj != null)
                    obj.SetActive(false);
            
            if (PlayerControl.LocalPlayer == hackPlayer) {
                FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled = useButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled = petButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = reportButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = killButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled = saboButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled = ventButtonEnabled;
                if (PlayerControl.LocalPlayer) {
                    CustomButton.buttons.ForEach(x => x.UnHack());
                }
                PlayerControl.LocalPlayer.MaxReportDistance = savedReportDistance;
            }
            Glitch.hackedPlayers.Remove(hackPlayer);
            Glitch.hackedPlayer = null;
        }

        public static bool stopGameEndForKillers() {
            bool powerCrewAlive = false;

            if (CustomOptionHolder.sheriffBlockGameEnd.getBool() && Sheriff.sheriff != null && !Sheriff.sheriff.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.veteranBlockGameEnd.getBool() && Veteran.veteran != null && !Veteran.veteran.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.veteranBlockGameEnd.getBool() && VampireHunter.veteran != null && !VampireHunter.veteran.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.mayorBlockGameEnd.getBool() && Mayor.mayor != null && !Mayor.mayor.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.swapperBlockGameEnd.getBool() && Swapper.swapper != null && !Swapper.swapper.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.niceGuesserBlockGameEnd.getBool() && Guesser.niceGuesser != null && !Guesser.niceGuesser.Data.IsDead) powerCrewAlive = true;
            if (CustomOptionHolder.vampireHunterBlockGameEnd.getBool() && VampireHunter.vampireHunter != null && !VampireHunter.vampireHunter.Data.IsDead) powerCrewAlive = true;

            return powerCrewAlive;
        }

        public static void randomPlayersTP()
        {
            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            if (PlayerControl.LocalPlayer.inVent)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
            }

            var SpawnPositions = GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
            {
                0 => MapData.SkeldSpawnPosition,
                1 => MapData.MiraSpawnPosition,
                2 => MapData.PolusSpawnPosition,
                3 => MapData.DleksSpawnPosition,
                4 => MapData.AirshipSpawnPosition,
                5 => MapData.FungleSpawnPosition,
                _ => MapData.FindVentSpawnPositions(),
            };
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SpawnPositions[TownOfUs.rnd.Next(SpawnPositions.Count)]);
        }

        public static bool checkArmored(PlayerControl target, bool breakShield, bool showShield, bool additionalCondition = true) {
            if (target != null && Armored.armored != null && Armored.armored == target && !Armored.isBrokenArmor && additionalCondition) {
                if (breakShield) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BreakArmor, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.breakArmor();
                }
                if (showShield) {
                    target.ShowFailedMurder();
                }
                return true;
            }
            return false;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            for (var i = list.Count - 1; i > 0; --i)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> Shuffle(Il2CppSystem.Collections.Generic.List<PlayerControl> playersToDie)
        {
            var count = playersToDie.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = playersToDie[i];
                playersToDie[i] = playersToDie[r];
                playersToDie[r] = tmp;
            }
            return playersToDie;
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> GetClosestPlayers(Vector2 truePosition, float radius, bool includeDead)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            float lightRadius = radius * ShipStatus.Instance.MaxLightRadius;
            Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index)
            {
                NetworkedPlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected && (!playerInfo.Object.Data.IsDead || includeDead))
                {
                    Vector2 vector2 = new Vector2(playerInfo.Object.GetTruePosition().x - truePosition.x, playerInfo.Object.GetTruePosition().y - truePosition.y);
                    float magnitude = ((Vector2)vector2).magnitude;
                    if (magnitude <= lightRadius)
                    {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }
    }

    public class MapData
    {
        public static readonly List<Vector3> SkeldSpawnPosition =
        [
            new Vector3(-2.2f, 2.2f, 0.0f), //cafeteria. botton. top left.
            new Vector3(0.7f, 2.2f, 0.0f), //caffeteria. button. top right.
            new Vector3(-2.2f, -0.2f, 0.0f), //caffeteria. button. bottom left.
            new Vector3(0.7f, -0.2f, 0.0f), //caffeteria. button. bottom right.
            new Vector3(10.0f, 3.0f, 0.0f), //weapons top
            new Vector3(9.0f, 1.0f, 0.0f), //weapons bottom
            new Vector3(6.5f, -3.5f, 0.0f), //O2
            new Vector3(11.5f, -3.5f, 0.0f), //O2-nav hall
            new Vector3(17.0f, -3.5f, 0.0f), //navigation top
            new Vector3(18.2f, -5.7f, 0.0f), //navigation bottom
            new Vector3(11.5f, -6.5f, 0.0f), //nav-shields top
            new Vector3(9.5f, -8.5f, 0.0f), //nav-shields bottom
            new Vector3(9.2f, -12.2f, 0.0f), //shields top
            new Vector3(8.0f, -14.3f, 0.0f), //shields bottom
            new Vector3(2.5f, -16f, 0.0f), //coms left
            new Vector3(4.2f, -16.4f, 0.0f), //coms middle
            new Vector3(5.5f, -16f, 0.0f), //coms right
            new Vector3(-1.5f, -10.0f, 0.0f), //storage top
            new Vector3(-1.5f, -15.5f, 0.0f), //storage bottom
            new Vector3(-4.5f, -12.5f, 0.0f), //storrage left
            new Vector3(0.3f, -12.5f, 0.0f), //storrage right
            new Vector3(4.5f, -7.5f, 0.0f), //admin top
            new Vector3(4.5f, -9.5f, 0.0f), //admin bottom
            new Vector3(-9.0f, -8.0f, 0.0f), //elec top left
            new Vector3(-6.0f, -8.0f, 0.0f), //elec top right
            new Vector3(-8.0f, -11.0f, 0.0f), //elec bottom
            new Vector3(-12.0f, -13.0f, 0.0f), //elec-lower hall
            new Vector3(-17f, -10f, 0.0f), //lower engine top
            new Vector3(-17.0f, -13.0f, 0.0f), //lower engine bottom
            new Vector3(-21.5f, -3.0f, 0.0f), //reactor top
            new Vector3(-21.5f, -8.0f, 0.0f), //reactor bottom
            new Vector3(-13.0f, -3.0f, 0.0f), //security top
            new Vector3(-12.6f, -5.6f, 0.0f), // security bottom
            new Vector3(-17.0f, 2.5f, 0.0f), //upper engibe top
            new Vector3(-17.0f, -1.0f, 0.0f), //upper engine bottom
            new Vector3(-10.5f, 1.0f, 0.0f), //upper-mad hall
            new Vector3(-10.5f, -2.0f, 0.0f), //medbay top
            new Vector3(-6.5f, -4.5f, 0.0f)
        ];

        public static readonly List<Vector3> MiraSpawnPosition =
        [
            new Vector3(-4.5f, 3.5f, 0.0f), //launchpad top
            new Vector3(-4.5f, -1.4f, 0.0f), //launchpad bottom
            new Vector3(8.5f, -1f, 0.0f), //launchpad- med hall
            new Vector3(14f, -1.5f, 0.0f), //medbay
            new Vector3(16.5f, 3f, 0.0f), // comms
            new Vector3(10f, 5f, 0.0f), //lockers
            new Vector3(6f, 1.5f, 0.0f), //locker room
            new Vector3(2.5f, 13.6f, 0.0f), //reactor
            new Vector3(6f, 12f, 0.0f), //reactor middle
            new Vector3(9.5f, 13f, 0.0f), //lab
            new Vector3(15f, 9f, 0.0f), //bottom left cross
            new Vector3(17.9f, 11.5f, 0.0f), //middle cross
            new Vector3(14f, 17.3f, 0.0f), //office
            new Vector3(19.5f, 21f, 0.0f), //admin
            new Vector3(14f, 24f, 0.0f), //greenhouse left
            new Vector3(22f, 24f, 0.0f), //greenhouse right
            new Vector3(21f, 8.5f, 0.0f), //bottom right cross
            new Vector3(28f, 3f, 0.0f), //caf right
            new Vector3(22f, 3f, 0.0f), //caf left
            new Vector3(19f, 4f, 0.0f), //storage
            new Vector3(22f, -2f, 0.0f)
        ];

        public static readonly List<Vector3> PolusSpawnPosition =
        [
            new Vector3(16.6f, -1f, 0.0f), //dropship top
            new Vector3(16.6f, -5f, 0.0f), //dropship bottom
            new Vector3(20f, -9f, 0.0f), //above storrage
            new Vector3(22f, -7f, 0.0f), //right fuel
            new Vector3(25.5f, -6.9f, 0.0f), //drill
            new Vector3(29f, -9.5f, 0.0f), //lab lockers
            new Vector3(29.5f, -8f, 0.0f), //lab weather notes
            new Vector3(35f, -7.6f, 0.0f), //lab table
            new Vector3(40.4f, -8f, 0.0f), //lab scan
            new Vector3(33f, -10f, 0.0f), //lab toilet
            new Vector3(39f, -15f, 0.0f), //specimen hall top
            new Vector3(36.5f, -19.5f, 0.0f), //specimen top
            new Vector3(36.5f, -21f, 0.0f), //specimen bottom
            new Vector3(28f, -21f, 0.0f), //specimen hall bottom
            new Vector3(24f, -20.5f, 0.0f), //admin tv
            new Vector3(22f, -25f, 0.0f), //admin books
            new Vector3(16.6f, -17.5f, 0.0f), //office coffe
            new Vector3(22.5f, -16.5f, 0.0f), //office projector
            new Vector3(24f, -17f, 0.0f), //office figure
            new Vector3(27f, -16.5f, 0.0f), //office lifelines
            new Vector3(32.7f, -15.7f, 0.0f), //lavapool
            new Vector3(31.5f, -12f, 0.0f), //snowmad below lab
            new Vector3(10f, -14f, 0.0f), //below storrage
            new Vector3(21.5f, -12.5f, 0.0f), //storrage vent
            new Vector3(19f, -11f, 0.0f), //storrage toolrack
            new Vector3(12f, -7f, 0.0f), //left fuel
            new Vector3(5f, -7.5f, 0.0f), //above elec
            new Vector3(10f, -12f, 0.0f), //elec fence
            new Vector3(9f, -9f, 0.0f), //elec lockers
            new Vector3(5f, -9f, 0.0f), //elec window
            new Vector3(4f, -11.2f, 0.0f), //elec tapes
            new Vector3(5.5f, -16f, 0.0f), //elec-O2 hall
            new Vector3(1.2f, -17.5f, 0.0f), //O2 tree hayball
            new Vector3(3f, -21f, 0.0f), //O2 middle
            new Vector3(2f, -19f, 0.0f), //O2 gas
            new Vector3(1.4f, -24f, 0.0f), //O2 water
            new Vector3(7f, -24f, 0.0f), //under O2
            new Vector3(9f, -20f, 0.0f), //right outside of O2
            new Vector3(7f, -15.8f, 0.0f), //snowman under elec
            new Vector3(11f, -17f, 0.0f), //comms table
            new Vector3(12.7f, -15.5f, 0.0f), //coms antenna pult
            new Vector3(13f, -24.5f, 0.0f), //weapons window
            new Vector3(15f, -17f, 0.0f), //between coms-office
            new Vector3(17.5f, -25.7f, 0.0f), //snowman under office
        ];

        public static readonly List<Vector3> DleksSpawnPosition = // No Dleks
        [
            new Vector3(2.2f, 2.2f, 0.0f), //cafeteria. botton. top left.
            new Vector3(-0.7f, 2.2f, 0.0f), //caffeteria. button. top right.
            new Vector3(2.2f, -0.2f, 0.0f), //caffeteria. button. bottom left.
            new Vector3(-0.7f, -0.2f, 0.0f), //caffeteria. button. bottom right.
            new Vector3(-10.0f, 3.0f, 0.0f), //weapons top
            new Vector3(-9.0f, 1.0f, 0.0f), //weapons bottom
            new Vector3(-6.5f, -3.5f, 0.0f), //O2
            new Vector3(-11.5f, -3.5f, 0.0f), //O2-nav hall
            new Vector3(-17.0f, -3.5f, 0.0f), //navigation top
            new Vector3(-18.2f, -5.7f, 0.0f), //navigation bottom
            new Vector3(-11.5f, -6.5f, 0.0f), //nav-shields top
            new Vector3(-9.5f, -8.5f, 0.0f), //nav-shields bottom
            new Vector3(-9.2f, -12.2f, 0.0f), //shields top
            new Vector3(-8.0f, -14.3f, 0.0f), //shields bottom
            new Vector3(-2.5f, -16f, 0.0f), //coms left
            new Vector3(-4.2f, -16.4f, 0.0f), //coms middle
            new Vector3(-5.5f, -16f, 0.0f), //coms right
            new Vector3(1.5f, -10.0f, 0.0f), //storage top
            new Vector3(1.5f, -15.5f, 0.0f), //storage bottom
            new Vector3(4.5f, -12.5f, 0.0f), //storrage left
            new Vector3(-0.3f, -12.5f, 0.0f), //storrage right
            new Vector3(-4.5f, -7.5f, 0.0f), //admin top
            new Vector3(-4.5f, -9.5f, 0.0f), //admin bottom
            new Vector3(9.0f, -8.0f, 0.0f), //elec top left
            new Vector3(6.0f, -8.0f, 0.0f), //elec top right
            new Vector3(8.0f, -11.0f, 0.0f), //elec bottom
            new Vector3(12.0f, -13.0f, 0.0f), //elec-lower hall
            new Vector3(17f, -10f, 0.0f), //lower engine top
            new Vector3(17.0f, -13.0f, 0.0f), //lower engine bottom
            new Vector3(21.5f, -3.0f, 0.0f), //reactor top
            new Vector3(21.5f, -8.0f, 0.0f), //reactor bottom
            new Vector3(13.0f, -3.0f, 0.0f), //security top
            new Vector3(12.6f, -5.6f, 0.0f), // security bottom
            new Vector3(17.0f, 2.5f, 0.0f), //upper engibe top
            new Vector3(17.0f, -1.0f, 0.0f), //upper engine bottom
            new Vector3(10.5f, 1.0f, 0.0f), //upper-mad hall
            new Vector3(10.5f, -2.0f, 0.0f), //medbay top
            new Vector3(6.5f, -4.5f, 0.0f)
        ];

        public static readonly List<Vector3> AirshipSpawnPosition =
        [
            // No Spawn Position for airships
        ];

        public static readonly List<Vector3> FungleSpawnPosition =
        [
            new Vector3(-10.0842f, 13.0026f, 0.013f),
            new Vector3(0.9815f, 6.7968f, 0.0068f),
            new Vector3(22.5621f, 3.2779f, 0.0033f),
            new Vector3(-1.8699f, -1.3406f, -0.0013f),
            new Vector3(12.0036f, 2.6763f, 0.0027f),
            new Vector3(21.705f, -7.8691f, -0.0079f),
            new Vector3(1.4485f, -1.6105f, -0.0016f),
            new Vector3(-4.0766f, -8.7178f, -0.0087f),
            new Vector3(2.9486f, 1.1347f, 0.0011f),
            new Vector3(-4.2181f, -8.6795f, -0.0087f),
            new Vector3(19.5553f, -12.5014f, -0.0125f),
            new Vector3(15.2497f, -16.5009f, -0.0165f),
            new Vector3(-22.7174f, -7.0523f, 0.0071f),
            new Vector3(-16.5819f, -2.1575f, 0.0022f),
            new Vector3(9.399f, -9.7127f, -0.0097f),
            new Vector3(7.3723f, 1.7373f, 0.0017f),
            new Vector3(22.0777f, -7.9315f, -0.0079f),
            new Vector3(-15.3916f, -9.3659f, -0.0094f),
            new Vector3(-16.1207f, -0.1746f, -0.0002f),
            new Vector3(-23.1353f, -7.2472f, -0.0072f),
            new Vector3(-20.0692f, -2.6245f, -0.0026f),
            new Vector3(-4.2181f, -8.6795f, -0.0087f),
            new Vector3(-9.9285f, 12.9848f, 0.013f),
            new Vector3(-8.3475f, 1.6215f, 0.0016f),
            new Vector3(-17.7614f, 6.9115f, 0.0069f),
            new Vector3(-0.5743f, -4.7235f, -0.0047f),
            new Vector3(-20.8897f, 2.7606f, 0.002f)
        ];

        public static List<Vector3> MapSpawnPosition()
        {
            return GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
            {
                0 => SkeldSpawnPosition,
                1 => MiraSpawnPosition,
                2 => PolusSpawnPosition,
                3 => DleksSpawnPosition,
                4 => AirshipSpawnPosition,
                5 => FungleSpawnPosition,
                _ => FindVentSpawnPositions()
            };
        }

        public static List<Vector3> FindVentSpawnPositions()
        {
            var poss = new List<Vector3>();
            foreach (var vent in DestroyableSingleton<ShipStatus>.Instance.AllVents)
            {
                var Transform = vent.transform;
                var position = Transform.position;
                poss.Add(new Vector3(position.x, position.y + 0.3f, position.z = 0.0f));
            }

            return poss;
        }

        public static readonly Dictionary<PlayerControl, Vent> PlayerVentDic = new();

        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
        [HarmonyPostfix]
        public static void OnEnterVent(PlayerControl pc, Vent __instance)
        {
            PlayerVentDic[pc] = __instance;
        }

        [HarmonyPatch(typeof(Vent._ExitVent_d__40), nameof(Vent._ExitVent_d__40.MoveNext))]
        [HarmonyPostfix]
        public static void OnExitVent(Vent._ExitVent_d__40 __instance)
        {
            if (PlayerVentDic.ContainsKey(__instance.pc)) PlayerVentDic.Remove(__instance.pc);
        }

        public static void AllPlayerExitVent()
        {
            foreach (var (player, vent) in PlayerVentDic) player.MyPhysics.RpcExitVent(vent.Id);
        }
    }
}