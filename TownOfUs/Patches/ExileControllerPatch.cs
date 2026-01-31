using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch
    {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
        {
            // Medic shield
            if (Medic.medic != null && AmongUsClient.Instance.AmHost && Medic.futureShielded != null && !Medic.medic.Data.IsDead)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                writer.Write(Medic.futureShielded.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.medicSetShielded(Medic.futureShielded.PlayerId);
            }
            if (Medic.usedShield) Medic.meetingAfterShielding = true;  // Has to be after the setting of the shield

            // Miner Vents
            if (Miner.miner != null && MinerVent.hasMinerVentLimitReached())
            {
                MinerVent.convertToVents();
            }

            // Plumber vents
            foreach (Vent vent in ventsToSeal)
            {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                Sprite newSprite = animator == null ? Plumber.getStaticVentSealedSprite() : Plumber.getAnimatedVentSealedSprite();
                SpriteRenderer rend = vent.myRend;
                if (Helpers.isFungle())
                {
                    newSprite = Plumber.getFungleVentSealedSprite();
                    rend = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                    animator = vent.transform.GetChild(3).GetComponent<PowerTools.SpriteAnim>();
                }
                animator?.Stop();
                rend.sprite = newSprite;
                rend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }
            ventsToSeal = new List<Vent>();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                NetworkedPlayerInfo networkedPlayer = __instance.initData.networkedPlayer;
                WrapUpPostfix((networkedPlayer != null) ? networkedPlayer.Object : null);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                NetworkedPlayerInfo networkedPlayer = __instance.initData.networkedPlayer;
                WrapUpPostfix((networkedPlayer != null) ? networkedPlayer.Object : null);
            }
        }

        static void WrapUpPostfix(PlayerControl exiled)
        {
            // Jester win condition
            if (exiled != null && Jester.IsJester(exiled.PlayerId, out Jester jester))
            {
                if (CustomOptionHolder.jesterWinEndsGame.getBool())
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(exiled.NetId, (byte)CustomRPC.SetJesterWinner, SendOption.Reliable, -1);
                    writer.Write(exiled.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setJesterWinner(exiled.PlayerId);
                }
                else
                {
                    jester.votedOut = true;
                }
            }
            // Executioner win condition
            else if (exiled != null && Executioner.target != null && exiled.PlayerId == Executioner.target.PlayerId && !Executioner.executioner.Data.IsDead)
            {
                if (CustomOptionHolder.executionerWinEndsGame.getBool())
                {
                    Executioner.triggerExecutionerWin = true;
                }
                else
                {
                    Executioner.triggerExile = true;
                    Executioner.executioner.Exiled();
                }
            }
            // Mini exile lose condition
            else if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId && !Mini.isGrownUp() && !Mini.mini.isEvil())
            {
                Mini.triggerMiniLose = true;
            }

            // Politician promotion
            if (Politician.politician != null && Politician.politician == PlayerControl.LocalPlayer && !Politician.politician.Data.IsDead)
            {
                int alivePlayersCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected && x != null);
                int campaignedAlivePlayersCount = Politician.campaignedPlayers.Count(x => !x.Data.IsDead && !x.Data.Disconnected && x != null);
                if (campaignedAlivePlayersCount >= alivePlayersCount)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PoliticianTurnMayor, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.politicianTurnMayor();
                }
            }

            // Plaguebearer promotion
            if (Plaguebearer.plaguebearer != null && Plaguebearer.plaguebearer == PlayerControl.LocalPlayer && !Plaguebearer.plaguebearer.Data.IsDead)
            {
                int alivePlayersCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected && x != null);
                int infectedAlivePlayersCount = Plaguebearer.infectedPlayers.Count(x => !x.Data.IsDead && !x.Data.Disconnected && x != null);
                if (infectedAlivePlayersCount >= alivePlayersCount)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaguebearerTurnPestilence, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.plaguebearerTurnPestilence();
                }
            }

            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Mini set adapted cooldown
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor)
            {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier);
            }

            // Seer deactivate dead poolable players
            if (Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer)
            {
                int visibleCounter = 0;
                Vector3 newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
                var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!playerIcons.ContainsKey(p.PlayerId)) continue;
                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        playerIcons[p.PlayerId].transform.localPosition = newBottomLeft + Vector3.right * visibleCounter * 0.4f;
                        visibleCounter++;
                    }
                }
            }

            // Unblackmail players
            if (Blackmailer.blackmailed != null)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UnblackmailPlayer, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.unblackmailPlayer();
            }

            // Mayor add bodyguards
            if (Mayor.mayor != null && !Mayor.mayor.Data.IsDead) Mayor.numberOfGuards++;

            // Shy update
            Shy.lastMoved.Clear();

            // Immovable set positon
            Immovable.setPosition();

            // Drunk add meeting
            if (Drunk.meetings > 0) Drunk.meetings--;

            // Satelite reset deadBodyPositions
            Satelite.deadBodyPositions = new List<Vector3>();

            // Taskmaster Complete task
            if (Taskmaster.taskMaster != null && Taskmaster.taskMaster == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                var taskinfos = PlayerControl.LocalPlayer.Data.Tasks.ToArray();
                var tasksLeft = taskinfos.Count(x => !x.Complete);
                if (tasksLeft != 0)
                {
                    var i = UnityEngine.Random.RandomRangeInt(PlayerControl.LocalPlayer.myTasks.Count - taskinfos.Count, PlayerControl.LocalPlayer.myTasks.Count);
                    while (true)
                    {
                        var task = PlayerControl.LocalPlayer.myTasks[i];
                        if (task.TryCast<NormalPlayerTask>() != null)
                        {
                            var normalPlayerTask = task.Cast<NormalPlayerTask>();

                            if (normalPlayerTask.IsComplete)
                            {
                                i++;
                                if (i >= PlayerControl.LocalPlayer.myTasks.Count) i = 0;
                                continue;
                            }

                            if (normalPlayerTask.TaskType == TaskTypes.PickUpTowels)
                            {
                                normalPlayerTask.Data = new Il2CppStructArray<byte>([250, 250, 250, 250, 250, 250, 250, 250]);
                                foreach (var console in UnityEngine.Object.FindObjectsOfType<TowelTaskConsole>())
                                    console.Image.color = Color.clear;
                            }
                            while (normalPlayerTask.taskStep < normalPlayerTask.MaxStep) normalPlayerTask.NextStep();

                            break;
                        }
                        else
                        {
                            i++;
                            if (i >= PlayerControl.LocalPlayer.myTasks.Count) i = 0;
                        }
                    }
                }
            }

            // Force Scavenger Bounty Update
            if (Scavenger.scavenger != null && Scavenger.scavenger == PlayerControl.LocalPlayer)
                Scavenger.bountyUpdateTimer = 0f;

            // Vampire Hunter Promote
            if (VampireHunter.vampireHunter != null && VampireHunter.vampireHunter == PlayerControl.LocalPlayer)
            {
                if (!VampireHunter.canStake) VampireHunter.canStake = true;
                PlayerControlFixedUpdatePatch.vampireHunterCheckPromotion(true);
            }

            // Clear Deceiver Decoys
            if ((DeceiverDecoy.ResetPlaceAfterMeeting && DeceiverDecoy.DecoyPermanent) || !DeceiverDecoy.DecoyPermanent)
            {
                Deceiver.decoy?.Destroy();
                Deceiver.decoy = null;
            }

            // Random Spawn Positions
            if (CustomOptionHolder.randomSpawnPositions.getSelection() == 2)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return;
                if (Immovable.immovable.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId)) return;

                if (MapBehaviour.Instance)
                    MapBehaviour.Instance.Close();
                if (Minigame.Instance)
                    Minigame.Instance.ForceClose();
                if (PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                }

                var SpawnPositions = GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
                {
                    0 => MapData.SkeldSpawnPosition,
                    1 => MapData.MiraSpawnPosition,
                    2 => MapData.PolusSpawnPosition,
                    3 => MapData.DleksSpawnPosition,
                    4 => MapData.AirshipSpawnPosition,
                    5 => MapData.FungleSpawnPosition,
                    _ => MapData.FindVentSpawnPositions(),
                };
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SpawnPositions[rnd.Next(SpawnPositions.Count)]);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]  // Set position of AntiTp players AFTER they have selected a spawn.
    class AirshipSpawnInPatch
    {
        static void Postfix()
        {
            Shy.lastMoved.Clear();
            Immovable.setPosition();
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.initData != null)
                {
                    PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.name).ToArray());
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                    {
                        if (player.IsJester(out _)) __result = "";
                    }
                    if (Tiebreaker.isTiebreak) __result += " (Tiebreaker)";
                    Tiebreaker.isTiebreak = false;
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}