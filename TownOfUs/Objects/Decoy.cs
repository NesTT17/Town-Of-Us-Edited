using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.Objects
{
    public class DeceiverDecoy : CustomObjectBase<DeceiverDecoy>
    {
        public PlayerControl Player;
        public float elapsedTime;

        public static bool ResetPlaceAfterMeeting;
        public static float DecoyDelayedDisplay;
        public static bool DecoyPermanent;
        public static float DecoyDuration;

        private static Sprite decoySprite;
        public static Sprite getDecoySprite() => decoySprite ??= Helpers.loadSpriteFromResources("TownOfUs.Resources.Decoy.png", 150f);

        public DeceiverDecoy(PlayerControl player, Vector3 pos)
        {
            ResetPlaceAfterMeeting = CustomOptionHolder.deceiverResetPlaceAfterMeeting.getBool();
            DecoyDuration = CustomOptionHolder.deceiverDecoyDuration.getFloat();
            DecoyPermanent = !CustomOptionHolder.deceiverDecoyPermanent.getBool();
            DecoyDelayedDisplay = CustomOptionHolder.deceiverDecoyDelayedDisplay.getFloat();

            Player = player;
            GameObject.name = "DeceiverDecoy " + Id;
            GameObject.SetActive(false);
            GameObject.transform.position = pos;
            Renderer.sprite = getDecoySprite();
            Renderer.color = Color.white * new Vector4(1, 1, 1, 0.66f);
            elapsedTime = 0f;

            _ = new LateTask(() =>
            {
                if (GameObject == null) return;
                IsActive = true;
                Renderer.color = Color.white;
            }, DecoyDelayedDisplay);
        }

        public override void OnDestroy()
        {
            Behaviour?.Destroy();
            Renderer?.Destroy();
            GameObject?.Destroy();
            GameObject = null;
            base.OnDestroy();
        }

        public override void Update()
        {
            if (!MeetingHud.Instance)
            {
                elapsedTime += Time.deltaTime;
                if (!DecoyPermanent && elapsedTime >= DecoyDuration)
                {
                    RPCProcedure.deceiverDecoyDestroy(Player, Id);
                    return;
                }
            }

            if (!DecoyPermanent && MeetingHud.Instance)
            {
                RPCProcedure.deceiverDecoyDestroy(Player, Id);
                return;
            }
            else if (MeetingHud.Instance)
            {
                GameObject.SetActive(false);
                return;
            }

            var canSee = PlayerControl.LocalPlayer == Deceiver.deceiver || Helpers.shouldShowGhostInfo() || (IsActive && ((Deceiver.showDecoy == 2 && PlayerControl.LocalPlayer.Data.Role.IsImpostor) || Deceiver.showDecoy == 3));

            GameObject.SetActive(canSee);
        }
    }
}