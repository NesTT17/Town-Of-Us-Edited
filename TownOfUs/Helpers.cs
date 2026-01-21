using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs
{
    public enum CustomGamemodes { Classic, Guesser }
    public enum BlockOptions { Off, Emergency, Always }
    public enum MurderAttemptResult { PerformKill, SuppressKill, BlankKill, DelayPoisonerKill, SurvivorReset, GAReset, ReverseKill }
    public enum FactionId { Crewmate, NeutralBenign, NeutralEvil, NeutralKilling, Impostor, Modifier }

    public static class Helpers
    {
        public static string previousEndGameSummary = "";
        public static Dictionary<string, Sprite> CachedSprites = new();
        public static bool GameStarted => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Started && !AmongUsClient.Instance.IsGameOver && ShipStatus.Instance != null;

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
                TownOfUsPlugin.Logger.LogError("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static unsafe Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                TownOfUsPlugin.Logger.LogError("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                    ImageConversion.LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                TownOfUsPlugin.Logger.LogError("Error loading texture from disk: " + path);
            }
            return null;
        }

        public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOUEd_AUDIO_CLIP")
        {
            // must be "raw (headerless) 2-channel signed 32 bit pcm (le) 48kHz" (can e.g. use Audacity® to export )
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int)stream.Length);
                float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
                int offset;
                for (int i = 0; i < samples.Length; i++)
                {
                    offset = i * 4;
                    samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
                }
                int channels = 2;
                int sampleRate = 48000;
                AudioClip audioClip = AudioClip.Create(clipName, samples.Length / 2, channels, sampleRate, false);
                audioClip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                audioClip.SetData(samples, 0);
                return audioClip;
            }
            catch
            {
                System.Console.WriteLine("Error loading AudioClip from resources: " + path);
            }
            return null;
        }

        public static async Task DownloadFile(string url, string path)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = await client.GetByteArrayAsync(url);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllBytesAsync(path, data);
            }
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static void refreshRoleDescription(PlayerControl player)
        {
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

            foreach (PlayerTask t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (string title in taskTexts)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = title;
                player.myTasks.Insert(0, task);
            }
        }

        internal static string getRoleString(RoleInfo roleInfo)
        {
            return cs(roleInfo.color, roleInfo.factionId == FactionId.Modifier ? $"Modifier: <b>{roleInfo.name}</b>\n{roleInfo.shortDescription}" : $"Role: <b>{roleInfo.name}</b>\n{roleInfo.shortDescription}");
        }

        public static bool isD(byte playerId)
        {
            return playerId % 2 == 0;
        }

        public static bool isLighterColor(PlayerControl target)
        {
            return isD(target.PlayerId);
        }

        public static bool hasFakeTasks(this PlayerControl player) => player.isAnyNeutral();
        public static bool canBeErased(this PlayerControl player) => !player.isKiller();
        public static bool shouldShowGhostInfo() => PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeInformation && !PlayerControl.LocalPlayer.isFlashedByGrenadier() || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;

        public static void clearAllTasks(this PlayerControl player)
        {
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
            player.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
        }

        public static bool isSkeld() => GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
        public static bool isMira() => GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
        public static bool isPolus() => GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
        public static bool isAirship() => GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
        public static bool isFungle() => GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
        public static bool MushroomSabotageActive() => PlayerControl.LocalPlayer.myTasks.ToArray().Any((x) => x.TaskType == TaskTypes.MushroomMixupSabotage);

        public static bool sabotageActive()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            return sabSystem.AnyActive;
        }

        public static float sabotageTimer()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            return sabSystem.Timer;
        }

        public static bool canUseSabotage()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            ISystemType systemType;
            IActivatable doors = null;
            if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out systemType))
            {
                doors = systemType.CastFast<IActivatable>();
            }
            return GameManager.Instance.SabotagesEnabled() && sabSystem.Timer <= 0f && !sabSystem.AnyActive && !(doors != null && doors.IsActive);
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value)
        {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts)
        {
            return t.GetString(key, parts);
        }

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        public static int lineCount(string text)
        {
            return text.Count(c => c == '\n');
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
        {
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

        public static bool hidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (Camouflager.camouflageTimer > 0f || MushroomSabotageActive()) return true; // No names are visible

            if (Swooper.swooper == target && Swooper.isInvisible) return true;
            else if (Venerer.venerer == target && Venerer.abilityTimer > 0f) return true;
            else if (!hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Vampire.vampire && Vampire.wasTeamRed || target == Dracula.dracula && Dracula.wasTeamRed)) return false;
            else if ((source == Dracula.dracula || source == Vampire.vampire) && (target == Dracula.dracula || target == Vampire.vampire || target == Dracula.fakeVampire)) return false;
            else if ((source == Lovers.lover1 || source == Lovers.lover2) && (target == Lovers.lover1 || target == Lovers.lover2)) return false;
            return true;
        }

        public static void setDefaultLook(this PlayerControl target)
        {
            if (Helpers.MushroomSabotageActive())
            {
                var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
                MushroomMixupSabotageSystem.CondensedOutfit condensedOutfit = instance.currentMixups[target.PlayerId];
                NetworkedPlayerInfo.PlayerOutfit playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
                target.MixUpOutfit(playerOutfit);
            }
            else
                target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId)
        {
            target.RawSetColor(colorId);
            target.RawSetVisor(visorId, colorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);

            SkinViewData nextSkin = null;
            try { nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId); } catch { return; }

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

            Shy.update(); // so that morphling and camo wont make the shy visible
        }

        public static void showFlash(Color color, float duration = 1f, string message = "")
        {
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
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
                if (p == 1f) messageText.gameObject.Destroy();
            })));
        }

        public static bool roleCanUseVents(this PlayerControl player)
        {
            bool roleCouldUse = false;
            if (Engineer.engineer != null && Engineer.engineer == player)
                roleCouldUse = true;
            else if (Jester.canEnterVents && player.IsJester(out _))
                roleCouldUse = true;
            else if (Thief.canUseVents && player.IsThief(out _))
                roleCouldUse = true;
            else if (Juggernaut.juggernaut != null && Juggernaut.juggernaut == player && Juggernaut.canVent)
                roleCouldUse = true;
            else if (Arsonist.arsonist != null && Arsonist.arsonist == player && Arsonist.canVent)
                roleCouldUse = true;
            else if (Werewolf.werewolf != null && Werewolf.werewolf == player && Werewolf.isRampageActive && Werewolf.canVent)
                roleCouldUse = true;
            else if (Dracula.canUseVents && Dracula.dracula != null && Dracula.dracula == player)
                roleCouldUse = true;
            else if (Vampire.canUseVents && Vampire.vampire != null && Vampire.vampire == player)
                roleCouldUse = true;
            else if (Pestilence.canVent && Pestilence.pestilence != null && Pestilence.pestilence == player)
                roleCouldUse = true;
            else if (Glitch.canVent && Glitch.glitch != null && Glitch.glitch == player)
                roleCouldUse = true;
            else if (Cannibal.canVent && Cannibal.cannibal != null && Cannibal.cannibal == player)
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                if (Camouflager.camouflager != null && Camouflager.camouflager == player && !Camouflager.canVent 
                    || Morphling.morphling != null && Morphling.morphling == player && !Morphling.canVent
                    || Swooper.swooper != null && Swooper.swooper == player && !Swooper.canVent
                    || Undertaker.undertaker != null && Undertaker.undertaker == player && !Undertaker.canVent
                    || Undertaker.undertaker != null && Undertaker.undertaker == player && Undertaker.dragedBody != null && !Undertaker.canDragAndVent
                    || Poisoner.poisoner != null && Poisoner.poisoner == player && !Poisoner.canVent
                    || Escapist.escapist != null && Escapist.escapist == player && !Escapist.canVent)
                {
                    roleCouldUse = false;
                }
                else
                {
                    roleCouldUse = true;
                }
            }
            return roleCouldUse;
        }

        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false, bool ignoreMedic = false)
        {
            var targetRole = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return MurderAttemptResult.PerformKill;

            // Handle blank shot
            if (Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setBlanked(killer.PlayerId, 0);
            }

            // Handle first kill attempt
            if (shieldFirstKill && firstKillPlayer == target && killer != Ruthless.ruthless) return MurderAttemptResult.SuppressKill;

            if (target.IsSurvivor(out Survivor survivor) && survivor.safeguardActive)
            {
                if (killer == Ruthless.ruthless) return MurderAttemptResult.PerformKill;
                else return MurderAttemptResult.SurvivorReset;
            }

            if (GuardianAngel.target != null && GuardianAngel.target == target && GuardianAngel.isProtectActive)
            {
                if (killer == Ruthless.ruthless) return MurderAttemptResult.PerformKill;
                else return MurderAttemptResult.GAReset;
            }

            if (Pestilence.pestilence != null && target == Pestilence.pestilence)
            {
                if (killer == Ruthless.ruthless) return MurderAttemptResult.PerformKill;
                else return MurderAttemptResult.ReverseKill;
            }

            if (target.IsVeteran(out Veteran veteran) && veteran.alertActive)
            {
                if (killer != Pestilence.pestilence) return MurderAttemptResult.ReverseKill;
                else if (killer == Pestilence.pestilence || killer == Ruthless.ruthless) return MurderAttemptResult.SuppressKill;
            }

            if (Mayor.mayor != null && Mayor.mayor == target && Mayor.isBodyguardActive)
            {
                if (killer != Pestilence.pestilence) return MurderAttemptResult.ReverseKill;
                else if (killer == Pestilence.pestilence || killer == Ruthless.ruthless) return MurderAttemptResult.SuppressKill;
            }

            if (VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == target && (killer == Dracula.dracula || killer == Vampire.vampire))
            {
                return MurderAttemptResult.ReverseKill;
            }

            // Block impostor shielded kill
            if (!ignoreMedic && Medic.shielded != null && Medic.shielded == target && killer != Ruthless.ruthless)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();
                SoundEffectsManager.play("shieldedMurderAttempt");
                return MurderAttemptResult.SuppressKill;
            }

            // Block impostor not fully grown mini kill
            if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp())
            {
                return MurderAttemptResult.SuppressKill;
            }

            if (Thief.isFailedThiefKill(target, killer, targetRole) && killer.IsThief(out Thief thief))
            {
                if (target.IsSurvivor(out Survivor survivor2) && survivor2.safeguardActive)
                {
                    return MurderAttemptResult.SurvivorReset;
                }
                else if (target.IsVeteran(out Veteran veteran2) && veteran2.alertActive)
                {
                    return MurderAttemptResult.ReverseKill;
                }
                else if (!ignoreMedic && Medic.shielded != null && Medic.shielded == target)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt();
                    SoundEffectsManager.play("shieldedMurderAttempt");
                    return MurderAttemptResult.SuppressKill;
                }
                else if (GuardianAngel.target != null && GuardianAngel.target == target && GuardianAngel.isProtectActive)
                {
                    return MurderAttemptResult.GAReset;
                }
                else if (Mayor.mayor != null && Mayor.mayor == target && Mayor.isBodyguardActive)
                {
                    return MurderAttemptResult.ReverseKill;
                }
                else if (Pestilence.pestilence != null && target == Pestilence.pestilence)
                {
                    return MurderAttemptResult.ReverseKill;
                }
                else if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp())
                {
                    return MurderAttemptResult.SuppressKill;
                }
                else if (shieldFirstKill && firstKillPlayer == target)
                {
                    return MurderAttemptResult.SuppressKill;
                }
                else
                {
                    thief.suicideFlag = true;
                    return MurderAttemptResult.SuppressKill;
                }
            }

            if (TransportationToolPatches.isUsingTransportation(target) && !blockRewind && killer == Poisoner.poisoner)
            {
                return MurderAttemptResult.DelayPoisonerKill;
            }
            else if (TransportationToolPatches.isUsingTransportation(target))
                return MurderAttemptResult.SuppressKill;
            return MurderAttemptResult.PerformKill;
        }

        public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false)
        {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible
            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);

            if (murder == MurderAttemptResult.PerformKill)
            {
                MurderPlayer(killer, target, showAnimation);
            }
            else if (murder == MurderAttemptResult.ReverseKill)
            {
                if (checkMurderAttemptAndKill(target, killer) == MurderAttemptResult.PerformKill)
                {
                    if (killer.isEvil()) target.addGameInfoRpc(InfoType.AddCorrectShot);
                    else target.addGameInfoRpc(InfoType.AddMisfire);
                }
            }
            else if (murder == MurderAttemptResult.DelayPoisonerKill)
            {
                HudManager.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) =>
                {
                    if (!TransportationToolPatches.isUsingTransportation(target) && Poisoner.poisoned != null)
                    {
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

        public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? Byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
        }

        public static void shareGameVersion()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.Write((byte)TownOfUsPlugin.Version.Major);
            writer.Write((byte)TownOfUsPlugin.Version.Minor);
            writer.Write((byte)TownOfUsPlugin.Version.Build);
            writer.Write(AmongUsClient.Instance.AmHost ? Patches.GameStartManagerPatch.timer : -1f);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(TownOfUsPlugin.Version.Revision < 0 ? 0xFF : TownOfUsPlugin.Version.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.versionHandshake(TownOfUsPlugin.Version.Major, TownOfUsPlugin.Version.Minor, TownOfUsPlugin.Version.Build, TownOfUsPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
        }

        public static void AddModSettingsChangeMessage(this NotificationPopper popper, StringNames key, string value, string option, bool playSound = true)
        {
            string str = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyChangeSettingNotification, "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + option + "</font>", "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>");
            popper.SettingsChangeMessageLogic(key, str, playSound);
        }

        public static bool isNeutralBenign(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.NeutralBenign;
            return false;
        }

        public static bool isNeutralEvil(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.NeutralEvil;
            return false;
        }

        public static bool isNeutralKilling(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.NeutralKilling;
            return false;
        }

        public static bool isAnyNeutral(this PlayerControl player) => player.isNeutralBenign() || player.isNeutralEvil() || player.isNeutralKilling();
        public static bool isKiller(this PlayerControl player) => player.Data.Role.IsImpostor || player.isNeutralKilling();
        public static bool isEvil(this PlayerControl player) => player.Data.Role.IsImpostor || player.isAnyNeutral();

        public static bool zoomOutStatus = false;
        public static IEnumerator zoomOut()
        {
            for (var ft = Camera.main!.orthographicSize; ft < 12f; ft += 0.3f)
            {
                Camera.main.orthographicSize = MeetingHud.Instance ? 3f : ft;
                ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
                foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main.orthographicSize;
                yield return null;
            }
            foreach (var cam in Camera.allCameras) cam.orthographicSize = 12f;
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

                var tzGO = GameObject.Find("TOGGLEZOOMBUTTON");
                if (tzGO != null)
                {
                    var rend = tzGO.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                    var rendActive = tzGO.transform.Find("Active").GetComponent<SpriteRenderer>();
                    rend.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_Button.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_Button.png", 100f);
                    rendActive.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_ButtonActive.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_ButtonActive.png", 100f);
                }
            }
            else
            {
                Coroutines.Start(zoomIn());
                zoomOutStatus = false;

                var tzGO = GameObject.Find("TOGGLEZOOMBUTTON");
                if (tzGO != null)
                {
                    var rend = tzGO.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                    var rendActive = tzGO.transform.Find("Active").GetComponent<SpriteRenderer>();
                    rend.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_Button.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_Button.png", 100f);
                    rendActive.sprite = zoomOutStatus ? Helpers.loadSpriteFromResources("TownOfUs.Resources.Plus_ButtonActive.png", 100f) : Helpers.loadSpriteFromResources("TownOfUs.Resources.Minus_ButtonActive.png", 100f);
                }
            }
        }

        public static bool hasImpVision(this NetworkedPlayerInfo player)
        {
            return player.Role.IsImpostor
                || playerById(player.PlayerId).IsJester(out _) && Jester.hasImpostorVision
                || Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == player.PlayerId && Juggernaut.hasImpostorVision
                || playerById(player.PlayerId).IsThief(out _) && Thief.hasImpostorVision
                || Arsonist.arsonist != null && Arsonist.arsonist.PlayerId == player.PlayerId && Arsonist.hasImpostorVision
                || Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == player.PlayerId && Werewolf.isRampageActive && Werewolf.hasImpostorVision
                || (Dracula.dracula != null && Dracula.dracula.PlayerId == player.PlayerId || Dracula.formerDraculas.Any(x => x.PlayerId == player.PlayerId)) && Dracula.hasImpostorVision
                || Vampire.vampire != null && Vampire.vampire.PlayerId == player.PlayerId && Vampire.hasImpostorVision
                || Pestilence.pestilence != null && Pestilence.pestilence.PlayerId == player.PlayerId && Pestilence.hasImpostorVision
                || Glitch.glitch != null && Glitch.glitch.PlayerId == player.PlayerId && Glitch.hasImpostorVision;
        }

        public static void addGameInfoRpc(this PlayerControl player, params InfoType[] infoTypes)
        {
            foreach (InfoType infoType in infoTypes)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.AddGameInfo, SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                writer.Write((byte)infoType);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.addGameInfo(player.PlayerId, infoType);
            }
        }

        public static bool IsWinner(this string playerName)
        {
            var flag = false;
            var winners = EndGameResult.CachedWinners;

            foreach (var win in winners)
            {
                if (win.PlayerName == playerName)
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }

        public static void erasePlayerRoles(byte playerId, bool ignoreModifier = true, bool addHistory = false)
        {
            PlayerControl player = playerById(playerId);
            if (player == null) return;

            // Crewmate roles
            if (player.IsSheriff(out _)) Sheriff.RemoveSheriff(player.PlayerId);
            if (player == Seer.seer) Seer.clearAndReload();
            if (player == Engineer.engineer) Engineer.clearAndReload();
            if (player == Snitch.snitch) Snitch.clearAndReload();
            if (player.IsVeteran(out _)) Veteran.RemoveVeteran(player.PlayerId);
            if (player == Medic.medic) Medic.clearAndReload(false);
            if (player == Swapper.swapper) Swapper.clearAndReload();
            if (player == Investigator.investigator) Investigator.clearAndReload();
            if (player == Spy.spy) Spy.clearAndReload();
            if (player == Vigilante.vigilante) Vigilante.clearAndReload();
            if (player == Tracker.tracker) Tracker.clearAndReload();
            if (player == Trapper.trapper) Trapper.clearAndReload();
            if (player == Detective.detective) Detective.clearAndReload();
            if (player == Mystic.mystic) Mystic.clearAndReload();
            if (player == Politician.politician) Politician.clearAndReload();
            if (player == Mayor.mayor) Mayor.clearAndReload();
            if (player == VampireHunter.vampireHunter) VampireHunter.clearAndReload();
            if (player == Lookout.lookout) Lookout.clearAndReload();
            if (player == Plumber.plumber) Plumber.clearAndReload();

            // Neutral roles
            if (player.IsJester(out _)) Jester.RemoveJester(player.PlayerId);
            if (player.IsSurvivor(out _)) Survivor.RemoveSurvivor(player.PlayerId);
            if (player == Juggernaut.juggernaut) Juggernaut.clearAndReload();
            if (player.IsPursuer(out _)) Pursuer.RemovePursuer(player.PlayerId);
            if (player.IsAmnesiac(out _)) Amnesiac.RemoveAmnesiac(player.PlayerId);
            if (player.IsThief(out _)) Thief.RemoveThief(player.PlayerId);
            if (player == Lawyer.lawyer) Lawyer.clearAndReload(false);
            if (player == Executioner.executioner) Executioner.clearAndReload(false);
            if (player == GuardianAngel.guardianAngel) GuardianAngel.clearAndReload(false);
            if (player == Arsonist.arsonist) Arsonist.clearAndReload();
            if (player == Werewolf.werewolf) Werewolf.clearAndReload();
            if (player == Dracula.dracula) Dracula.clearAndReload();
            if (player == Vampire.vampire) Vampire.clearAndReload();
            if (player == Plaguebearer.plaguebearer) Plaguebearer.clearAndReload();
            if (player == Pestilence.pestilence) Pestilence.clearAndReload();
            if (player == Doomsayer.doomsayer) Doomsayer.clearAndReload();
            if (player == Glitch.glitch) Glitch.clearAndReload();
            if (player == Cannibal.cannibal) Cannibal.clearAndReload();

            // Impostor roles
            if (player == Camouflager.camouflager) Camouflager.clearAndReload();
            if (player == Morphling.morphling) Morphling.clearAndReload();
            if (player == Assassin.assassin) Assassin.clearAndReload();
            if (player == Swooper.swooper) Swooper.clearAndReload();
            if (player == Miner.miner) Miner.clearAndReload();
            if (player == Janitor.janitor) Janitor.clearAndReload();
            if (player == Undertaker.undertaker) Undertaker.clearAndReload();
            if (player == Grenadier.grenadier) Grenadier.clearAndReload();
            if (player == Blackmailer.blackmailer) Blackmailer.clearAndReload();
            if (player == Poisoner.poisoner) Poisoner.clearAndReload();
            if (player == Venerer.venerer) Venerer.clearAndReload();
            if (player == Scavenger.scavenger) Scavenger.clearAndReload();
            if (player == Escapist.escapist) Escapist.clearAndReload();

            if (!ignoreModifier)
            {
                if (Bait.bait.Any(x => x.PlayerId == player.PlayerId)) Bait.bait.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Blind.blind) Blind.clearAndReload();
                if (player == ButtonBarry.buttonBarry) ButtonBarry.clearAndReload();
                if (Shy.shy.Any(x => x.PlayerId == player.PlayerId)) Shy.shy.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Flash.flash.Any(x => x.PlayerId == player.PlayerId)) Flash.flash.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Mini.mini) Mini.clearAndReload();
                if (player == Indomitable.indomitable) Indomitable.clearAndReload();
                if (player == Lovers.lover1 || player == Lovers.lover2) Lovers.clearAndReload();
                if (Multitasker.multitasker.Any(x => x.PlayerId == player.PlayerId)) Multitasker.multitasker.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Radar.radar) Radar.clearAndReload();
                if (player == Sleuth.sleuth) Sleuth.clearAndReload();
                if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
                if (Torch.torch.Any(x => x.PlayerId == player.PlayerId)) Torch.torch.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Vip.vip.Any(x => x.PlayerId == player.PlayerId)) Vip.vip.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Drunk.drunk.Any(x => x.PlayerId == player.PlayerId)) Drunk.drunk.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Immovable.immovable.Any(x => x.PlayerId == player.PlayerId)) Immovable.immovable.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == DoubleShot.doubleShot) DoubleShot.clearAndReload();
                if (player == Ruthless.ruthless) Ruthless.clearAndReload();
                if (player == Underdog.underdog) Underdog.clearAndReload();
                if (player == Saboteur.saboteur) Saboteur.clearAndReload();
                if (player == Frosty.frosty) Frosty.clearAndReload();
                if (player == Satelite.satelite) Satelite.clearAndReload();
                if (player == SixthSense.sixthSense) SixthSense.clearAndReload();
                if (player == Taskmaster.taskMaster) Taskmaster.clearAndReload();
                if (player == Disperser.disperser) Disperser.clearAndReload();
            }

            if (addHistory)
            {
                PlayerGameInfo.AddRole(player.PlayerId, player.Data.Role.IsImpostor ? RoleInfo.impostor : RoleInfo.crewmate);
            }
        }

        public static bool checkSuspendAction(PlayerControl source, PlayerControl target)
        {
            if (target.IsVeteran(out Veteran veteran) && veteran.alertActive)
            {
                MurderAttemptResult murder = checkMurderAttemptAndKill(target, source);
                if (murder == MurderAttemptResult.PerformKill)
                {
                    if (source.isEvil()) target.addGameInfoRpc(InfoType.AddCorrectShot);
                    else target.addGameInfoRpc(InfoType.AddMisfire);
                }
                return true;
            }
            else if (Pestilence.pestilence != null && Pestilence.pestilence == target)
            {
                MurderAttemptResult murder = checkMurderAttemptAndKill(target, source);
                if (murder == MurderAttemptResult.PerformKill)
                {
                    target.addGameInfoRpc(InfoType.AddKill);
                }
                return true;
            }
            else if (VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == target && (source == Dracula.dracula || source == Vampire.vampire))
            {
                MurderAttemptResult murder = checkMurderAttemptAndKill(target, source);
                if (murder == MurderAttemptResult.PerformKill)
                {
                    target.addGameInfoRpc(InfoType.AddCorrectShot);
                }
                return true;
            }
            else return false;
        }

        public static void showFlashForSixthSense(this PlayerControl source, PlayerControl target)
        {
            if (SixthSense.sixthSense != null && SixthSense.sixthSense == target)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(source.NetId, (byte)CustomRPC.SixthSenseAbilityTrigger, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sixthSenseAbilityTrigger();
            }
        }

        public static bool CanUseMeetingAbility(this PlayerControl player)
        {
            return Blackmailer.blackmailed != null && Blackmailer.blackmailed == player && Blackmailer.blockTargetAbility;
        }

        public static IEnumerator BlackmailShhh()
        {
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

        public static Vector3 convertPos(int index, int arrangeType, (int x, int y)[] arrangement, Vector3 origin, Vector3[] originOffset, Vector3 contentsOffset, float[] scale, (float x, float y)[] contentAreaMultiplier)
        {
            int num1 = index % arrangement[arrangeType].x;
            int num2 = index / arrangement[arrangeType].x;
            return origin + originOffset[arrangeType] + new Vector3(contentsOffset.x * scale[arrangeType] * contentAreaMultiplier[arrangeType].x * num1, contentsOffset.y * scale[arrangeType] * contentAreaMultiplier[arrangeType].y * num2, (float)(-(double)num2 * 0.0099999997764825821));
        }

        public static int GetDisplayType(int players)
        {
            if (players <= 15)
                return 0;
            return players > 18 ? 2 : 1;
        }

        public static void TurnToImpostor(this PlayerControl player)
        {
            player.roleAssigned = false;
            RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
            player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            player.roleAssigned = true;
            player.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));

            foreach (PlayerControl otherPlayer in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (otherPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                    player.cosmetics.nameText.color = Palette.ImpostorRed;
        }

        public static GameObject CreateObject(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        {
            var obj = new GameObject(objName);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            if (layer.HasValue) obj.layer = layer.Value;
            else if (parent != null) obj.layer = parent.gameObject.layer;
            return obj;
        }

        public static List<DeadBody> DeadBodies(float distance, List<byte> ignorelist = null)
        {
            PlayerControl LocalPlayer = PlayerControl.LocalPlayer;
            Vector2 truePosition = LocalPlayer.GetTruePosition();
            var colliders = Physics2D.OverlapCircleAll(truePosition, distance, Constants.PlayersOnlyMask);
            List<DeadBody> list = new();
            if (LocalPlayer.Data.IsDead) return list;

            foreach (Collider2D collider in colliders)
            {
                if (collider.tag != "DeadBody")
                    continue;

                DeadBody component = collider.GetComponent<DeadBody>();
                if (ignorelist != null && ignorelist.Any(x => x == component.ParentId))
                {
                    continue;
                }
                if (PhysicsHelpers.AnythingBetween(truePosition, component.TruePosition, Constants.ShipAndObjectsMask, false))
                {
                    if (Vector2.Distance(truePosition, component.TruePosition) <= 0.25f)
                    {
                        list.Add(component);
                    }
                    continue;
                }

                list.Add(component);
            }
            return list;
        }

        public static void grenadierFlash(Color color, float duration = 10f, float alpha = 1f)
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;

            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            DestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.active = true;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>(p =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;
                var fadeFraction = 0.5f / duration;

                if (MeetingHud.Instance)
                {
                    renderer.enabled = false;
                    if (PlayerControl.LocalPlayer == Grenadier.grenadier && Grenadier.flashedPlayers.Count > 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                        writer.Write(true);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        Grenadier.flashedPlayers.Clear();
                    }
                    return;
                }

                if (p < fadeFraction)
                {
                    var fadeInProgress = p / fadeFraction;
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(fadeInProgress * alpha));
                }
                else if (p > 1 - fadeFraction)
                {
                    var fadeOutProgress = (p - (1 - fadeFraction)) / fadeFraction;
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - fadeOutProgress) * alpha));

                    if (PlayerControl.LocalPlayer == Grenadier.grenadier && Grenadier.flashedPlayers.Count > 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GrenadierFlash, SendOption.Reliable, -1);
                        writer.Write(true);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        Grenadier.flashedPlayers.Clear();
                    }
                }
                else
                {
                    if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, alpha);
                }

                if (p == 1f && renderer != null) renderer.enabled = false;
            })));

            if (Grenadier.flashTimer > 0.5f)
            {
                try
                {
                    if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && MapBehaviour.Instance.infectedOverlay.sabSystem.Timer < 0.5f)
                        MapBehaviour.Instance.infectedOverlay.sabSystem.Timer = 0.5f;
                }
                catch { }
            }
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> getClosestPlayers(Vector2 truePosition, float radius, bool includeDead)
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
                    float magnitude = vector2.magnitude;
                    if (magnitude <= lightRadius)
                    {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }

        public static bool isFlashedByGrenadier(this PlayerControl player)
        {
            return Grenadier.grenadier != null && Grenadier.flashTimer > 0f && Grenadier.flashedPlayers.Contains(player);
        }

        public static void rpcCampaignPlayer(this PlayerControl source, PlayerControl target)
        {
            if (Politician.politician == null || Politician.politician.Data.IsDead) return;
            byte targetId;
            if (Politician.campaignedPlayers.Contains(source) && !Politician.campaignedPlayers.Contains(target)) targetId = target.PlayerId;
            else if (!Politician.campaignedPlayers.Contains(source) && Politician.campaignedPlayers.Contains(target)) targetId = source.PlayerId;
            else return;
            new WaitForSeconds(1f);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(source.NetId, (byte)CustomRPC.PoliticianCampaign, SendOption.Reliable, -1);
            writer.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.politicianCampaign(targetId);
        }

        public static void handlePoisonerKillOnBodyReport()
        {
            // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
            Helpers.checkMurderAttemptAndKill(Poisoner.poisoner, Poisoner.poisoned, true, false);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
            writer.Write(byte.MaxValue);
            writer.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue);
        }

        public static void rpcInfectPlayer(this PlayerControl source, PlayerControl target)
        {
            if (Plaguebearer.plaguebearer == null || Plaguebearer.plaguebearer.Data.IsDead) return;
            byte targetId;
            if (Plaguebearer.infectedPlayers.Contains(source) && !Plaguebearer.infectedPlayers.Contains(target)) targetId = target.PlayerId;
            else if (!Plaguebearer.infectedPlayers.Contains(source) && Plaguebearer.infectedPlayers.Contains(target)) targetId = source.PlayerId;
            else return;
            new WaitForSeconds(1f);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(source.NetId, (byte)CustomRPC.PlaguebearerInfect, SendOption.Reliable, -1);
            writer.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.plaguebearerInfect(targetId);
        }

        public static IEnumerator Hack(PlayerControl hackPlayer)
        {
            var lockImg = new GameObject[6];
            if (Glitch.hackedPlayers.ContainsKey(hackPlayer))
            {
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

            if (PlayerControl.LocalPlayer == hackPlayer)
            {
                useButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled;
                petButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled;
                reportButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled;
                killButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled;
                saboButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled;
                ventButtonEnabled = FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled;
            }

            while (true)
            {
                if (PlayerControl.LocalPlayer == hackPlayer)
                {
                    // Kill Button
                    if (FastDestroyableSingleton<HudManager>.Instance.KillButton != null && killButtonEnabled)
                    {
                        if (lockImg[0] == null)
                        {
                            lockImg[0] = new GameObject();
                            lockImg[0].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[0] != null)
                    {
                        lockImg[0].layer = 5;
                        lockImg[0].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.position.y, -50f);
                    }

                    // Use Button
                    if (FastDestroyableSingleton<HudManager>.Instance.UseButton != null && useButtonEnabled)
                    {
                        if (lockImg[1] == null)
                        {
                            lockImg[1] = new GameObject();
                            lockImg[1].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[1] != null)
                    {
                        lockImg[1].layer = 5;
                        lockImg[1].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.position.y, -50f);
                    }

                    // Pet Button
                    if (FastDestroyableSingleton<HudManager>.Instance.PetButton != null && petButtonEnabled)
                    {
                        if (lockImg[2] == null)
                        {
                            lockImg[2] = new GameObject();
                            lockImg[2].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.PetButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[2] != null)
                    {
                        lockImg[2].layer = 5;
                        lockImg[2].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.PetButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.PetButton.transform.position.y, -50f);
                    }

                    // Report Button
                    if (FastDestroyableSingleton<HudManager>.Instance.ReportButton != null && reportButtonEnabled)
                    {
                        if (lockImg[3] == null)
                        {
                            lockImg[3] = new GameObject();
                            lockImg[3].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[3] != null)
                    {
                        lockImg[3].layer = 5;
                        lockImg[3].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.ReportButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.ReportButton.transform.position.y, -50f);
                    }

                    // Sabotage Button
                    if (FastDestroyableSingleton<HudManager>.Instance.SabotageButton != null && saboButtonEnabled)
                    {
                        if (lockImg[4] == null)
                        {
                            lockImg[4] = new GameObject();
                            lockImg[4].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                        }
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.color = Palette.DisabledClear;
                        FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.material.SetFloat("_Desat", 1f);
                    }
                    if (lockImg[4] != null)
                    {
                        lockImg[4].layer = 5;
                        lockImg[4].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.SabotageButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.SabotageButton.transform.position.y, -50f);
                    }

                    // Vent Button
                    if (PlayerControl.LocalPlayer.roleCanUseVents())
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton != null && ventButtonEnabled)
                        {
                            if (lockImg[5] == null)
                            {
                                lockImg[5] = new GameObject();
                                lockImg[5].AddComponent<SpriteRenderer>().sprite = CustomButton.getLockSprite();
                            }
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.color = Palette.DisabledClear;
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.material.SetFloat("_Desat", 1f);
                        }
                        if (lockImg[5] != null)
                        {
                            lockImg[5].layer = 5;
                            lockImg[5].transform.position = new Vector3(FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.transform.position.x, FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.transform.position.y, -50f);
                        }
                    }

                    // Custom Buttons
                    if (PlayerControl.LocalPlayer)
                    {
                        CustomButton.buttons.ForEach(x => x.Hack());
                    }

                    if (Minigame.Instance)
                    {
                        Minigame.Instance.Close();
                        Minigame.Instance.ForceClose();
                    }

                    if (MapBehaviour.Instance)
                    {
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

            if (PlayerControl.LocalPlayer == hackPlayer)
            {
                FastDestroyableSingleton<HudManager>.Instance.UseButton.enabled = useButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.PetButton.enabled = petButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = reportButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = killButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.enabled = saboButtonEnabled;
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.enabled = ventButtonEnabled;
                if (PlayerControl.LocalPlayer)
                {
                    CustomButton.buttons.ForEach(x => x.UnHack());
                }
                PlayerControl.LocalPlayer.MaxReportDistance = savedReportDistance;
            }
            Glitch.hackedPlayers.Remove(hackPlayer);
            Glitch.hackedPlayer = null;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            for (var i = list.Count - 1; i > 0; --i)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static bool canGuess(this PlayerControl player, RoleInfo roleInfo)
        {
            RoleId roleId = roleInfo.roleId;
            FactionId factionId = roleInfo.factionId;
            bool guesserGm = HandleGuesser.isGuesserGm;

            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            if (guesserGm)
            {
                if (roleId == RoleId.Mayor) return false;
                else if (roleId == RoleId.Pestilence) return false;
                else if (roleId == RoleId.Crewmate)
                    return
                        !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessCrewmateRoles.getBool()
                        || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessCrewmateRoles.getBool()
                        || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessCrewmateRoles.getBool()
                        || player == Doomsayer.doomsayer
                        || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessCrewmateRoles.getBool()
                        || player.Data.Role.IsImpostor;
                else if (roleId == RoleId.Impostor)
                    return
                        !player.isEvil()
                        || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessImpostorRoles.getBool()
                        || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessImpostorRoles.getBool()
                        || player == Doomsayer.doomsayer && Doomsayer.canGuessImpostor
                        || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessImpostorRoles.getBool()
                        || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessImpostorRoles.getBool();
                else if (factionId == FactionId.Crewmate)
                {
                    bool additionalTemp = roleData.crewSettings.ContainsKey((byte)roleId) && roleData.crewSettings[(byte)roleId] > 0;
                    return
                        additionalTemp && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessCrewmateRoles.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessCrewmateRoles.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessCrewmateRoles.getBool()
                            || player == Doomsayer.doomsayer
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessCrewmateRoles.getBool()
                            || player.Data.Role.IsImpostor
                        );
                }
                else if (factionId == FactionId.NeutralBenign)
                {
                    bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                    if (roleId == RoleId.Amnesiac)
                    {
                        bool additionalTempA = CustomOptionHolder.amnesiacSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Amnesiac || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Amnesiac || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Amnesiac;
                        return additionalTempA && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralBenign
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralBenign.getBool()
                        );
                    }
                    if (roleId == RoleId.Pursuer)
                    {
                        bool additionalTempP = CustomOptionHolder.pursuerSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Pursuer || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Pursuer || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Pursuer;
                        return additionalTempP && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralBenign
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralBenign.getBool()
                        );
                    }
                    if (roleId == RoleId.Survivor)
                    {
                        bool additionalTempS = CustomOptionHolder.survivorSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Survivor || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Survivor || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Survivor;
                        return additionalTempS && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralBenign
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralBenign.getBool()
                        );
                    }
                    if (roleId == RoleId.Thief)
                    {
                        bool additionalTempT = CustomOptionHolder.thiefSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Thief || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Thief || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Thief;
                        return additionalTempT && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralBenign
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralBenign.getBool()
                        );
                    }
                    return
                        additionalTemp && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralBenign.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralBenign.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralBenign
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralBenign.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralBenign.getBool()
                        );
                }
                else if (factionId == FactionId.NeutralEvil)
                {
                    bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                    return
                        additionalTemp && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralEvil.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralEvil.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralEvil.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralEvil
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralEvil.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralEvil.getBool()
                        );
                }
                else if (factionId == FactionId.NeutralKilling)
                {
                    if (roleId == RoleId.Vampire)
                    {
                        bool additionalTempV = CustomOptionHolder.draculaSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaCanCreateVampire.getBool();
                        return additionalTempV && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralKilling.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralKilling.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralKilling.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralKilling
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralKilling.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralKilling.getBool()
                        );
                    }
                    bool additionalTemp = roleData.neutralKillingSettings.ContainsKey((byte)roleId) && roleData.neutralKillingSettings[(byte)roleId] > 0;
                    return
                        additionalTemp && (
                            !player.isEvil() && CustomOptionHolder.guesserGamemodeCrewmateGuesserCanGuessNeutralKilling.getBool()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessNeutralKilling.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessNeutralKilling.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessNeutralKilling
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessNeutralKilling.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessNeutralKilling.getBool()
                        );
                }
                else if (factionId == FactionId.Impostor)
                {
                    bool additionalTemp = roleData.impSettings.ContainsKey((byte)roleId) && roleData.impSettings[(byte)roleId] > 0;
                    return
                        additionalTemp && (
                            !player.isEvil()
                            || player.isNeutralBenign() && CustomOptionHolder.guesserGamemodeNeutralBenignGuesserCanGuessImpostorRoles.getBool()
                            || player.isNeutralEvil() && player != Doomsayer.doomsayer && CustomOptionHolder.guesserGamemodeNeutralEvilGuesserCanGuessImpostorRoles.getBool()
                            || player == Doomsayer.doomsayer && Doomsayer.canGuessImpostor
                            || player.isNeutralKilling() && CustomOptionHolder.guesserGamemodeNeutralKillingGuesserCanGuessImpostorRoles.getBool()
                            || player.Data.Role.IsImpostor && CustomOptionHolder.guesserGamemodeImpostorGuesserCanGuessImpostorRoles.getBool()
                        );
                }
            }
            else
            {
                if (player == Vigilante.vigilante)
                {
                    if (roleId == RoleId.Mayor) return false;
                    else if (roleId == RoleId.Pestilence) return false;
                    else if (roleId == RoleId.Crewmate)
                        return Vigilante.canGuessCrewmateRoles;
                    else if (roleId == RoleId.Impostor)
                        return true;
                    else if (factionId == FactionId.Crewmate)
                    {
                        bool additionalTemp = roleData.crewSettings.ContainsKey((byte)roleId) && roleData.crewSettings[(byte)roleId] > 0;
                        return additionalTemp && Vigilante.canGuessCrewmateRoles;
                    }
                    else if (factionId == FactionId.NeutralBenign)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        if (roleId == RoleId.Amnesiac)
                        {
                            bool additionalTempA = CustomOptionHolder.amnesiacSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Amnesiac || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Amnesiac || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Amnesiac;
                            return additionalTempA && Vigilante.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Pursuer)
                        {
                            bool additionalTempP = CustomOptionHolder.pursuerSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Pursuer || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Pursuer || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Pursuer;
                            return additionalTempP && Vigilante.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Survivor)
                        {
                            bool additionalTempS = CustomOptionHolder.survivorSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Survivor || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Survivor || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Survivor;
                            return additionalTempS && Vigilante.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Thief)
                        {
                            bool additionalTempT = CustomOptionHolder.thiefSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Thief || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Thief || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Thief;
                            return additionalTempT && Vigilante.canGuessNeutralBenign;
                        }
                        return additionalTemp && Vigilante.canGuessNeutralBenign;
                    }
                    else if (factionId == FactionId.NeutralEvil)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        return additionalTemp && Vigilante.canGuessNeutralEvil;
                    }
                    else if (factionId == FactionId.NeutralKilling)
                    {
                        if (roleId == RoleId.Vampire)
                        {
                            bool additionalTempV = CustomOptionHolder.draculaSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaCanCreateVampire.getBool();
                            return additionalTempV && Vigilante.canGuessNeutralKilling;
                        }
                        bool additionalTemp = roleData.neutralKillingSettings.ContainsKey((byte)roleId) && roleData.neutralKillingSettings[(byte)roleId] > 0;
                        return additionalTemp && Vigilante.canGuessNeutralKilling;
                    }
                    else if (factionId == FactionId.Impostor)
                    {
                        bool additionalTemp = roleData.impSettings.ContainsKey((byte)roleId) && roleData.impSettings[(byte)roleId] > 0;
                        return additionalTemp;
                    }
                }
                else if (player == Assassin.assassin)
                {
                    if (roleId == RoleId.Mayor) return false;
                    else if (roleId == RoleId.Pestilence) return false;
                    else if (roleId == RoleId.Crewmate)
                        return true;
                    else if (roleId == RoleId.Impostor)
                        return Assassin.canGuessImpostorRoles;
                    else if (factionId == FactionId.Crewmate)
                    {
                        bool additionalTemp = roleData.crewSettings.ContainsKey((byte)roleId) && roleData.crewSettings[(byte)roleId] > 0;
                        return additionalTemp;
                    }
                    else if (factionId == FactionId.NeutralBenign)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        if (roleId == RoleId.Amnesiac)
                        {
                            bool additionalTempA = CustomOptionHolder.amnesiacSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Amnesiac || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Amnesiac || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Amnesiac;
                            return additionalTempA && Assassin.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Pursuer)
                        {
                            bool additionalTempP = CustomOptionHolder.pursuerSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Pursuer || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Pursuer || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Pursuer;
                            return additionalTempP && Assassin.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Survivor)
                        {
                            bool additionalTempS = CustomOptionHolder.survivorSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Survivor || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Survivor || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Survivor;
                            return additionalTempS && Assassin.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Thief)
                        {
                            bool additionalTempT = CustomOptionHolder.thiefSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Thief || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Thief || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Thief;
                            return additionalTempT && Assassin.canGuessNeutralBenign;
                        }
                        return additionalTemp && Assassin.canGuessNeutralBenign;
                    }
                    else if (factionId == FactionId.NeutralEvil)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        return additionalTemp && Assassin.canGuessNeutralEvil;
                    }
                    else if (factionId == FactionId.NeutralKilling)
                    {
                        if (roleId == RoleId.Vampire)
                        {
                            bool additionalTempV = CustomOptionHolder.draculaSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaCanCreateVampire.getBool();
                            return additionalTempV && Assassin.canGuessNeutralKilling;
                        }
                        bool additionalTemp = roleData.neutralKillingSettings.ContainsKey((byte)roleId) && roleData.neutralKillingSettings[(byte)roleId] > 0;
                        return additionalTemp && Assassin.canGuessNeutralKilling;
                    }
                    else if (factionId == FactionId.Impostor)
                    {
                        bool additionalTemp = roleData.impSettings.ContainsKey((byte)roleId) && roleData.impSettings[(byte)roleId] > 0;
                        return additionalTemp && Assassin.canGuessImpostorRoles;
                    }
                }
                else if (player == Doomsayer.doomsayer)
                {
                    if (roleId == RoleId.Mayor) return false;
                    else if (roleId == RoleId.Pestilence) return false;
                    else if (roleId == RoleId.Crewmate)
                        return true;
                    else if (roleId == RoleId.Impostor)
                        return Doomsayer.canGuessImpostor;
                    else if (factionId == FactionId.Crewmate)
                    {
                        bool additionalTemp = roleData.crewSettings.ContainsKey((byte)roleId) && roleData.crewSettings[(byte)roleId] > 0;
                        return additionalTemp;
                    }
                    else if (factionId == FactionId.NeutralBenign)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        if (roleId == RoleId.Amnesiac)
                        {
                            bool additionalTempA = CustomOptionHolder.amnesiacSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Amnesiac || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Amnesiac || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Amnesiac;
                            return additionalTempA && Doomsayer.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Pursuer)
                        {
                            bool additionalTempP = CustomOptionHolder.pursuerSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Pursuer || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Pursuer || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Pursuer;
                            return additionalTempP && Doomsayer.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Survivor)
                        {
                            bool additionalTempS = CustomOptionHolder.survivorSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Survivor || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Survivor || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Survivor;
                            return additionalTempS && Doomsayer.canGuessNeutralBenign;
                        }
                        if (roleId == RoleId.Thief)
                        {
                            bool additionalTempT = CustomOptionHolder.thiefSpawnRate.getSelection() > 0 || Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Thief || Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Thief || GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Thief;
                            return additionalTempT && Doomsayer.canGuessNeutralBenign;
                        }
                        return additionalTemp && Doomsayer.canGuessNeutralBenign;
                    }
                    else if (factionId == FactionId.NeutralEvil)
                    {
                        bool additionalTemp = roleData.neutralSettings.ContainsKey((byte)roleId) && roleData.neutralSettings[(byte)roleId] > 0;
                        return additionalTemp && Doomsayer.canGuessNeutralEvil;
                    }
                    else if (factionId == FactionId.NeutralKilling)
                    {
                        if (roleId == RoleId.Vampire)
                        {
                            bool additionalTempV = CustomOptionHolder.draculaSpawnRate.getSelection() > 0 && CustomOptionHolder.draculaCanCreateVampire.getBool();
                            return additionalTempV && Doomsayer.canGuessNeutralKilling;
                        }
                        bool additionalTemp = roleData.neutralKillingSettings.ContainsKey((byte)roleId) && roleData.neutralKillingSettings[(byte)roleId] > 0;
                        return additionalTemp && Doomsayer.canGuessNeutralKilling;
                    }
                    else if (factionId == FactionId.Impostor)
                    {
                        bool additionalTemp = roleData.impSettings.ContainsKey((byte)roleId) && roleData.impSettings[(byte)roleId] > 0;
                        return additionalTemp && Doomsayer.canGuessImpostor;
                    }
                }
            }
            return false;
        }

        public static bool cantGuessSnitch(this PlayerControl player)
        {
            if (Assassin.assassin != null && Assassin.assassin == player && Assassin.cantGuessSnitch) return true;
            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == player && Doomsayer.cantGuessSnitch) return true;
            if (GuesserGM.isGuesser(player.PlayerId))
            {
                if (player.isEvil() && CustomOptionHolder.guesserGamemodeCantGuessSnitchIfTaksDone.getBool()) return true;
            }
            return false;
        }

        public static bool hasMultipleShotsPerMeeting(this PlayerControl player)
        {
            if (Assassin.assassin != null && Assassin.assassin == player && Assassin.multipleKill) return true;
            if (Vigilante.vigilante != null && Vigilante.vigilante == player && Vigilante.multipleKill) return true;
            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == player && Doomsayer.hasMultipleShotsPerMeeting) return true;
            if (GuesserGM.isGuesser(player.PlayerId) && CustomOptionHolder.guesserGamemodeHasMultipleShotsPerMeeting.getBool()) return true;
            return false;
        }
        
        public static bool killsThroughShield(this PlayerControl player)
        {
            if (Assassin.assassin != null && Assassin.assassin == player && Assassin.canKillsThroughShield) return true;
            if (Vigilante.vigilante != null && Vigilante.vigilante == player && Vigilante.canKillsThroughShield) return true;
            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == player && Doomsayer.canKillsThroughShield) return true;
            if (GuesserGM.isGuesser(player.PlayerId) && CustomOptionHolder.guesserGamemodeKillsThroughShield.getBool()) return true;
            return false;
        }

        public static bool IsSheriff(this PlayerControl player, out Sheriff sheriff) => Sheriff.IsSheriff(player.PlayerId, out sheriff);
        public static bool IsSheriff(this PlayerControl player) => Sheriff.IsSheriff(player.PlayerId, out _);

        public static bool IsJester(this PlayerControl player, out Jester jester) => Jester.IsJester(player.PlayerId, out jester);
        public static bool IsJester(this PlayerControl player) => Jester.IsJester(player.PlayerId, out _);

        public static bool IsSurvivor(this PlayerControl player, out Survivor survivor) => Survivor.IsSurvivor(player.PlayerId, out survivor);
        public static bool IsSurvivor(this PlayerControl player) => Survivor.IsSurvivor(player.PlayerId, out _);

        public static bool IsVeteran(this PlayerControl player, out Veteran veteran) => Veteran.IsVeteran(player.PlayerId, out veteran);
        public static bool IsVeteran(this PlayerControl player) => Veteran.IsVeteran(player.PlayerId, out _);

        public static bool IsPursuer(this PlayerControl player, out Pursuer pursuer) => Pursuer.IsPursuer(player.PlayerId, out pursuer);
        public static bool IsPursuer(this PlayerControl player) => Pursuer.IsPursuer(player.PlayerId, out _);

        public static bool IsAmnesiac(this PlayerControl player, out Amnesiac amnesiac) => Amnesiac.IsAmnesiac(player.PlayerId, out amnesiac);
        public static bool IsAmnesiac(this PlayerControl player) => Amnesiac.IsAmnesiac(player.PlayerId, out _);

        public static bool IsThief(this PlayerControl player, out Thief thief) => Thief.IsThief(player.PlayerId, out thief);
        public static bool IsThief(this PlayerControl player) => Thief.IsThief(player.PlayerId, out _);
    }
}