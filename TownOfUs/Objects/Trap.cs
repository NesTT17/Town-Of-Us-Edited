using HarmonyLib;
using Reactor.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Objects
{
    public class Trap
    {
        public Dictionary<byte, float> players = new Dictionary<byte, float>();
        public Transform transform;

        public IEnumerator FrameTimer()
        {
            while (transform != null)
            {
                yield return 0;
                Update();
            }
        }

        public void Update()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead) continue;

                if (Vector2.Distance(transform.position, player.GetTruePosition()) < (Trapper.trapSize + 0.01f) * ShipStatus.Instance.MaxLightRadius)
                {
                    if (!players.ContainsKey(player.PlayerId)) players.Add(player.PlayerId, 0f);
                }
                else
                {
                    if (players.ContainsKey(player.PlayerId)) players.Remove(player.PlayerId);
                }

                var entry = player;
                if (players.ContainsKey(entry.PlayerId))
                {
                    players[entry.PlayerId] += Time.deltaTime;
                    if (players[entry.PlayerId] > Trapper.timeInTrap)
                    {
                        RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(entry, false).Where(x => x.factionId != FactionId.Modifier && x.factionId != FactionId.Ghost).FirstOrDefault();
                        if (!Trapper.trappedPlayers.Contains(roleInfo) && entry != Trapper.trapper) Trapper.trappedPlayers.Add(roleInfo);
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    public static class TrapExtentions
    {
        public static void ClearTraps(this List<Trap> obj)
        {
            foreach (Trap t in obj)
            {
                Object.Destroy(t.transform.gameObject);
                Coroutines.Stop(t.FrameTimer());
            }
            obj.Clear();
        }

        public static Trap CreateTrap(this Vector3 location)
        {
            var TrapPref = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            TrapPref.name = "Trap";
            TrapPref.transform.localScale = new Vector3(Trapper.trapSize * ShipStatus.Instance.MaxLightRadius * 2f, Trapper.trapSize * ShipStatus.Instance.MaxLightRadius * 2f, Trapper.trapSize * ShipStatus.Instance.MaxLightRadius * 2f);
            GameObject.Destroy(TrapPref.GetComponent<SphereCollider>());
            TrapPref.GetComponent<MeshRenderer>().material = Trapper.trapMaterial;
            TrapPref.transform.position = location;
            var TrapScript = new Trap();
            TrapScript.transform = TrapPref.transform;
            Coroutines.Start(TrapScript.FrameTimer());
            return TrapScript;
        }
    }
}