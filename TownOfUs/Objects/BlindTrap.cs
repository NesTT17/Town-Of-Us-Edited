using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Objects
{
    class BlindTrap
    {
        public static List<BlindTrap> blindTraps = new List<BlindTrap>();

        private static int instanceCounter = 0;
        public int instanceId = 0;
        public GameObject blindTrap;
        public bool revealed = false;

        private static Sprite trapSprite;
        public static Sprite getTrapSprite()
        {
            if (trapSprite) return trapSprite;
            trapSprite = Helpers.loadSpriteFromResources("TownOfUs.Resources.BlindTrap.png", 300f);
            return trapSprite;
        }

        public BlindTrap(Vector2 p)
        {
            blindTrap = new GameObject("BlindTrap") { layer = 11 };
            Vector3 position = new Vector3(p.x, p.y, p.y / 1000 + 0.001f); // just behind player
            blindTrap.transform.position = position;

            var trapRenderer = blindTrap.AddComponent<SpriteRenderer>();
            trapRenderer.sprite = getTrapSprite();
            blindTrap.SetActive(false);
            if (PlayerControl.LocalPlayer.PlayerId == Poisoner.poisoner.PlayerId) blindTrap.SetActive(true);
            trapRenderer.color = Color.white * new Vector4(1, 1, 1, 0.5f);
            this.instanceId = ++instanceCounter;
            blindTraps.Add(this);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5, new Action<float>((x) =>
            {
                if (x == 1f)
                {
                    trapRenderer.color = Color.white;
                }
            })));
        }

        public static void clearBlindTraps()
        {
            foreach (BlindTrap t in blindTraps)
            {
                UnityEngine.Object.Destroy(t.blindTrap);
            }
            blindTraps = new List<BlindTrap>();
            instanceCounter = 0;
        }

        public static void clearRevealedBlindTraps()
        {
            var trapsToClear = blindTraps.FindAll(x => x.revealed);

            foreach (BlindTrap t in trapsToClear)
            {
                blindTraps.Remove(t);
                UnityEngine.Object.Destroy(t.blindTrap);
            }
        }

        public static void triggerTrap(byte playerId, byte trapId)
        {
            BlindTrap t = blindTraps.FirstOrDefault(x => x.instanceId == (int)trapId);
            PlayerControl player = Helpers.playerById(playerId);
            if (Poisoner.poisoner == null || t == null || player == null) return;
            bool localIsPoisoner = PlayerControl.LocalPlayer.PlayerId == Poisoner.poisoner.PlayerId;
            if (playerId == PlayerControl.LocalPlayer.PlayerId || playerId == Poisoner.poisoner.PlayerId)
            {
                t.blindTrap.SetActive(true);
                SoundEffectsManager.play("trapperTrap");
            }
            Poisoner.blindTrappedPlayers.Add(player.PlayerId);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Poisoner.trapDuration, new Action<float>((p) =>
            {
                if (p == 1f)
                {
                    Poisoner.blindTrappedPlayers.RemoveAll(x => x == player.PlayerId);
                }
            })));

            t.revealed = true;
        }

        public static void Update()
        {
            if (Poisoner.poisoner == null) return;
            var player = PlayerControl.LocalPlayer;
            Vent vent = MapUtilities.CachedShipStatus.AllVents[0];
            float closestDistance = float.MaxValue;
            if (vent == null || player == null) return;

            float ud = vent.UsableDistance / 2;
            BlindTrap target = null;
            foreach (BlindTrap trap in blindTraps)
            {
                if (trap.revealed || Poisoner.blindTrappedPlayers.Contains(player.PlayerId)) continue;
                if (!player.CanMove) continue;
                float distance = Vector2.Distance(trap.blindTrap.transform.position, player.GetTruePosition());
                if (distance <= ud && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = trap;
                }
            }

            if (target != null && player.PlayerId != Poisoner.poisoner.PlayerId && !player.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TriggerBlindTrap, Hazel.SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                writer.Write(target.instanceId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.triggerBlindTrap(player.PlayerId, (byte)target.instanceId);
            }

            if (!player.Data.IsDead || player.PlayerId == Poisoner.poisoner.PlayerId) return;

            foreach (BlindTrap trap in blindTraps)
            {
                if (!trap.blindTrap.active) trap.blindTrap.SetActive(true);
            }
        }
    }
}