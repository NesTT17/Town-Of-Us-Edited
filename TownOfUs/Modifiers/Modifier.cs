using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch]
    public static class ModifierData
    {
        public static Dictionary<RoleId, Type> allModTypes = new Dictionary<RoleId, Type>
        {
            { RoleId.Bait, typeof(ModifierBase<Bait>) },
            { RoleId.Shifter, typeof(ModifierBase<Shifter>) },
            { RoleId.Blind, typeof(ModifierBase<Blind>) },
            { RoleId.Indomitable, typeof(ModifierBase<Indomitable>) },
            { RoleId.Torch, typeof(ModifierBase<Torch>) },
            { RoleId.Vip, typeof(ModifierBase<Vip>) },

            { RoleId.ButtonBarry, typeof(ModifierBase<ButtonBarry>) },
            { RoleId.Sleuth, typeof(ModifierBase<Sleuth>) },
            { RoleId.Drunk, typeof(ModifierBase<Drunk>) },
            { RoleId.Radar, typeof(ModifierBase<Radar>) },
            { RoleId.Immovable, typeof(ModifierBase<Immovable>) },
            { RoleId.Tiebreaker, typeof(ModifierBase<Tiebreaker>) },
            { RoleId.Chameleon, typeof(ModifierBase<Chameleon>) },

            { RoleId.Disperser, typeof(ModifierBase<Disperser>) },
            { RoleId.DoubleShot, typeof(ModifierBase<DoubleShot>) },
        };
    }

    public abstract class Modifier
    {
        public static List<Modifier> allModifiers = new List<Modifier>();
        public PlayerControl player;
        public RoleId modId;

        public abstract void OnMeetingStart();
        public abstract void OnMeetingEnd();
        public abstract void FixedUpdate();
        public abstract void HudUpdate();
        public abstract void OnKill(PlayerControl target);
        public abstract void OnDeath(PlayerControl killer = null);
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
        public virtual void ResetModifier() { }

        public virtual string modifyNameText(string nameText) { return nameText; }
        public virtual string modifyRoleText(string roleText, List<RoleInfo> roleInfo, bool useColors = true, bool includeHidden = false) { return roleText; }
        public virtual string meetingInfoText() { return ""; }

        public static void ClearAll()
        {
            allModifiers = new List<Modifier>();
        }
    }

    [HarmonyPatch]
    public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
    {
        public static List<T> players = new List<T>();
        public static RoleId ModType;
        public static List<RoleId> persistRoleChange = new List<RoleId>();

        public void Init(PlayerControl player)
        {
            this.player = player;
            players.Add((T)this);
            allModifiers.Add(this);
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

        public static T getModifier(PlayerControl player = null)
        {
            player = player ?? PlayerControl.LocalPlayer;
            return players.FirstOrDefault(x => x.player == player);
        }

        public static bool hasModifier(PlayerControl player)
        {
            return players.Any(x => x.player == player);
        }

        public static T addModifier(PlayerControl player)
        {
            T mod = new T();
            mod.Init(player);
            return mod;
        }

        public static void eraseModifier(PlayerControl player, RoleId newRole = RoleId.NoRole)
        {
            List<T> toRemove = new List<T>();

            foreach (var p in players)
            {
                if (p.player == player && p.modId == ModType && !persistRoleChange.Contains(newRole))
                    toRemove.Add(p);
            }
            players.RemoveAll(x => toRemove.Contains(x));
            allModifiers.RemoveAll(x => toRemove.Contains(x));
        }

        public static void swapModifier(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.player == p1);
            if (index >= 0)
            {
                players[index].player = p2;
            }
        }
    }

    public static class ModifierHelpers
    {
        public static bool hasModifier(this PlayerControl player, RoleId mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    return (bool)t.Value.GetMethod("hasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                }
            }
            return false;
        }

        public static void addModifier(this PlayerControl player, RoleId mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    t.Value.GetMethod("addModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }
        }

        public static void eraseModifier(this PlayerControl player, RoleId mod, RoleId newRole = RoleId.NoRole)
        {
            if (hasModifier(player, mod))
            {
                foreach (var t in ModifierData.allModTypes)
                {
                    if (mod == t.Key)
                    {
                        t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, newRole });
                        return;
                    }
                }
                TownOfUsPlugin.Logger.LogError($"eraseRole: no method found for role type {mod}");
            }
        }

        public static void eraseAllModifiers(this PlayerControl player, RoleId newRole = RoleId.NoRole)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, newRole });
            }
        }

        public static void swapModifiers(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (player.hasModifier(t.Key))
                {
                    t.Value.GetMethod("swapModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }
        }
    }
}