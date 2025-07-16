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
            HudManagerUpdate.CloseSettings();
        }
	}
}
