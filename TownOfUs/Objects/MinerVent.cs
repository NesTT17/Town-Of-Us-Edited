using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TownOfUs.Objects
{
    public class MinerVent {
        public static List<MinerVent> AllMinerVents = new List<MinerVent>();
        public static int MinerVentLimit = 3;
        public static bool convertedToVents = false;

        public GameObject GameObject;
        public Vent vent;
        public SpriteRenderer holeRender;
        private static Sprite _PolusMinerHole;
        private static Sprite _OtherMinerHole;

        public static Sprite GetMinerConstruction()
            => Helpers.isPolus() ? _PolusMinerHole ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.Vent.png", 200f)
                : _OtherMinerHole ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.Vent.png", 225f);
            
        public MinerVent(Vector2 p) {
            GameObject = new GameObject("MinerVentLocation") { layer = 11 };
            Vector3 position = new(p.x, p.y, p.y / 1000 + 0.0008f); // just behind player
            Vector2 offset = PlayerControl.LocalPlayer.Collider.offset * .7f;
            position += new Vector3(offset.x, offset.y, 0); // Add collider offset that DoMove moves the player up at a valid position

            // Create the marker
            GameObject.transform.position = position;
            holeRender = GameObject.AddComponent<SpriteRenderer>();
            holeRender.sprite = GetMinerConstruction();
            holeRender.color = holeRender.color.SetAlpha(0.5f);

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            vent.transform.position = GameObject.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.Offset = new Vector3(0f, 0.25f, 0f);
            vent.myAnim?.Stop();
            vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            vent.myRend.sprite = GetMinerConstruction();
            if (Helpers.isFungle())
                vent.myRend.transform.localPosition = new Vector3(0, -.01f);
            List<Vent> allVentsList = MapUtilities.CachedShipStatus.AllVents.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
            vent.name = "MinerVent_" + vent.Id;
            vent.gameObject.SetActive(false);

            // Only render the vent for the Miner and for Ghosts
            var showBoxToLocalPlayer = PlayerControl.LocalPlayer == Miner.miner || PlayerControl.LocalPlayer.Data.IsDead;
            GameObject.SetActive(showBoxToLocalPlayer);

            AllMinerVents.Add(this);
        }

        public static void UpdateStates()
        {
            if (convertedToVents == true) return;
            foreach (MinerVent vent in AllMinerVents)
                vent.GameObject.SetActive(PlayerControl.LocalPlayer == Miner.miner || PlayerControl.LocalPlayer.Data.IsDead);
        }

        public void ConvertToVent()
        {
            GameObject.SetActive(true);
            vent.gameObject.SetActive(true);
            holeRender.color = holeRender.color.SetAlpha(1f);
            vent.myRend.sprite = GetMinerConstruction();
            return;
        }

        public static void ConvertToVents()
        {
            foreach (MinerVent vent in AllMinerVents)
                vent.ConvertToVent();
            ConnectVents();
            convertedToVents = true;
            return;
        }

        public static bool hasLimitReached() {
            return (AllMinerVents.Count >= MinerVentLimit);
        }

        private static void ConnectVents()
        {
            for (var i = 0; i < AllMinerVents.Count - 1; i++) {
                var a = AllMinerVents[i];
                var b = AllMinerVents[i + 1];
                a.vent.Right = b.vent;
                b.vent.Left = a.vent;
            }
            // Connect first with last
            AllMinerVents.First().vent.Left = AllMinerVents.Last().vent;
            AllMinerVents.Last().vent.Right = AllMinerVents.First().vent;
        }

        public static void ClearMinerVents()
        {
            convertedToVents = false;
            AllMinerVents = new List<MinerVent>();
        }
    }
}