using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Objects
{
    public class Bomb { public Transform transform; }

    [HarmonyPatch]
    public static class BombExtensions {
        public static void ClearBomb(this Bomb b) {
            Object.Destroy(b.transform.gameObject);
            b = null;
        }

        public static Bomb CreateBomb(this Vector3 location)
        {
            var BombPref = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            BombPref.name = "Bomb";
            BombPref.transform.localScale = new Vector3(Bomber.radius * ShipStatus.Instance.MaxLightRadius * 2f, Bomber.radius * ShipStatus.Instance.MaxLightRadius * 2f, Bomber.radius * ShipStatus.Instance.MaxLightRadius * 2f);
            GameObject.Destroy(BombPref.GetComponent<SphereCollider>());
            BombPref.GetComponent<MeshRenderer>().material = Bomber.bombMaterial;
            BombPref.transform.position = location;
            var BombScript = new Bomb();
            BombScript.transform = BombPref.transform;
            return BombScript;
        }
    }
}