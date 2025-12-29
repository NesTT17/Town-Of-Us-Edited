using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs
{
    public enum MurderAttemptResult { PerformKill, SuppressKill, BlankKill, ReverseKill }
    public enum CustomGamemodes { Classic, Guesser, AllAny }
    public enum SabatageTypes { Comms, O2, Reactor, Lights, MushroomMixUp, None }

    public static class Helpers
    {
        public static bool isMovedVentOnPolus = false;
        public static string previousEndGameSummary = "";
        public static Dictionary<string, Sprite> CachedSprites = new();
        public static bool ShowKillAnimation
        {
            get
            {
                return MeetingHud.Instance?.state is not
                    MeetingHud.VoteStates.Animating and not MeetingHud.VoteStates.Results
                    and not MeetingHud.VoteStates.Proceeding;
            }
        }
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
                System.Console.WriteLine("Error loading texture from resources: " + path);
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

        public static bool isD(byte playerId)
        {
            return playerId % 2 == 0;
        }

        public static bool isLighterColor(PlayerControl target)
        {
            return isD(target.PlayerId);
        }

        public static bool hasFakeTasks(this PlayerControl player)
        {
            return player.isAnyNeutral();
        }

        public static bool shouldShowGhostInfo()
        {
            return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeInformation || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;
        }

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
            player.MurderPlayer(target, MurderResultFlags.Succeeded);
        }

        public static void RpcRepairSystem(this ShipStatus shipStatus, SystemTypes systemType, byte amount)
        {
            shipStatus.RpcUpdateSystem(systemType, amount);
        }

        public static bool isSkeld()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
        }

        public static bool isMira()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
        }

        public static bool isPolus()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
        }

        public static bool isAirship()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
        }

        public static bool isFungle()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value, float alpha = 0.25f)
        {
            alpha = value ? alpha : 1f;
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
            if (Camouflager.camouflageTimer > 0f || getActiveSabo() == SabatageTypes.MushroomMixUp) return true;

            if (Swooper.players.Any(x => x.player.PlayerId == target.PlayerId && x.isInvisble)) return true;
            else if (Venerer.players.Any(x => x.player.PlayerId == target.PlayerId && x.morphTimer > 0f)) return true;
            else if (!hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target.isRole(RoleId.Agent) || Vampire.players.Any(x => x.player == target && x.wasTeamRed) || Dracula.players.Any(x => x.player == target && x.wasTeamRed))) return false;
            else if (Dracula.players.Any(x => x.player == source && (target == x.fakeVampire || (Dracula.getVampire(source) != null && Dracula.getVampire(source).player == target))) || Vampire.players.Any(x => x.player == source && x.dracula.player == target)) return false;
            else if (source.getPartner() == target) return false;
            return true;
        }

        public static void setDefaultLook(this PlayerControl target)
        {
            if (getActiveSabo() == SabatageTypes.MushroomMixUp)
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
            try
            {
                nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId);
            }
            catch { return; }

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

            Chameleon.local.update();
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
            if (player.isRole(RoleId.Engineer)) roleCouldUse = true;
            else if (player.isRole(RoleId.Juggernaut) && Juggernaut.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Pestilence) && Pestilence.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Werewolf) && Werewolf.canVent && Werewolf.players.Any(x => x.player == player && x.isRampageActive)) roleCouldUse = true;
            else if (player.isRole(RoleId.Dracula) && Dracula.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Vampire) && Vampire.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Scavenger) && Scavenger.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Glitch) && Glitch.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Arsonist) && Arsonist.canVent) roleCouldUse = true;
            else if (player.isRole(RoleId.Agent) && Agent.canEnterVents) roleCouldUse = true;
            else if (player.isRole(RoleId.Thief) && Thief.canUseVents) roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                roleCouldUse = true;
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
            if (!ignoreBlank && Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setBlanked(killer.PlayerId, 0);

                return MurderAttemptResult.BlankKill;
            }

            // Handle first kill attempt
            if (shieldFirstKill && firstKillPlayer == target) return MurderAttemptResult.SuppressKill;

            // Block impostor shielded kill
            if (Medic.isShielded(target))
            {
                foreach (var medic in Medic.GetMedic(target))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                    writer.Write(medic.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                }
                return MurderAttemptResult.SuppressKill;
            }

            // Block impostor safeguard kill
            if (target.isRole(RoleId.Survivor) && Survivor.players.Any(x => x.player == target && x.isSafeguardActive)) return MurderAttemptResult.BlankKill;

            // Block impostor protect kill
            if (GuardianAngel.isProtected(target)) return MurderAttemptResult.BlankKill;

            // Block impostor merc shielded kill
            if (Mercenary.isShielded(target))
            {
                foreach (var mercenary in Mercenary.GetMercenary(target))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                    writer.Write(mercenary.shielded.PlayerId);
                    writer.Write(mercenary.player.PlayerId);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                }
                return MurderAttemptResult.BlankKill;
            }

            // Handle veteran alert
            if (Veteran.players.Any(x => x.player == target && x.isAlertActive))
            {
                if (shieldFirstKill && firstKillPlayer == killer) return MurderAttemptResult.SuppressKill;
                if (killer.isRole(RoleId.Pestilence)) return MurderAttemptResult.SuppressKill;
                if (Medic.isShielded(killer))
                {
                    foreach (var medic in Medic.GetMedic(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(medic.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (Mercenary.isShielded(killer))
                {
                    foreach (var mercenary in Mercenary.GetMercenary(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(mercenary.shielded.PlayerId);
                        writer.Write(mercenary.player.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (GuardianAngel.isProtected(killer)) return MurderAttemptResult.SuppressKill;
                return MurderAttemptResult.ReverseKill;
            }

            // Handle mayor bodyguard
            if (Mayor.players.Any(x => x.player == target && x.isBodyguardActive))
            {
                if (shieldFirstKill && firstKillPlayer == killer) return MurderAttemptResult.SuppressKill;
                if (killer.isRole(RoleId.Pestilence)) return MurderAttemptResult.SuppressKill;
                if (Medic.isShielded(killer))
                {
                    foreach (var medic in Medic.GetMedic(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(medic.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (Mercenary.isShielded(killer))
                {
                    foreach (var mercenary in Mercenary.GetMercenary(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(mercenary.shielded.PlayerId);
                        writer.Write(mercenary.player.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (GuardianAngel.isProtected(killer)) return MurderAttemptResult.SuppressKill;
                return MurderAttemptResult.ReverseKill;
            }

            // Handle pestilence revert kill
            if (Pestilence.players.Any(x => x.player == target))
            {
                if (shieldFirstKill && firstKillPlayer == killer) return MurderAttemptResult.SuppressKill;
                if (Medic.isShielded(killer))
                {
                    foreach (var medic in Medic.GetMedic(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(medic.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (Mercenary.isShielded(killer))
                {
                    foreach (var mercenary in Mercenary.GetMercenary(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(mercenary.shielded.PlayerId);
                        writer.Write(mercenary.player.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (GuardianAngel.isProtected(killer)) return MurderAttemptResult.SuppressKill;
                return MurderAttemptResult.ReverseKill;
            }

            // Handle mercenary armor
            if (Mercenary.players.Any(x => x.player == target && x.isArmorActive))
            {
                if (shieldFirstKill && firstKillPlayer == killer) return MurderAttemptResult.SuppressKill;
                if (killer.isRole(RoleId.Pestilence)) return MurderAttemptResult.SuppressKill;
                if (Medic.isShielded(killer))
                {
                    foreach (var medic in Medic.GetMedic(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(medic.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (Mercenary.isShielded(killer))
                {
                    foreach (var mercenary in Mercenary.GetMercenary(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(mercenary.shielded.PlayerId);
                        writer.Write(mercenary.player.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (GuardianAngel.isProtected(killer)) return MurderAttemptResult.SuppressKill;
                return MurderAttemptResult.ReverseKill;
            }

            // Handle vampire hunter reverse kill
            if (VampireHunter.players.Any(x => x.player == target) && (Dracula.players.Any(x => x.player == killer) || Vampire.players.Any(x => x.player == killer)))
            {
                if (shieldFirstKill && firstKillPlayer == killer) return MurderAttemptResult.SuppressKill;
                if (Medic.isShielded(killer))
                {
                    foreach (var medic in Medic.GetMedic(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(medic.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (Mercenary.isShielded(killer))
                {
                    foreach (var mercenary in Mercenary.GetMercenary(killer))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        writer.Write(mercenary.shielded.PlayerId);
                        writer.Write(mercenary.player.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, false);
                    }
                    return MurderAttemptResult.SuppressKill;
                }
                if (GuardianAngel.isProtected(killer)) return MurderAttemptResult.SuppressKill;
                return MurderAttemptResult.ReverseKill;
            }

            // Thief if hit crew only kill if setting says so, but also kill the thief.
            if (Thief.isFailedThiefKill(target, killer, targetRole))
            {
                var thief = Thief.getRole(killer);
                thief.suicideFlag = true;
                return MurderAttemptResult.SuppressKill;
            }

            if (TransportationToolPatches.isUsingTransportation(target))
                return MurderAttemptResult.SuppressKill;
            return MurderAttemptResult.PerformKill;
        }

        public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? Byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
            politicianRpcCampaign(killer, target);
            plaguebearerRpcInfect(killer, target);
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
            if (murder == MurderAttemptResult.ReverseKill)
            {
                checkMurderAttemptAndKill(target, killer, isMeetingStart);
            }
            return murder;
        }

        public static bool checkSuspendAction(PlayerControl player, PlayerControl target)
        {
            if (player == null || target == null) return false;
            if (Veteran.players.Any(x => x.player == target && x.isAlertActive))
            {
                checkMurderAttemptAndKill(target, player);
                return true;
            }
            if (Mayor.players.Any(x => x.player == target && x.isBodyguardActive))
            {
                checkMurderAttemptAndKill(target, player);
                return true;
            }
            if (Pestilence.players.Any(x => x.player == target))
            {
                checkMurderAttemptAndKill(target, player);
                return true;
            }
            return false;
        }

        public static bool isBenignNeutral(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.BenignNeutral;
            return false;
        }

        public static bool isEvilNeutral(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.EvilNeutral;
            return false;
        }

        public static bool isKillingNeutral(this PlayerControl player)
        {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.factionId == FactionId.KillingNeutral;
            return false;
        }

        public static bool isAnyNeutral(this PlayerControl player)
        {
            return player.isBenignNeutral() || player.isEvilNeutral() || player.isKillingNeutral();
        }

        public static bool isKiller(this PlayerControl player)
        {
            return player.Data.Role.IsImpostor || player.isKillingNeutral();
        }

        public static bool isEvil(this PlayerControl player)
        {
            return player.Data.Role.IsImpostor || player.isAnyNeutral();
        }

        public static bool canBeErased(this PlayerControl player)
        {
            return !player.isKillingNeutral();
        }

        public static bool MushroomSabotageActive()
        {
            return PlayerControl.LocalPlayer.myTasks.ToArray().Any((x) => x.TaskType == TaskTypes.MushroomMixupSabotage);
        }

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
                || (player.Object.isRole(RoleId.Jester) && Jester.hasImpostorVision)
                || (player.Object.isRole(RoleId.Juggernaut) && Juggernaut.hasImpostorVision)
                || (player.Object.isRole(RoleId.Pestilence) && Pestilence.hasImpostorVision)
                || (player.Object.isRole(RoleId.Dracula) && Dracula.hasImpostorVision)
                || (player.Object.isRole(RoleId.Vampire) && Vampire.hasImpostorVision)
                || (player.Object.isRole(RoleId.Glitch) && Glitch.hasImpostorVision)
                || (player.Object.isRole(RoleId.Arsonist) && Arsonist.hasImpostorVision)
                || (player.Object.isRole(RoleId.Agent) && Agent.hasImpostorVision)
                || (player.Object.isRole(RoleId.Thief) && Thief.hasImpostorVision);
        }

        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = LegacyGameOptions.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;

            color = color.SetAlpha(Chameleon.local.visibility(target.PlayerId));

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        public static void turnToImpostor(PlayerControl player)
        {
            player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
            player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

            foreach (var player2 in PlayerControl.AllPlayerControls)
            {
                if (player2.Data.Role.IsImpostor && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    player.cosmetics.nameText.color = Palette.ImpostorRed;
                }
            }
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
                else if (task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.StopCharles || task.TaskType == TaskTypes.ResetSeismic)
                {
                    return SabatageTypes.Reactor;
                }
                else if (task.TaskType == TaskTypes.FixComms)
                {
                    return SabatageTypes.Comms;
                }
                else if (task.TaskType == TaskTypes.MushroomMixupSabotage)
                {
                    return SabatageTypes.MushroomMixUp;
                }
            }
            return SabatageTypes.None;
        }

        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }

        public static bool isSabotageTask(PlayerTask task)
        {
            var tt = task.TaskType;
            return tt is (TaskTypes.FixComms or TaskTypes.ResetSeismic or TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage or TaskTypes.ResetReactor);
        }

        public static bool TryAdd<T>(this List<T> list, T item)
        {
            if (list == null || item == null || list.Contains(item)) return false;
            try
            {
                list.Add(item);
                return true;
            }
            catch
            {
                return false;
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

        public static void politicianRpcCampaign(PlayerControl source, PlayerControl target)
        {
            new WaitForSeconds(1f);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoliticianCampaign, SendOption.Reliable, -1);
            writer.Write(source.PlayerId);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.politicianCampaign(source.PlayerId, target.PlayerId);
        }

        public static void plaguebearerRpcInfect(PlayerControl source, PlayerControl target)
        {
            new WaitForSeconds(1f);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaguebearerInfect, SendOption.Reliable, -1);
            writer.Write(source.PlayerId);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.plaguebearerInfect(source.PlayerId, target.PlayerId);
        }

        public static void handleVampireKillOnBodyReport()
        {
            foreach (var poisoner in Poisoner.players)
            {
                MurderAttemptResult murder = checkMuderAttempt(poisoner.player, poisoner.poisoned, true, false, true);
                switch (murder)
                {
                    case MurderAttemptResult.PerformKill:
                        MurderPlayer(poisoner.player, poisoner.poisoned, false);
                        break;
                    case MurderAttemptResult.SuppressKill:
                        break;
                    case MurderAttemptResult.BlankKill:
                        break;
                    case MurderAttemptResult.ReverseKill:
                        checkMurderAttemptAndKill(poisoner.poisoned, poisoner.player, true, false);
                        break;
                }

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoisonerSetPoisoned, Hazel.SendOption.Reliable, -1);
                writer.Write(byte.MaxValue);
                writer.Write(byte.MaxValue);
                writer.Write(poisoner.player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.poisonerSetPoisoned(byte.MaxValue, byte.MaxValue, poisoner.player.PlayerId);
            }
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

        // Add a method to calculate weighted role probabilities based on the session's role history
        public static Dictionary<RoleId, float> CalculateWeightedRoleProbabilities(PlayerControl player)
        {
            return new Dictionary<RoleId, float>();
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
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Grenadier) && Grenadier.flashedPlayers.Count > 0)
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

                    if (PlayerControl.LocalPlayer.isRole(RoleId.Grenadier) && Grenadier.flashedPlayers.Count > 0)
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

        public static bool isFlashedByGrenadier(this PlayerControl player)
        {
            return Grenadier.exists && Grenadier.flashTimer > 0f && Grenadier.flashedPlayers.Contains(player);
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

        public static int GetDisplayType(int players)
        {
            if (players <= 15)
                return 0;
            return players > 18 ? 2 : 1;
        }

        public static Vector3 convertPos(int index, int arrangeType, (int x, int y)[] arrangement, Vector3 origin, Vector3[] originOffset, Vector3 contentsOffset, float[] scale, (float x, float y)[] contentAreaMultiplier)
        {
            int num1 = index % arrangement[arrangeType].x;
            int num2 = index / arrangement[arrangeType].x;
            return origin + originOffset[arrangeType] + new Vector3(contentsOffset.x * scale[arrangeType] * contentAreaMultiplier[arrangeType].x * num1, contentsOffset.y * scale[arrangeType] * contentAreaMultiplier[arrangeType].y * num2, (float)(-(double)num2 * 0.0099999997764825821));
        }

        public static IEnumerator TransportPlayers(byte player1, byte player2)
        {
            var TP1 = playerById(player1);
            var TP2 = playerById(player2);
            var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            DeadBody Player1Body = null;
            DeadBody Player2Body = null;
            if (TP1.Data.IsDead)
            {
                foreach (var body in deadBodies) if (body.ParentId == TP1.PlayerId) Player1Body = body;
                if (Player1Body == null) yield break;
            }
            if (TP2.Data.IsDead)
            {
                foreach (var body in deadBodies) if (body.ParentId == TP2.PlayerId) Player2Body = body;
                if (Player2Body == null) yield break;
            }

            if (TP1.inVent && PlayerControl.LocalPlayer.PlayerId == TP1.PlayerId)
            {
                TP1.MyPhysics.ExitAllVents();
            }
            if (TP2.inVent && PlayerControl.LocalPlayer.PlayerId == TP2.PlayerId)
            {
                TP2.MyPhysics.ExitAllVents();
            }

            if (Player1Body == null && Player2Body == null)
            {
                TP1.MyPhysics.ResetMoveState();
                TP2.MyPhysics.ResetMoveState();
                var TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);
                var TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                TP1.transform.position = TP2Position;
                TP1.NetTransform.SnapTo(TP2Position);
                TP2.transform.position = TP1Position;
                TP2.NetTransform.SnapTo(TP1Position);
            }
            else if (Player1Body != null && Player2Body == null)
            {
                TP2.MyPhysics.ResetMoveState();
                var TP1Position = Player1Body.TruePosition;
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);
                var TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                Player1Body.transform.position = TP2Position;
                TP2.transform.position = TP1Position;
                TP2.NetTransform.SnapTo(TP1Position);
            }
            else if (Player1Body == null && Player2Body != null)
            {
                TP1.MyPhysics.ResetMoveState();
                var TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);
                var TP2Position = Player2Body.TruePosition;
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                Player2Body.transform.position = TP1Position;
                TP1.transform.position = TP2Position;
                TP1.NetTransform.SnapTo(TP2Position);
            }
            else if (Player1Body != null && Player2Body != null)
            {
                var TempPosition = Player1Body.TruePosition;
                Player1Body.transform.position = Player2Body.TruePosition;
                Player2Body.transform.position = TempPosition;
            }

            if (PlayerControl.LocalPlayer.PlayerId == TP1.PlayerId || PlayerControl.LocalPlayer.PlayerId == TP2.PlayerId)
            {
                showFlash(Transporter.color);
                if (Minigame.Instance) Minigame.Instance.Close();
            }

            TP1.moveable = true;
            TP2.moveable = true;
            TP1.Collider.enabled = true;
            TP2.Collider.enabled = true;
            TP1.NetTransform.enabled = true;
            TP2.NetTransform.enabled = true;
        }

        public static List<RoleInfo> allRoleInfos()
        {
            var allRoleInfo = new List<RoleInfo>();
            foreach (var role in RoleInfo.allRoleInfos)
            {
                if (role.factionId is FactionId.Modifier) continue;
                allRoleInfo.Add(role);
            }
            return allRoleInfo;
        }

        public static List<RoleInfo> onlineRoleInfo()
        {
            var onlineRoleInfo = new List<RoleInfo>();
            var roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            foreach (var roleInfo in RoleInfo.allRoleInfos)
            {
                if (roleInfo.factionId is FactionId.Modifier) continue;
                if (roleData.nonKillingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.nonKillingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.killingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.killingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleInfo.roleId == RoleId.Vampire && (!CustomOptionHolder.draculaCanCreateVampire.getBool() || CustomOptionHolder.draculaSpawnRate.getSelection() == 0)) continue;
                else if (roleInfo.roleId == RoleId.VampireHunter && CustomOptionHolder.draculaSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Pursuer && CustomOptionHolder.lawyerSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Survivor && CustomOptionHolder.guardianAngelSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Pestilence && CustomOptionHolder.plaguebearerSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Mayor && CustomOptionHolder.politicianSpawnRate.getSelection() == 0) continue;
                onlineRoleInfo.Add(roleInfo);
            }
            return onlineRoleInfo;
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

        public static bool stopGameEndForKillers()
        {
            bool powerCrewAlive = false;

            if (CustomOptionHolder.sheriffBlockGameEnd.getBool() && Sheriff.livingPlayers.Count > 0) powerCrewAlive = true;
            if (CustomOptionHolder.veteranBlockGameEnd.getBool() && Veteran.livingPlayers.Count > 0) powerCrewAlive = true;
            if (CustomOptionHolder.vampireHunterBlockGameEnd.getBool() && VampireHunter.livingPlayers.Count > 0) powerCrewAlive = true;
            if (CustomOptionHolder.mayorBlockGameEnd.getBool() && Mayor.livingPlayers.Count > 0) powerCrewAlive = true;
            if (CustomOptionHolder.swapperBlockGameEnd.getBool() && Swapper.livingPlayers.Count > 0) powerCrewAlive = true;

            return powerCrewAlive;
        }

        public static bool isLovers(this PlayerControl player)
        {
            return player != null && Lovers.isLovers(player);
        }

        public static PlayerControl getPartner(this PlayerControl player)
        {
            return Lovers.getPartner(player);
        }

        internal static bool Check(int probability)
        {
            if (probability == 0) return false;
            if (probability == 100) return true;
            var num = UnityEngine.Random.RandomRangeInt(1, 101);
            return num <= probability;
        }
    }
}