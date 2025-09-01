using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfUs.Roles
{
    [HarmonyPatch]
    public class BountyHunter : RoleBase<BountyHunter>
    {
        public static Color color = Palette.ImpostorRed;

        public static float bountyDuration { get => CustomOptionHolder.bountyHunterBountyDuration.getFloat(); }
        public static bool showArrow { get => CustomOptionHolder.bountyHunterShowArrow.getBool(); }
        public static float bountyKillCooldown { get => CustomOptionHolder.bountyHunterReducedCooldown.getFloat(); }
        public static float punishmentTime { get => CustomOptionHolder.bountyHunterPunishmentTime.getFloat(); }
        public static float arrowUpdateIntervall { get => CustomOptionHolder.bountyHunterArrowUpdateIntervall.getFloat(); }

        public Arrow arrow;
        public float arrowUpdateTimer = 0f;
        public float bountyUpdateTimer = 0f;
        public PlayerControl bounty;
        public TMPro.TextMeshPro cooldownText;
        public BountyHunter()
        {
            RoleType = roleId = RoleId.BountyHunter;
            bounty = null;
            arrowUpdateTimer = 0f;
            bountyUpdateTimer = 0f;
        }

        public void generateCooldownText()
        {
            if (FastDestroyableSingleton<HudManager>.Instance != null)
            {
                cooldownText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                cooldownText.transform.localScale = Vector3.one * 0.4f;
                cooldownText.gameObject.SetActive(PlayerControl.LocalPlayer == player);
            }
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        { 
            if (player == PlayerControl.LocalPlayer)
                bountyUpdateTimer = 0f;
        }
        public override void FixedUpdate()
        { 
            if (PlayerControl.LocalPlayer != player) return;

            if (player.Data.IsDead)
            {
                if (arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
                arrow = null;
                if (cooldownText != null && cooldownText.gameObject != null) UnityEngine.Object.Destroy(cooldownText.gameObject);
                cooldownText = null;
                bounty = null;
                resetPoolables();
                return;
            }

            arrowUpdateTimer -= Time.fixedDeltaTime;
            bountyUpdateTimer -= Time.fixedDeltaTime;

            if (bounty == null || bountyUpdateTimer <= 0f)
            {
                // Set new bounty
                bounty = null;
                arrowUpdateTimer = 0f; // Force arrow to update
                bountyUpdateTimer = bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && !p.isRole(RoleId.Agent) && !Vampire.players.Any(x => x.player == p && x.wasTeamRed) && !Dracula.players.Any(x => x.player == p && x.wasTeamRed))
                        possibleTargets.Add(p);
                if (possibleTargets.Count == 0) return;
                bounty = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                if (bounty == null) return;

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    resetPoolables();
                    if (playerIcons.ContainsKey(bounty.PlayerId) && playerIcons[bounty.PlayerId].gameObject != null)
                        playerIcons[bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Hide in meeting
            if (MeetingHud.Instance && playerIcons.ContainsKey(bounty.PlayerId) && playerIcons[bounty.PlayerId].gameObject != null)
                playerIcons[bounty.PlayerId].gameObject.SetActive(false);

            // Update Cooldown Text
            if (cooldownText != null)
            {
                cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(bountyUpdateTimer, 0, bountyDuration)).ToString();
                cooldownText.gameObject.SetActive(!MeetingHud.Instance);  // Show if not in meeting
                cooldownText.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f);
            }
            else generateCooldownText();

            // Update Arrow
            if (showArrow && bounty != null)
            {
                arrow ??= new Arrow(Color.red);
                if (arrowUpdateTimer <= 0f)
                {
                    arrow.Update(bounty.transform.position);
                    arrowUpdateTimer = arrowUpdateIntervall;
                }
                arrow.Update();
            }
        }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                if (target == bounty)
                {
                    player.SetKillTimer(bountyKillCooldown);
                    bountyUpdateTimer = 0f; // Force bounty update
                }
                else
                {
                    player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + punishmentTime);
                }
            }
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public override void ResetRole(bool isShifted)
        {
            if (arrow?.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = null;
            TOUMapOptions.resetPoolables();
            if (cooldownText != null && cooldownText.gameObject != null) {
                UnityEngine.Object.Destroy(cooldownText.gameObject);
                cooldownText = null;
            }
        }

        public static void Clear()
        {
            players = new List<BountyHunter>();
        }
    }
}