using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TMPro;
using Reactor.Utilities;
using Assets.CoreScripts;

namespace TownOfUs.Patches {
    [HarmonyPatch]
    class MeetingHudPatch {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static NetworkedPlayerInfo target = null;
        private const float scale = 0.65f;
        private static TMPro.TextMeshPro meetingExtraButtonText;
        private static PassiveButton[] swapperButtonList;
        private static TMPro.TextMeshPro meetingExtraButtonLabel;
        private static PlayerVoteArea swapped1 = null;
        private static PlayerVoteArea swapped2 = null;
        public static bool shookAlready;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance) {
                Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254) {
                        PlayerControl player = Helpers.playerById((byte)playerVoteArea.TargetPlayerId);
                        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;

                        int currentVotes;
                        int additionalVotes = (Mayor.mayor != null && Mayor.mayor.PlayerId == playerVoteArea.TargetPlayerId && Mayor.isRevealed) ? 3 : 1; // Mayor vote
                        if (dictionary.TryGetValue(playerVoteArea.VotedFor, out currentVotes))
                            dictionary[playerVoteArea.VotedFor] = currentVotes + additionalVotes;
                        else
                            dictionary[playerVoteArea.VotedFor] = additionalVotes;
                    }
                }

                // Swapper swap votes
                if (Swapper.swapper != null && !Swapper.swapper.Data.IsDead) {
                    swapped1 = null;
                    swapped2 = null;
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                    }

                    if (swapped1 != null && swapped2 != null) {
                        if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                        if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                        int tmp = dictionary[swapped1.TargetPlayerId];
                        dictionary[swapped1.TargetPlayerId] = dictionary[swapped2.TargetPlayerId];
                        dictionary[swapped2.TargetPlayerId] = tmp;
                    }
                }
                return dictionary;
            }

            static bool Prefix(MeetingHud __instance) {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote)) {
                    if (target == null && TOUMapOptions.blockSkippingInEmergencyMeetings == 1) {
                        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                            if (playerVoteArea.VotedFor == byte.MaxValue - 1) playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                    } else if (TOUMapOptions.blockSkippingInEmergencyMeetings == 2) {
                        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                            if (playerVoteArea.VotedFor == byte.MaxValue - 1) playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                    }

			        Dictionary<byte, int> self = CalculateVotes(__instance);
                    bool tie;
			        KeyValuePair<byte, int> max = self.MaxPair(out tie);
                    NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                    // TieBreaker 
                    List<NetworkedPlayerInfo> potentialExiled = new List<NetworkedPlayerInfo>();
                    bool skipIsTie = false;
                    if (self.Count > 0) {
                        Tiebreaker.isTiebreak = false;
                        int maxVoteValue = self.Values.Max();
                        PlayerVoteArea tb = null;
                        if (Tiebreaker.tiebreaker != null)
                            tb = __instance.playerStates.ToArray().FirstOrDefault(x => x.TargetPlayerId == Tiebreaker.tiebreaker.PlayerId);
                        bool isTiebreakerSkip = tb == null || tb.VotedFor == 253;
                        if (tb != null && tb.AmDead) isTiebreakerSkip = true;

                        foreach (KeyValuePair<byte, int> pair in self) {
                            if (pair.Value != maxVoteValue || isTiebreakerSkip) continue;
                            if (pair.Key != 253)
                                potentialExiled.Add(GameData.Instance.AllPlayers.ToArray().FirstOrDefault(x => x.PlayerId == pair.Key));
                            else 
                                skipIsTie = true;
                        }
                    }

                    MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        array[i] = new MeetingHud.VoterState {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };

                        if (Tiebreaker.tiebreaker == null || playerVoteArea.TargetPlayerId != Tiebreaker.tiebreaker.PlayerId) continue;

                        byte tiebreakerVote = playerVoteArea.VotedFor;
                        if (swapped1 != null && swapped2 != null) {
                            if (tiebreakerVote == swapped1.TargetPlayerId) tiebreakerVote = swapped2.TargetPlayerId;
                            else if (tiebreakerVote == swapped2.TargetPlayerId) tiebreakerVote = swapped1.TargetPlayerId;
                        }

                        if (potentialExiled.FindAll(x => x != null && x.PlayerId == tiebreakerVote).Count > 0 && (potentialExiled.Count > 1 || skipIsTie)) {
                            exiled = potentialExiled.ToArray().FirstOrDefault(v => v.PlayerId == tiebreakerVote);
                            tie = false;

                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTiebreak, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.setTiebreak();
                        }
                    }

                    // RPCVotingComplete
                    __instance.RpcVotingComplete(array, exiled, tie);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        class MeetingHudBloopAVoteIconPatch {
            public static bool Prefix(MeetingHud __instance, NetworkedPlayerInfo voterPlayer, int index, Transform parent) {
                var spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                var showVoteColors = !GameManager.Instance.LogicOptions.GetAnonymousVotes() ||
                                      (PlayerControl.LocalPlayer.Data.IsDead && TOUMapOptions.ghostsSeeVotes);
                if (showVoteColors)
                {
                    PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
                }
                else
                {
                    PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
                }

                var transform = spriteRenderer.transform;
                transform.SetParent(parent);
                transform.localScale = Vector3.zero;
                var component = parent.GetComponent<PlayerVoteArea>();
                if (component != null)
                {
                    spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
                }

                __instance.StartCoroutine(Effects.Bloop(index * 0.3f, transform));
                parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        class MeetingHudPopulateVotesPatch {
            private static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states) {
                // Swapper swap
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }
                bool doSwap = swapped1 != null && swapped2 != null && Swapper.swapper != null && !Swapper.swapper.Data.IsDead;
                if (doSwap) {
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
                }

                __instance.TitleText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte targetPlayerId = playerVoteArea.TargetPlayerId;
                    // Swapper change playerVoteArea that gets the votes
                    if (doSwap && playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId) playerVoteArea = swapped2;
                    else if (doSwap && playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId) playerVoteArea = swapped1;

                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    for (int j = 0; j < states.Length; j++) {
                        MeetingHud.VoterState voterState = states[j];
                        NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null) {
                            Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                        } else if (i == 0 && voterState.SkippedVote && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.mayor != null && voterState.VoterId == (sbyte)Mayor.mayor.PlayerId && Mayor.isRevealed && i == 0 && voterState.SkippedVote && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                            num++;  
                        } else if (Mayor.mayor != null && voterState.VoterId == (sbyte)Mayor.mayor.PlayerId && Mayor.isRevealed && voterState.VotedForId == targetPlayerId && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                            num2++;   
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]byte[] states, [HarmonyArgument(1)]NetworkedPlayerInfo exiled, [HarmonyArgument(2)]bool tie)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                // Lovers, Lawyer & Pursuer save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                Pursuer.notAckedExiled = false;
                if (exiled != null) {
                    Lovers.notAckedExiledIsLover = ((Lovers.lover1 != null && Lovers.lover1.PlayerId == exiled.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == exiled.PlayerId));
                    Pursuer.notAckedExiled = (Pursuer.pursuer != null && Pursuer.pursuer.PlayerId == exiled.PlayerId) || (Lawyer.lawyer != null && Lawyer.target != null && Lawyer.target.PlayerId == exiled.PlayerId && Lawyer.target != Jester.jester);
                }

                Camouflager.camoComms = false;
            }
        }

        static void swapperOnClick(int i, MeetingHud __instance) {
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (__instance.playerStates[i].AmDead) return;

            int selectedCount = selections.Where(b => b).Count();
            SpriteRenderer renderer = renderers[i];

            if (selectedCount == 0) {
                renderer.color = Color.yellow;
                selections[i] = true;
            } else if (selectedCount == 1) {
                if (selections[i]) {
                    renderer.color = Color.red;
                    selections[i] = false;
                } else {
                    selections[i] = true;
                    renderer.color = Color.yellow;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.yellow, "Confirm Swap");
                }
            } else if (selectedCount == 2) {
                if (selections[i]) {
                    renderer.color = Color.red;
                    selections[i] = false;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");
                }
            }
        }

        static void swapperConfirm(MeetingHud __instance) {
            __instance.playerStates[0].Cancel();  // This will stop the underlying buttons of the template from showing up
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (selections.Where(b => b).Count() != 2) return;
            if (Swapper.playerId1 != Byte.MaxValue) return;

            PlayerVoteArea firstPlayer = null;
            PlayerVoteArea secondPlayer = null;
            for (int A = 0; A < selections.Length; A++) {
                if (selections[A]) {
                    if (firstPlayer == null) {
                        firstPlayer = __instance.playerStates[A];
                    } else {
                        secondPlayer = __instance.playerStates[A];
                    }
                    renderers[A].color = Color.green;
                } else if (renderers[A] != null) {
                    renderers[A].color = Color.gray;
                    }
                if (swapperButtonList[A] != null) swapperButtonList[A].OnClick.RemoveAllListeners();  // Swap buttons can't be clicked / changed anymore
            }
            if (firstPlayer != null && secondPlayer != null) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwapperSwap, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)firstPlayer.TargetPlayerId);
                writer.Write((byte)secondPlayer.TargetPlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                RPCProcedure.swapperSwap((byte)firstPlayer.TargetPlayerId, (byte)secondPlayer.TargetPlayerId);
                meetingExtraButtonLabel.text = Helpers.cs(Color.green, "Swapping!");
            }
        }

        public static void swapperCheckAndReturnSwap(MeetingHud __instance, byte dyingPlayerId) {
            // someone was guessed or dced in the meeting, check if this affects the swapper.
            if (Swapper.swapper == null || __instance.state == MeetingHud.VoteStates.Results) return;

            // reset swap.
            bool reset = false;
            if (dyingPlayerId == Swapper.playerId1 || dyingPlayerId == Swapper.playerId2) {
                reset = true;
                Swapper.playerId1 = Swapper.playerId2 = byte.MaxValue;
            }
            

            // Only for the swapper: Reset all the buttons and charges value to their original state.
            if (PlayerControl.LocalPlayer != Swapper.swapper) return;


            // check if dying player was a selected player (but not confirmed yet)
            for (int i = 0; i < __instance.playerStates.Count; i++) {
                reset = reset || selections[i] && __instance.playerStates[i].TargetPlayerId == dyingPlayerId;
                if (reset) break;
            }

            if (!reset) return;


            for (int i = 0; i < selections.Length; i++) {
                selections[i] = false;
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId && Swapper.canOnlySwapOthers)) continue;
                renderers[i].color = Color.red;
                int copyI = i;
                swapperButtonList[i].OnClick.RemoveAllListeners();
                swapperButtonList[i].OnClick.AddListener((System.Action)(() => swapperOnClick(copyI, __instance)));
            }
            meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");

        }

        static void mayorReveal(int i, MeetingHud __instance) {
            __instance.playerStates[0].Cancel();  // This will stop the underlying buttons of the template from showing up
            if (__instance.state == MeetingHud.VoteStates.Results) return;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MayorReveal, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.mayorReveal();
        }

        public static GameObject guesserUI;
        public static PassiveButton guesserUIExitButton;
        public static byte guesserCurrentTarget;
        static void guesserOnClick(int buttonTarget, MeetingHud __instance) {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted)) return;
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            Transform PhoneUI = UnityEngine.Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI");
            Transform container = UnityEngine.Object.Instantiate(PhoneUI, __instance.transform);
            container.transform.localPosition = new Vector3(0, 0, -5f);
            guesserUI = container.gameObject;

            int i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;

            guesserCurrentTarget = __instance.playerStates[buttonTarget].TargetPlayerId;

            Transform exitButtonParent = (new GameObject()).transform;
            exitButtonParent.SetParent(container);
            Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
            exitButtonParent.transform.localScale = new Vector3(0.217f, 0.9f, 1);
            guesserUIExitButton = exitButton.GetComponent<PassiveButton>();
            guesserUIExitButton.OnClick.RemoveAllListeners();
            guesserUIExitButton.OnClick.AddListener((System.Action)(() => {
                __instance.playerStates.ToList().ForEach(x => {
                    x.gameObject.SetActive(true);
                    if (PlayerControl.LocalPlayer.Data.IsDead && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                });
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            List<Transform> buttons = new List<Transform>();
            Transform selectedButton = null;

            foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos) {
                RoleId guesserRole = (Guesser.niceGuesser != null && PlayerControl.LocalPlayer.PlayerId == Guesser.niceGuesser.PlayerId) ? RoleId.Vigilante :  RoleId.Assassin;
                if (roleInfo.factionId == FactionId.Modifier || roleInfo.roleId == guesserRole) continue; // Not guessable roles & modifier
                if (HandleGuesser.isGuesserGm && (roleInfo.roleId == RoleId.Vigilante || roleInfo.roleId == RoleId.Assassin)) continue; // remove Guesser for guesser game mode
                
                // remove all roles that cannot spawn due to the settings from the ui.
                RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
                if (roleData.neutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.neutralSettings[(byte)roleInfo.roleId] == 0) continue;
                else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId] == 0) continue;
                else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId] == 0) continue;
                else if (roleData.kneutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.kneutralSettings[(byte)roleInfo.roleId] == 0) continue;

                if (roleInfo.roleId == RoleId.Vampire && CustomOptionHolder.draculaSpawnRate.getSelection() == 0) continue;
                if (roleInfo.roleId == RoleId.Pursuer && CustomOptionHolder.lawyerSpawnRate.getSelection() == 0 && CustomOptionHolder.executionerSpawnRate.getSelection() == 0) continue;
                if (roleInfo.roleId == RoleId.Survivor && CustomOptionHolder.guardianAngelSpawnRate.getSelection() == 0) continue;
                if (roleInfo.roleId == RoleId.FallenAngel && CustomOptionHolder.guardianAngelSpawnRate.getSelection() == 0) continue;
                if (roleInfo.roleId == RoleId.Doomsayer && Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer) continue;
                if (roleInfo.roleId == RoleId.VampireHunter && CustomOptionHolder.draculaSpawnRate.getSelection() == 0) continue;
                if (roleInfo.roleId == RoleId.VHVeteran) continue;
                
                if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer && Doomsayer.observedPlayers.Contains(Helpers.playerById(guesserCurrentTarget))) {
                    if (roleInfo.observeResults != RoleInfo.getRoleInfoForPlayer(Helpers.playerById(guesserCurrentTarget), false).FirstOrDefault().observeResults) continue;
                }
                
                if (Snitch.snitch != null && HandleGuesser.guesserCantGuessSnitch) {
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    if (numberOfLeftTasks <= 0 && roleInfo.roleId == RoleId.Snitch) continue;
                }

                if (Mayor.mayor != null && Mayor.isRevealed && roleInfo.roleId == RoleId.Mayor) continue;

                Transform buttonParent = (new GameObject()).transform;
                buttonParent.SetParent(container);
                Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
                Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
                TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                button.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
                buttons.Add(button);
                int row = i/5, col = i%5;
                buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = Helpers.cs(roleInfo.color, roleInfo.name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.7f;
                int copiedIndex = i;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                if (!PlayerControl.LocalPlayer.Data.IsDead && !Helpers.playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId).Data.IsDead) button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() => {
                    if (selectedButton != button) {
                        selectedButton = button;
                        buttons.ForEach(x => x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                    } else {
                        PlayerControl focusedTarget = Helpers.playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId);
                        if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null || HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) <= 0 ) return;

                        if (!HandleGuesser.killsThroughShield && focusedTarget == Medic.shielded) { // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true)); 
                            UnityEngine.Object.Destroy(container.gameObject);

                            MessageWriter murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                            RPCProcedure.shieldedMurderAttempt();
                            return;
                        }

                        if (Indomitable.indomitable != null && focusedTarget == Indomitable.indomitable) {
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true)); 
                            UnityEngine.Object.Destroy(container.gameObject);

                            MessageWriter murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShowIndomitableFlash, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                            RPCProcedure.showIndomitableFlash();
                            return;
                        }

                        var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(focusedTarget, false).FirstOrDefault();
                        if (mainRoleInfo == null) return;

                        PlayerControl dyingTarget = (mainRoleInfo == roleInfo) ? focusedTarget : PlayerControl.LocalPlayer;
                        if (mainRoleInfo == RoleInfo.vhVeteran && roleInfo == RoleInfo.veteran) {
                            dyingTarget = focusedTarget;
                        }
                        if (DoubleShot.doubleShot != null && DoubleShot.doubleShot == PlayerControl.LocalPlayer && !DoubleShot.lifeUsed && dyingTarget == PlayerControl.LocalPlayer) {
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true)); 
                            UnityEngine.Object.Destroy(container.gameObject);
                            __instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == focusedTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                            dyingTarget = null;
                            DoubleShot.lifeUsed = true;
                            Helpers.showFlash(Palette.ImpostorRed, message: "EXTRA LIFE IS USED");
                            return;
                        }

                        if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == PlayerControl.LocalPlayer && dyingTarget == PlayerControl.LocalPlayer) {
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true)); 
                            UnityEngine.Object.Destroy(container.gameObject);
                            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                            dyingTarget = null;
                            Helpers.showFlash(Palette.ImpostorRed, message: "WRONG GUESS");
                            return;
                        }

                        // Reset the GUI
                        __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                        UnityEngine.Object.Destroy(container.gameObject);
                        if (HandleGuesser.hasMultipleShotsPerMeeting && HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 1 && dyingTarget != PlayerControl.LocalPlayer)
                            __instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                        else
                            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                        // Shoot player and send chat info if activated
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuesserShoot, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(dyingTarget.PlayerId);
                        writer.Write(focusedTarget.PlayerId);
                        writer.Write((byte)roleInfo.roleId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.guesserShoot(PlayerControl.LocalPlayer.PlayerId, dyingTarget.PlayerId, focusedTarget.PlayerId, (byte)roleInfo.roleId);
                    }
                }));

                i++;
            }
            container.transform.localScale *= 0.75f;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        class PlayerVoteAreaSelectPatch {
            static bool Prefix(MeetingHud __instance) {
                return !(PlayerControl.LocalPlayer != null && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && guesserUI != null);
            }
        }

        static void populateButtonsPostfix(MeetingHud __instance) {
            // Add Guesser Buttons
            bool isGuesser = HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId);
            int remainingShots = HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId);
            if (isGuesser && !PlayerControl.LocalPlayer.Data.IsDead && remainingShots > 0) {
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (Executioner.executioner != null && Executioner.target != null && PlayerControl.LocalPlayer == Executioner.executioner && playerVoteArea.TargetPlayerId == Executioner.target.PlayerId) continue;
                    if (Lawyer.lawyer != null && Lawyer.target != null && PlayerControl.LocalPlayer == Lawyer.lawyer && playerVoteArea.TargetPlayerId == Lawyer.target.PlayerId) continue;
                    if (GuardianAngel.guardianAngel != null && GuardianAngel.target != null && PlayerControl.LocalPlayer == GuardianAngel.guardianAngel && playerVoteArea.TargetPlayerId == GuardianAngel.target.PlayerId) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = HandleGuesser.getTargetSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => guesserOnClick(copiedIndex, __instance)));
                }
            }

            // Add Swapper Buttons
            bool addSwapperButtons = Swapper.swapper != null && PlayerControl.LocalPlayer == Swapper.swapper && !Swapper.swapper.Data.IsDead;
            if (addSwapperButtons) {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];
                swapperButtonList = new PassiveButton[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId && Swapper.canOnlySwapOthers)) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject checkbox = UnityEngine.Object.Instantiate(template);
                    checkbox.transform.SetParent(playerVoteArea.transform);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId)) checkbox.transform.localPosition = new Vector3(-0.5f, 0.03f, -1.3f);
                    SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.getCheckSprite();
                    renderer.color = Color.red;

                    PassiveButton button = checkbox.GetComponent<PassiveButton>();
                    swapperButtonList[i] = button;
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => swapperOnClick(copiedIndex, __instance)));
                    
                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }

            // Add meeting extra button, i.e. Swapper Confirm Button. Swapper Button uses ExtraButtonText on the Left of the Button. (Future meeting buttons can easily be added here)
            if (addSwapperButtons) {
                Transform meetingUI = UnityEngine.Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI");

                var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
                var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
                var textTemplate = __instance.playerStates[0].NameText;
                Transform meetingExtraButtonParent = (new GameObject()).transform;
                meetingExtraButtonParent.SetParent(meetingUI);
                Transform meetingExtraButton = UnityEngine.Object.Instantiate(buttonTemplate, meetingExtraButtonParent);

                Transform infoTransform = __instance.playerStates[0].NameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro meetingInfo = infoTransform != null ? infoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                meetingExtraButtonText = UnityEngine.Object.Instantiate(__instance.playerStates[0].NameText, meetingExtraButtonParent);
                meetingExtraButtonText.text = "";
                meetingExtraButtonText.enableWordWrapping = false;
                meetingExtraButtonText.transform.localScale = Vector3.one * 1.7f;
                meetingExtraButtonText.transform.localPosition = new Vector3(-2.5f, 0f, 0f);

                Transform meetingExtraButtonMask = UnityEngine.Object.Instantiate(maskTemplate, meetingExtraButtonParent);
                meetingExtraButtonLabel = UnityEngine.Object.Instantiate(textTemplate, meetingExtraButton);
                meetingExtraButton.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;

                meetingExtraButtonParent.localPosition = new Vector3(0, -2.225f, -5);
                meetingExtraButtonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                meetingExtraButtonLabel.alignment = TMPro.TextAlignmentOptions.Center;
                meetingExtraButtonLabel.transform.localPosition = new Vector3(0, 0, meetingExtraButtonLabel.transform.localPosition.z);
                if (addSwapperButtons) {
                    meetingExtraButtonLabel.transform.localScale *= 1.7f;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");
                }
                PassiveButton passiveButton = meetingExtraButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                if (!PlayerControl.LocalPlayer.Data.IsDead) {
                    if (addSwapperButtons)
                        passiveButton.OnClick.AddListener((Action)(() => swapperConfirm(__instance)));
                }
                meetingExtraButton.parent.gameObject.SetActive(false);
                __instance.StartCoroutine(Effects.Lerp(7.27f, new Action<float>((p) => { // Button appears delayed, so that its visible in the voting screen only!
                    if (p == 1f) {
                        meetingExtraButton.parent.gameObject.SetActive(true);
                    }
                })));
            }

            // Add Mayor Buttons
            bool addMayorButton = Mayor.mayor != null && Mayor.mayor == PlayerControl.LocalPlayer && !Mayor.isRevealed;
            if (addMayorButton && !PlayerControl.LocalPlayer.Data.IsDead) {
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId != PlayerControl.LocalPlayer.PlayerId) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "RevealButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId)) targetBox.transform.localPosition = new Vector3(-0.5f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Mayor.getCheckSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => mayorReveal(copiedIndex, __instance)));
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch {
            static void Postfix(MeetingHud __instance)
            {
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]MessageReader reader, [HarmonyArgument(1)]bool initialState)
            {
                // Add swapper buttons
                if (initialState) {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        class StartMeetingPatch {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo meetingTarget) {
                // Save the meeting target
                target = meetingTarget;
                // Resett Bait list
                Bait.active = new Dictionary<DeadPlayer, float>();
                // Reset Poisoner poisoned
                Poisoner.poisoned = null;

                // Reset zoomed out ghosts
                Helpers.toggleZoom(reset: true);

                // Close In-Game Settings Display if open
                HudManagerUpdate.CloseSettings();

                // Notify blackmailed player
                if (Blackmailer.blackmailed == PlayerControl.LocalPlayer) { 
                    Coroutines.Start(Helpers.BlackmailShhh());
                }

                /* Add trapped Info */
                
                // into Trapper chat
                if (Trapper.trapper != null && !Trapper.trapper.Data.IsDead && Trapper.trapper == PlayerControl.LocalPlayer) {
                    string msg = "";
                    if (Trapper.trappedPlayers.Count == 0) msg = "No players entered any of your traps";
                    else if (Trapper.trappedPlayers.Count < Trapper.minPlayersInTrap) msg = "Not enough players triggered your traps";
                    else {
                        int evilPlayers = 0;
                        foreach (PlayerControl trapped in Trapper.trappedPlayers) {
                            if (trapped.Data.Role.IsImpostor || trapped.isNeutral() && Trapper.neutShowsEvil || trapped.isNeutralKiller() && Trapper.kneutShowsEvil)
                                evilPlayers++;
                        }
                        msg = $"{evilPlayers} evil roles was caught in your traps";
                    }

                    if (!string.IsNullOrWhiteSpace(msg)) {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);
                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }

                // into Trapped chat
                if (Trapper.trapper != null && PlayerControl.LocalPlayer != Trapper.trapper) {
                    string msg = "";
                    if (Trapper.trappedPlayers.Contains(PlayerControl.LocalPlayer)) {
                        msg = "You have been caught in trap";
                    }

                    if (!string.IsNullOrWhiteSpace(msg)) {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);
                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }

                // Add examined Info into Detective chat
                if (Detective.detective != null && !Detective.detective.Data.IsDead && Detective.detective == PlayerControl.LocalPlayer && Detective.examined != null && !Detective.examined.Data.IsDead) {
                    string msg = "Detective Examine Report Info:\n\n";
                    foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos.Where(x => x.observeResults == RoleInfo.getRoleInfoForPlayer(Detective.examined, false).FirstOrDefault().observeResults).OrderBy(x => Guid.NewGuid())) {
                        if (roleInfo == RoleInfo.vhVeteran) roleInfo.name = "";
                        msg += $"{roleInfo.name} ";
                    }

                    if (!string.IsNullOrWhiteSpace(msg)) {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);
                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }

                // Add confessor Info into Oracle chat
                if (Oracle.oracle != null && !Oracle.oracle.Data.IsDead && Oracle.confessor != null && Oracle.oracle == PlayerControl.LocalPlayer) {
                    string msg = "Oracle Confess Info:\n\n";
                    
                    if (Oracle.confessor.Data.IsDead || Oracle.confessor.Data.Disconnected) {
                        msg += "Your confessor failed to survive so you received no confession";
                    } else {
                        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && x != PlayerControl.LocalPlayer && x != Oracle.confessor).ToList();
                        if (allPlayers.Count < 2) msg += "Too few people alive to receive a confessional";
                        var evilPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && !x.Data.Disconnected && (x.Data.Role.IsImpostor || (x.isNeutral() && Oracle.neutShowsEvil) || (x.isNeutralKiller() && Oracle.kneutShowsEvil))).ToList();
                        if (evilPlayers.Count == 0) msg += $"{Oracle.confessor.Data.DefaultOutfit.PlayerName} confesses to knowing that there are no more evil players!";
                        allPlayers.Shuffle();
                        evilPlayers.Shuffle();
                        var secondPlayer = allPlayers[0];
                        var firstTwoEvil = false;
                        foreach (var evilPlayer in evilPlayers)
                            if (evilPlayer == Oracle.confessor || evilPlayer == secondPlayer)
                                firstTwoEvil = true;
                        
                        if (firstTwoEvil) {
                            var thirdPlayer = allPlayers[1];
                            msg += $"{Oracle.confessor.Data.DefaultOutfit.PlayerName} confesses to knowing that they, {secondPlayer.Data.DefaultOutfit.PlayerName} and/or {thirdPlayer.Data.DefaultOutfit.PlayerName} is evil!";
                        } else {
                            var thirdPlayer = evilPlayers[0];
                            msg +=  $"{Oracle.confessor.Data.DefaultOutfit.PlayerName} confesses to knowing that they, {secondPlayer.Data.DefaultOutfit.PlayerName} and/or {thirdPlayer.Data.DefaultOutfit.PlayerName} is evil!";
                        }
                    }
                    
                    if (!string.IsNullOrWhiteSpace(msg)) {
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);
                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.ChatInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch {
            public static Sprite Overlay => Blackmailer.getBlackmailOverlaySprite();
            static void Postfix(MeetingHud __instance) {
                if (target == null && TOUMapOptions.blockSkippingInEmergencyMeetings == 1) {
                    __instance.SkipVoteButton.gameObject.SetActive(false);
                } else if (TOUMapOptions.blockSkippingInEmergencyMeetings == 2) {
                    __instance.SkipVoteButton.gameObject.SetActive(false);
                }

                // Remove first kill shield
                if (__instance.state >= MeetingHud.VoteStates.Discussion)
                    TOUMapOptions.firstKillPlayer = null;

                // Show Blackmailed overlay
                if (Blackmailer.blackmailed != null) {
                    var playerState = __instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == Blackmailer.blackmailed.PlayerId);
                    playerState.Overlay.gameObject.SetActive(true);
                    playerState.Overlay.sprite = Overlay;
                    if (__instance.state != MeetingHud.VoteStates.Animating && shookAlready == false)
                    {
                        shookAlready = true;
                        __instance.StartCoroutine(Effects.SwayX(playerState.transform));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class MeetingHudStart
        {
            public static void Postfix(MeetingHud __instance)
            {
                shookAlready = false;
                if (Blackmailer.blackmailed == null) return;
                if (Blackmailer.blackmailed.Data.PlayerId == PlayerControl.LocalPlayer.PlayerId && !Blackmailer.blackmailed.Data.IsDead)
                {
                    if (Blackmailer.blackmailed == PlayerControl.LocalPlayer) {
                        Coroutines.Start(Helpers.BlackmailShhh());
                    }
                }
            }
        }

        [HarmonyPatch]
        public class ShowHost {
            private static TextMeshPro Text = null;
            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
            [HarmonyPostfix]

            public static void Setup(MeetingHud __instance)
            {
                if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;

                __instance.ProceedButton.gameObject.transform.localPosition = new(-2.5f, 2.2f, 0);
                __instance.ProceedButton.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                __instance.ProceedButton.GetComponent<PassiveButton>().enabled = false;
                __instance.HostIcon.gameObject.SetActive(true);
                __instance.ProceedButton.gameObject.SetActive(true);
            }

            [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
            [HarmonyPostfix]
            public static void Postfix(MeetingHud __instance) {
                var host = GameData.Instance.GetHost();
                if (host != null)
                {
                    PlayerMaterial.SetColors(host.DefaultOutfit.ColorId, __instance.HostIcon);
                    if (Text == null) Text = __instance.ProceedButton.gameObject.GetComponentInChildren<TextMeshPro>();
                    Text.text = $"host: {host.PlayerName}";
                }
            }
        }
    }
}
