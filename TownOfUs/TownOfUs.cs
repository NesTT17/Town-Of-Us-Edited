using System;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Modifiers;
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
            ResetButtonCooldown.clearAndReload();

            Sheriff.Clear();
            Seer.Clear();
            Engineer.Clear();
            Jester.Clear();
            Juggernaut.Clear();
            Snitch.Clear();
            Veteran.Clear();
            Camouflager.Clear();
            Morphling.Clear();
            Lawyer.Clear();
            Pursuer.Clear();
            Politician.Clear();
            Mayor.Clear();
            Plaguebearer.Clear();
            Pestilence.Clear();
            Dracula.Clear();
            Vampire.Clear();
            Werewolf.Clear();
            Swapper.Clear();
            VampireHunter.Clear();
            Medic.Clear();
            Survivor.Clear();
            GuardianAngel.Clear();
            Amnesiac.Clear();
            Executioner.Clear();
            Spy.Clear();
            Poisoner.Clear();
            Scavenger.Clear();
            Investigator.Clear();
            Janitor.Clear();
            Swooper.Clear();
            Oracle.Clear();
            Mercenary.Clear();
            Blackmailer.Clear();
            Grenadier.Clear();
            Mystic.Clear();
            Glitch.Clear();
            Transporter.Clear();
            Arsonist.Clear();
            Agent.Clear();
            Thief.Clear();
            Lighter.Clear();
            Detective.Clear();
            Bomber.Clear();
            Venerer.Clear();
            BountyHunter.Clear();
            Doomsayer.Clear();

            Bait.Clear();
            Shifter.Clear();
            ButtonBarry.Clear();
            Blind.Clear();
            Sleuth.Clear();
            Indomitable.Clear();
            Drunk.Clear();
            Torch.Clear();
            Vip.Clear();
            Radar.Clear();
            Disperser.Clear();
            Lovers.clearAndReload();
            DoubleShot.Clear();
            Immovable.Clear();
            Chameleon.Clear();

            Role.ClearAll();
            Modifier.ClearAll();

            HandleGuesser.clearAndReload();
        }

        public static class ResetButtonCooldown
        {
            public static float initialCooldown = 10f;

            public static void clearAndReload()
            {
                initialCooldown = CustomOptionHolder.initialCooldown.getFloat();
            }
        }

        public static void FixedUpdate(PlayerControl player)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.FixedUpdate());
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
        }

        public static void HudUpdate()
        {
            Modifier.allModifiers.Do(x => x.HudUpdate());
        }

        public static void OnMeetingStart()
        {
            Role.allRoles.Do(x => x.OnMeetingStart());
            Modifier.allModifiers.Do(x => x.OnMeetingStart());
        }

        public static void OnMeetingEnd()
        {
            Role.allRoles.Do(x => x.OnMeetingEnd());
            Modifier.allModifiers.Do(x => x.OnMeetingEnd());
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
                    Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));
                }
            }
        }

        public static void OnKill(this PlayerControl player, PlayerControl target)
        {
            foreach (var role in new List<Role>(Role.allRoles))
            {
                if (role.player == player) role.OnKill(target);
            }
            foreach (var modifier in new List<Modifier>(Modifier.allModifiers))
            {
                if (modifier.player == player) modifier.OnKill(target);
            }
        }

        public static void OnDeath(this PlayerControl player, PlayerControl killer)
        {
            foreach (var role in new List<Role>(Role.allRoles))
            {
                if (role.player == player) role.OnDeath(killer);
            }
            if (player.isLovers())
                Lovers.killLovers(player, killer);
            foreach (var modifier in new List<Modifier>(Modifier.allModifiers))
            {
                if (modifier.player == player) modifier.OnDeath(killer);
            }
            if (Helpers.ShowKillAnimation)
            {
                RPCProcedure.updateMeeting(player.PlayerId);
                if (FastDestroyableSingleton<HudManager>.Instance != null && PlayerControl.LocalPlayer == player)
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();

                // remove shoot button from targets for all guessers and close their guesserUI
                if (GuesserGM.isGuesser(PlayerControl.LocalPlayer.PlayerId) && !PlayerControl.LocalPlayer.Data.IsDead && GuesserGM.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance)
                {
                    MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == player.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                    if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null)
                    {
                        if (MeetingHudPatch.guesserCurrentTarget == player.PlayerId)
                            MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                    }
                }
            }
        }
    }

    public class Couple
    {
        public PlayerControl lover1;
        public PlayerControl lover2;
        public Color color;

        public Couple(PlayerControl lover1, PlayerControl lover2, Color color)
        {
            this.lover1 = lover1;
            this.lover2 = lover2;
            this.color = color;
            notAckedExiledIsLover = false;
        }

        public bool notAckedExiledIsLover = false;

        public string icon
        {
            get
            {
                return Helpers.cs(color, " ♥");
            }
        }

        public bool existing
        {
            get
            {
                return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
            }
        }

        public bool alive
        {
            get
            {
                return lover1 != null && lover2 != null && !lover1.Data.IsDead && !lover2.Data.IsDead;
            }
        }

        public bool existingAndAlive
        {
            get
            {
                return existing && alive;
            }
        }

        public bool existingWithKiller
        {
            get
            {
                return existing && (Helpers.isKiller(lover1) || Helpers.isKiller(lover2));
            }
        }

        public bool hasAliveKillingLover
        {
            get
            {
                return existingAndAlive && existingWithKiller;
            }
        }
    }

    public static class Lovers {
        public static List<Couple> couples = [];
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);
        public static List<Color> loverIconColors =
        [
            color,                         // pink
            new Color32(255, 165, 0, 255), // orange
            new Color32(255, 255, 0, 255), // yellow
            new Color32(0, 255, 0, 255),   // green
            new Color32(0, 0, 255, 255),   // blue
            new Color32(0, 255, 255, 255), // light blue
            new Color32(255, 0, 0, 255),   // red
            new Color32(255, 255, 240, 255), // ivory
            new Color32(238, 130, 238, 255), // violet
            new Color32(255, 127, 80, 255), // coral
            new Color32(0, 250, 154, 255), // aquamarine
        ];

        public static string getIcon(PlayerControl player)
        {
            if (isLovers(player))
            {
                var couple = couples.Find(x => x.lover1 == player || x.lover2 == player);
                return couple.icon;
            }
            return "";
        }

        public static Color getColor(PlayerControl player)
        {
            if (isLovers(player))
            {
                var couple = couples.Find(x => x.lover1 == player || x.lover2 == player);
                return couple.color;
            }
            return color;
        }

        public static bool isLovers(PlayerControl player)
        {
            return getCouple(player) != null;
        }

        public static Couple getCouple(PlayerControl player)
        {
            foreach (var pair in couples)
            {
                if (pair.lover1?.PlayerId == player?.PlayerId || pair.lover2?.PlayerId == player?.PlayerId) return pair;
            }
            return null;
        }

        public static PlayerControl getPartner(PlayerControl player)
        {
            var couple = getCouple(player);
            if (couple != null)
            {
                return player?.PlayerId == couple.lover1?.PlayerId ? couple.lover2 : couple.lover1;
            }
            return null;
        }

        public static void killLovers(PlayerControl player, PlayerControl killer = null)
        {
            if (!player.isLovers()) return;

            if (!bothDie) return;

            var partner = getPartner(player);
            if (partner != null)
            {
                if (!partner.Data.IsDead)
                {
                    if (killer != null)
                    {
                        partner.MurderPlayer(partner, MurderResultFlags.Succeeded);
                    }
                    else
                    {
                        if (PlayerControl.LocalPlayer == partner && Helpers.ShowKillAnimation)
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(partner.Data, partner.Data);
                        partner.Exiled();
                    }

                    GameHistory.overrideDeathReasonAndKiller(partner, DeadPlayer.CustomDeathReason.LoverSuicide, null);
                }
            }
        }

        public static void addCouple(PlayerControl player1, PlayerControl player2)
        {
            var availableColors = new List<Color>(loverIconColors);
            foreach (var couple in couples) {
                availableColors.RemoveAll(x => x == couple.color);
            }
            var newCouple = new Couple(player1, player2, availableColors[0]);
            couples.Add(newCouple);
        }

        public static void eraseCouple(PlayerControl player)
        {
            couples.RemoveAll(x => x.lover1 == player || x.lover2 == player);
        }

        public static void swapLovers(PlayerControl player1, PlayerControl player2)
        {
            var couple1 = couples.FindIndex(x => x.lover1 == player1 || x.lover2 == player1);
            var couple2 = couples.FindIndex(x => x.lover1 == player2 || x.lover2 == player2);

            // trying to swap within the same couple, just ignore
            if (couple1 == couple2) return;

            if (couple1 >= 0)
            {
                if (couples[couple1].lover1 == player1) couples[couple1].lover1 = player2;
                if (couples[couple1].lover2 == player1) couples[couple1].lover2 = player2;
            }

            if (couple2 >= 0)
            {
                if (couples[couple2].lover1 == player2) couples[couple2].lover1 = player1;
                if (couples[couple2].lover2 == player2) couples[couple2].lover2 = player1;
            }
        }

        public static bool anyAlive()
        {
            foreach (var couple in couples)
            {
                if (couple.alive) return true;
            }
            return false;
        }

        public static bool anyNonKillingCouples()
        {
            foreach (var couple in couples)
            {
                if (!couple.hasAliveKillingLover) return true;
            }
            return false;
        }

        public static bool existingAndAlive(PlayerControl player)
        {
            var couple = getCouple(player);
            return couple != null && couple.existingAndAlive && !couple.notAckedExiledIsLover;
        }

        public static bool existingWithKiller(PlayerControl player)
        {
            return getCouple(player)?.existingWithKiller == true;
        }

        public static void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            eraseCouple(player);
        }

        public static bool bothDie = true;
        public static bool enableChat = true;

        public static bool hasAliveKillingLover(this PlayerControl player) {
            if (!existingAndAlive(player) || !existingWithKiller(player))
                return false;
            return player != null && isLovers(player);
        }

        public static void clearAndReload() {
            bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
            enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
            couples = [];
        }
    }
}