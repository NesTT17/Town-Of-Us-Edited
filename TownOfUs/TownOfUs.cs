using System;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Modifiers;

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
}