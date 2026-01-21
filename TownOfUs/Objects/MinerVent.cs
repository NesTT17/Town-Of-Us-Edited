using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Objects
{
    public class MinerVent
    {
        public static System.Collections.Generic.List<MinerVent> AllMinerVents = new System.Collections.Generic.List<MinerVent>();
        public static int MinerVentLimit = 3;
        public static bool ventsConverted = false;

        public GameObject GameObject;
        public Vent vent;
        public SpriteRenderer holeRender;
        private static Sprite _PolusMinerHole;
        private static Sprite _OtherMinerHole;

        public static Sprite GetMinerConstruction()
        {
            return Helpers.isPolus() ? _PolusMinerHole ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MinerVent.png", 200f) : _OtherMinerHole ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.MinerVent.png", 225f);
        }

        public MinerVent(Vector2 p)
        {
            GameObject = new GameObject("MinerVent") { layer = 11 };
            Vector3 position = new(p.x, p.y, p.y / 1000 + 0.0008f); // just behind player
            Vector2 offset = PlayerControl.LocalPlayer.Collider.offset * .7f;
            position += new Vector3(offset.x, offset.y, 0); // Add collider offset that DoMove moves the player up at a valid position

            // Create the marker
            GameObject.transform.position = position;
            holeRender = GameObject.AddComponent<SpriteRenderer>();
            holeRender.sprite = GetMinerConstruction();
            holeRender.color = new Color(1f, 1f, 1f, 0.5f);

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            vent.transform.position = GameObject.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.Offset = new Vector3(0f, 0.1f, 0f);
            vent.myAnim?.Stop();
            vent.Id = MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            vent.myRend.sprite = GetMinerConstruction();
            if (Helpers.isFungle())
            {
                vent.myRend.transform.localPosition = new Vector3(0, -.01f);
            }
            vent.name = "MinerVent_" + vent.Id;
            vent.gameObject.SetActive(false);

            List<Vent> allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();

            var showVentToLocalPlayer = PlayerControl.LocalPlayer == Miner.miner || PlayerControl.LocalPlayer.Data.IsDead;
            GameObject.SetActive(showVentToLocalPlayer);

            AllMinerVents.Add(this);
        }

        public static void UpdateStates()
        {
            if (ventsConverted == true) return;
            foreach (var vent in AllMinerVents)
            {
                var showBoxToLocalPlayer = PlayerControl.LocalPlayer == Miner.miner || PlayerControl.LocalPlayer.Data.IsDead;
                vent.GameObject.SetActive(showBoxToLocalPlayer);
            }
        }

        public void convertToVent()
        {
            GameObject.SetActive(true);
            vent.gameObject.SetActive(true);
            vent.myRend.sprite = GetMinerConstruction();
            return;
        }

        public static void convertToVents()
        {
            foreach (var vent in AllMinerVents)
            {
                vent.convertToVent();
            }
            connectVents();
            ventsConverted = true;
            return;
        }

        public static bool hasMinerVentLimitReached()
        {
            return AllMinerVents.Count >= MinerVentLimit;
        }

        private static void connectVents()
        {
            for (var i = 0; i < AllMinerVents.Count - 1; i++)
            {
                var a = AllMinerVents[i];
                var b = AllMinerVents[i + 1];
                a.vent.Right = b.vent;
                b.vent.Left = a.vent;
            }
            // Connect first with last
            AllMinerVents.First().vent.Left = AllMinerVents.Last().vent;
            AllMinerVents.Last().vent.Right = AllMinerVents.First().vent;
        }

        public static void clearMinerVents()
        {
            ventsConverted = false;
            AllMinerVents = new List<MinerVent>();
        }
    }
}