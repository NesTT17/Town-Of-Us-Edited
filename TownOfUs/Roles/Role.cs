using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TownOfUs.Roles
{
    public enum RoleId
    {
        // Special Roles
        Sheriff,
        Seer,
        Engineer,
        Jester,
        Juggernaut,
        Snitch,
        Veteran,
        Camouflager,
        Morphling,
        Lawyer,
        Pursuer,
        Politician,
        Mayor,
        Plaguebearer,
        Pestilence,
        Werewolf,
        Dracula,
        Vampire,
        Swapper,
        VampireHunter,
        Medic,
        Survivor,
        GuardianAngel,
        Amnesiac,
        Executioner,
        Spy,
        Poisoner,
        Scavenger,
        Investigator,
        Janitor,
        Swooper,
        Oracle,
        Mercenary,
        Blackmailer,
        Grenadier,
        Mystic,
        Glitch,
        Transporter,
        Arsonist,
        Agent,
        Thief,
        Lighter,
        Detective,
        Bomber,
        Venerer,
        BountyHunter,
        // Modifiers
        Bait,
        Shifter,
        ButtonBarry,
        Blind,
        Sleuth,
        Indomitable,
        Drunk,
        Torch,
        Vip,
        Radar,
        // Vanilla Roles
        Crewmate, Impostor,
        // don't put anything below this
        NoRole = int.MaxValue
    }

    [HarmonyPatch]
    public static class RoleData
    {
        public static Dictionary<RoleId, Type> allRoleTypes = new Dictionary<RoleId, Type>
        {
            { RoleId.Sheriff, typeof(RoleBase<Sheriff>) },
            { RoleId.Seer, typeof(RoleBase<Seer>) },
            { RoleId.Engineer, typeof(RoleBase<Engineer>) },
            { RoleId.Snitch, typeof(RoleBase<Snitch>) },
            { RoleId.Veteran, typeof(RoleBase<Veteran>) },
            { RoleId.Politician, typeof(RoleBase<Politician>) },
            { RoleId.Mayor, typeof(RoleBase<Mayor>) },
            { RoleId.Swapper, typeof(RoleBase<Swapper>) },
            { RoleId.VampireHunter, typeof(RoleBase<VampireHunter>) },
            { RoleId.Medic, typeof(RoleBase<Medic>) },
            { RoleId.Spy, typeof(RoleBase<Spy>) },
            { RoleId.Investigator, typeof(RoleBase<Investigator>) },
            { RoleId.Oracle, typeof(RoleBase<Oracle>) },
            { RoleId.Mystic, typeof(RoleBase<Mystic>) },
            { RoleId.Transporter, typeof(RoleBase<Transporter>) },
            { RoleId.Agent, typeof(RoleBase<Agent>) },
            { RoleId.Lighter, typeof(RoleBase<Lighter>) },
            { RoleId.Detective, typeof(RoleBase<Detective>) },
            
            { RoleId.Lawyer, typeof(RoleBase<Lawyer>) },
            { RoleId.Pursuer, typeof(RoleBase<Pursuer>) },
            { RoleId.GuardianAngel, typeof(RoleBase<GuardianAngel>) },
            { RoleId.Survivor, typeof(RoleBase<Survivor>) },
            { RoleId.Amnesiac, typeof(RoleBase<Amnesiac>) },
            { RoleId.Thief, typeof(RoleBase<Thief>) },

            { RoleId.Jester, typeof(RoleBase<Jester>) },
            { RoleId.Executioner, typeof(RoleBase<Executioner>) },
            { RoleId.Scavenger, typeof(RoleBase<Scavenger>) },
            { RoleId.Mercenary, typeof(RoleBase<Mercenary>) },

            { RoleId.Juggernaut, typeof(RoleBase<Juggernaut>) },
            { RoleId.Plaguebearer, typeof(RoleBase<Plaguebearer>) },
            { RoleId.Pestilence, typeof(RoleBase<Pestilence>) },
            { RoleId.Werewolf, typeof(RoleBase<Werewolf>) },
            { RoleId.Dracula, typeof(RoleBase<Dracula>) },
            { RoleId.Vampire, typeof(RoleBase<Vampire>) },
            { RoleId.Glitch, typeof(RoleBase<Glitch>) },
            { RoleId.Arsonist, typeof(RoleBase<Arsonist>) },
            
            { RoleId.Camouflager, typeof(RoleBase<Camouflager>) },
            { RoleId.Morphling, typeof(RoleBase<Morphling>) },
            { RoleId.Poisoner, typeof(RoleBase<Poisoner>) },
            { RoleId.Janitor, typeof(RoleBase<Janitor>) },
            { RoleId.Swooper, typeof(RoleBase<Swooper>) },
            { RoleId.Blackmailer, typeof(RoleBase<Blackmailer>) },
            { RoleId.Grenadier, typeof(RoleBase<Grenadier>) },
            { RoleId.Bomber, typeof(RoleBase<Bomber>) },
            { RoleId.Venerer, typeof(RoleBase<Venerer>) },
            { RoleId.BountyHunter, typeof(RoleBase<BountyHunter>) },
        };
    }

    public abstract class Role
    {
        public static List<Role> allRoles = new List<Role>();
        public PlayerControl player;
        public RoleId roleId;

        public abstract void OnMeetingStart();
        public abstract void OnMeetingEnd();
        public abstract void FixedUpdate();
        public abstract void OnKill(PlayerControl target);
        public abstract void OnDeath(PlayerControl killer = null);
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
        public virtual void ResetRole(bool isShifted) { }
        public virtual void PostInit()
        {
        }
        public virtual string modifyNameText(string nameText) { return nameText; }
        public virtual string meetingInfoText() { return ""; }

        public static void ClearAll()
        {
            allRoles = new List<Role>();
        }
    }

    [HarmonyPatch]
    public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
    {
        public static List<T> players = new List<T>();
        public static RoleId RoleType;

        public void Init(PlayerControl player)
        {
            this.player = player;
            players.Add((T)this);
            allRoles.Add(this);
            PostInit();
            PlayerGameInfo.addRole(this.player.PlayerId, RoleInfo.roleInfoById[RoleType]);
        }

        public static T local
        {
            get
            {
                return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
            }
        }

        public static List<PlayerControl> allPlayers
        {
            get
            {
                return players.Select(x => x.player).ToList();
            }
        }

        public static List<PlayerControl> livingPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => !x.Data.IsDead && !x.Data.Disconnected).ToList();
            }
        }

        public static List<PlayerControl> deadPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => x.Data.IsDead).ToList();
            }
        }

        public static bool exists
        {
            get { return players.Count > 0; }
        }

        public static bool hasAlivePlayers
        {
            get { return livingPlayers.Count > 0; }
        }

        public static T getRole(PlayerControl player = null)
        {
            player = player ?? PlayerControl.LocalPlayer;
            return players.FirstOrDefault(x => x.player == player);
        }

        public static bool isRole(PlayerControl player)
        {
            return players.Any(x => x.player == player);
        }

        public static T setRole(PlayerControl player)
        {
            if (!isRole(player))
            {
                T role = new T();
                role.Init(player);
                return role;
            }
            return null;
        }

        public static void eraseRole(PlayerControl player)
        {
            players.DoIf(x => x.player == player, x => x.ResetRole(false));
            players.RemoveAll(x => x.player == player && x.roleId == RoleType);
            allRoles.RemoveAll(x => x.player == player && x.roleId == RoleType);
        }

        public static void swapRole(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.player == p1);
            if (index >= 0)
            {
                players.DoIf(x => x.player == p1, x => x.ResetRole(true));
                players[index].player = p2;
                players.DoIf(x => x.player == p2, x => x.PostInit());
            }
        }
    }

    public static class RoleHelpers
    {
        public static bool isRole(this PlayerControl player, RoleId role)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                if (role == t.Key)
                {
                    return (bool)t.Value.GetMethod("isRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                }
            }

            switch (role)
            {
                default:
                    TownOfUsPlugin.Logger.LogError($"isRole: no method found for role type {role}");
                    break;
            }

            return false;
        }

        public static void setRole(this PlayerControl player, RoleId role)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                if (role == t.Key)
                {
                    t.Value.GetMethod("setRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }

            switch (role)
            {
                default:
                    TownOfUsPlugin.Logger.LogError($"setRole: no method found for role type {role}");
                    return;
            }
        }

        public static void eraseRole(this PlayerControl player, RoleId role)
        {
            if (isRole(player, role))
            {
                foreach (var t in RoleData.allRoleTypes)
                {
                    if (role == t.Key)
                    {
                        t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                        return;
                    }
                }
                TownOfUsPlugin.Logger.LogError($"eraseRole: no method found for role type {role}");
            }
        }

        public static void eraseAllRoles(this PlayerControl player)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }
        }

        public static void swapRoles(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                if (player.isRole(t.Key))
                {
                    t.Value.GetMethod("swapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }
        }

        public static string modifyNameText(this PlayerControl player, string nameText)
        {
            if (player == null || player.Data.Disconnected) return nameText;

            foreach (var role in Role.allRoles)
            {
                if (role.player == player)
                    nameText = role.modifyNameText(nameText);
            }

            foreach (var mod in Modifier.allModifiers)
            {
                if (mod.player == player)
                    nameText = mod.modifyNameText(nameText);
            }

            return nameText;
        }

        public static string modifyRoleText(this PlayerControl player, string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false)
        {
            foreach (var mod in Modifier.allModifiers)
            {
                if (mod.player == player)
                    roleText = mod.modifyRoleText(roleText, roleInfo, useColors, includeHidden);
            }
            return roleText;
        }

        public static void OnKill(this PlayerControl player, PlayerControl target)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
        }

        public static void OnDeath(this PlayerControl player, PlayerControl killer)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

            RPCProcedure.updateMeeting(player.PlayerId, true);
        }
    }
}