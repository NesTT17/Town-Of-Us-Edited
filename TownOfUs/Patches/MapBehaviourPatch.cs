using HarmonyLib;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace TownOfUs.Patches {

	[HarmonyPatch(typeof(MapBehaviour))]
	static class MapBehaviourPatch {
		public static Dictionary<Byte, SpriteRenderer> herePoints = new();
		public static Dictionary<string, GameObject> mapIcons = new();

		public static void clearAndReload() {
			foreach (var mapIcon in mapIcons.Values) {
				mapIcon.Destroy();
			}
			mapIcons = new();
			herePoints = new();
        }

		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
		static void Postfix(MapBehaviour __instance) {
			__instance.HerePoint.transform.SetLocalZ(-2.1f);
			if (Tracker.tracker != null && Tracker.tracker.PlayerId == PlayerControl.LocalPlayer.PlayerId && !Tracker.tracker.Data.IsDead) {
				if (MeetingHud.Instance == null) {
					foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
						if (!Tracker.trackedPlayers.Contains(player)) continue;
						if (player.Data.IsDead) continue;

						Vector3 v = player.transform.position;
						v /= MapUtilities.CachedShipStatus.MapScale;
						v.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
						v.z = -2.1f;

						if (herePoints.ContainsKey(player.PlayerId)) {
							herePoints[player.PlayerId].transform.localPosition = v;
							continue;
						}

						var herePoint = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent, true);
						herePoint.transform.localPosition = v;
						herePoint.enabled = true;
						int colorId = player.CurrentOutfit.ColorId;
						player.CurrentOutfit.ColorId = 6;
						player.SetPlayerMaterialColors(herePoint);
						player.CurrentOutfit.ColorId = colorId;
						herePoints.Add(player.PlayerId, herePoint);
					}
				} else {
					foreach (var s in herePoints) {
						UnityEngine.Object.Destroy(s.Value);
						herePoints.Remove(s.Key);
					}
				}
			}
            HudManagerUpdate.CloseSettings();
        }
	}
}
