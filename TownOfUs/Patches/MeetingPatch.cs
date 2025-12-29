using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Assets.CoreScripts;
using AmongUs.QuickChat;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    class MeetingHudPatch
    {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static NetworkedPlayerInfo target = null;
        private const float scale = 0.65f;
        private static TMPro.TextMeshPro meetingExtraButtonText;
        private static PassiveButton[] swapperButtonList;
        private static TMPro.TextMeshPro meetingExtraButtonLabel;
        private static PlayerVoteArea swapped1 = null;
        private static PlayerVoteArea swapped2 = null;
        static bool IsBlockedBlackmail() => Blackmailer.players.Any(x => x.player && x.blackmailed == PlayerControl.LocalPlayer) && Blackmailer.blockTargetAbility;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch
        {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
            {
                Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254)
                    {
                        PlayerControl player = Helpers.playerById((byte)playerVoteArea.TargetPlayerId);
                        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;
                        if (Blackmailer.players.Any(x => x.player && x.blackmailed == player) && Blackmailer.blockTargetVote) continue;

                        int currentVotes;
                        int additionalVotes = 1;

                        if (player.isRole(RoleId.Mayor)) additionalVotes = 3;
                        if (dictionary.TryGetValue(playerVoteArea.VotedFor, out currentVotes))
                            dictionary[playerVoteArea.VotedFor] = currentVotes + additionalVotes;
                        else
                            dictionary[playerVoteArea.VotedFor] = additionalVotes;
                    }
                }

                if (Swapper.hasAlivePlayers)
                {
                    swapped1 = null;
                    swapped2 = null;
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                    }

                    if (swapped1 != null && swapped2 != null) {
                        if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                        if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                        (dictionary[swapped2.TargetPlayerId], dictionary[swapped1.TargetPlayerId]) = (dictionary[swapped1.TargetPlayerId], dictionary[swapped2.TargetPlayerId]);
                    }
                }
                return dictionary;
            }


            static bool Prefix(MeetingHud __instance)
            {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote || (Blackmailer.players.Any(x => x.player && x.blackmailed && x.blackmailed.PlayerId == ps.TargetPlayerId) && Blackmailer.blockTargetVote)))
                {
                    Dictionary<byte, int> self = CalculateVotes(__instance);
                    bool tie;
                    KeyValuePair<byte, int> max = self.MaxPair(out tie);
                    NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                    // TieBreaker 
                    List<NetworkedPlayerInfo> potentialExiled = new List<NetworkedPlayerInfo>();
                    bool skipIsTie = false;
                    if (self.Count > 0)
                    {
                        Tiebreaker.isTiebreak = false;
                        int maxVoteValue = self.Values.Max();
                        PlayerVoteArea tb = null;
                        if (Tiebreaker.exists)
                            tb = __instance.playerStates.ToArray().FirstOrDefault(x => Helpers.playerById(x.TargetPlayerId).hasModifier(RoleId.Tiebreaker));
                        bool isTiebreakerSkip = tb == null || tb.VotedFor == 253;
                        if (tb != null && tb.AmDead) isTiebreakerSkip = true;

                        foreach (KeyValuePair<byte, int> pair in self)
                        {
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
                        array[i] = new MeetingHud.VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };

                        if (!Tiebreaker.exists || !Helpers.playerById(playerVoteArea.TargetPlayerId).hasModifier(RoleId.Tiebreaker)) continue;

                        byte tiebreakerVote = playerVoteArea.VotedFor;
                        if (swapped1 != null && swapped2 != null)
                        {
                            if (tiebreakerVote == swapped1.TargetPlayerId) tiebreakerVote = swapped2.TargetPlayerId;
                            else if (tiebreakerVote == swapped2.TargetPlayerId) tiebreakerVote = swapped1.TargetPlayerId;
                        }

                        if (potentialExiled.FindAll(x => x != null && x.PlayerId == tiebreakerVote).Count > 0 && (potentialExiled.Count > 1 || skipIsTie))
                        {
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

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
        class MeetingHudSelectPatch
        {
            public static bool Prefix(ref bool __result, MeetingHud __instance, [HarmonyArgument(0)] int suspectStateIdx)
            {
                if (Blackmailer.players.Any(x => x.player && PlayerControl.LocalPlayer == x.blackmailed) && Blackmailer.blockTargetVote) {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
        class SetHighlightedPatch
        {
            public static bool Prefix(PlayerVoteArea __instance, bool value)
            {
                if (!__instance.HighlightedFX) return false;
                __instance.HighlightedFX.enabled = value && __instance.canBeHighlighted() && !(Blackmailer.players.Any(x => x.player && PlayerControl.LocalPlayer == x.blackmailed) && Blackmailer.blockTargetVote);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.GetVotesRemaining))]
        class MeetingHudGetVotesRemainingPatch
        {
            public static bool Prefix(MeetingHud __instance, ref int __result)
            {
                try
                {
                    __result = __instance.playerStates.Count(ps => !ps.AmDead && !ps.DidVote && !(Blackmailer.players.Any(x => x.player && x.blackmailed && x.blackmailed.PlayerId == ps.TargetPlayerId) && Blackmailer.blockTargetVote));
                }
                catch
                {
                    __result = 0;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        class MeetingHudBloopAVoteIconPatch
        {
            public static bool Prefix(MeetingHud __instance, NetworkedPlayerInfo voterPlayer, int index, Transform parent)
            {
                var spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                var showVoteColors = !GameManager.Instance.LogicOptions.GetAnonymousVotes() || (PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeVotes);

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
        class MeetingHudPopulateVotesPatch
        {
            private static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
            {
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }
                bool doSwap = swapped1 != null && swapped2 != null && Swapper.hasAlivePlayers;

                if (doSwap) {
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
                }

                __instance.TitleText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte targetPlayerId = playerVoteArea.TargetPlayerId;
                    if (doSwap && playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId) playerVoteArea = swapped2;
                    else if (doSwap && playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId) playerVoteArea = swapped1;

                    playerVoteArea.ClearForResults();

                    int num2 = 0;
                    Dictionary<int, int> votesApplied = new();
                    for (int j = 0; j < states.Length; j++)
                    {
                        MeetingHud.VoterState voterState = states[j];
                        NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null)
                        {
                            Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                        }
                        else if (Blackmailer.players.Any(x => x.player && x.blackmailed && voterState.VoterId == x.blackmailed.PlayerId) && Blackmailer.blockTargetVote)
                            continue;
                        else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        if (!votesApplied.ContainsKey(voterState.VoterId))
                            votesApplied[voterState.VoterId] = 0;
                        votesApplied[voterState.VoterId]++;
                        if (Mayor.exists && Mayor.allPlayers.Any(x => x.PlayerId == voterState.VoterId) && votesApplied[voterState.VoterId] < 3) {
                            j--;
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch
        {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte[] states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                foreach (var pursuer in Pursuer.players)
                    pursuer.notAckedExiled = false;
                foreach (var couple in Lovers.couples)
                    couple.notAckedExiledIsLover = false;
                if (exiled != null)
                {
                    foreach (var pursuer in Pursuer.players)
                        pursuer.notAckedExiled = pursuer.player != null && pursuer.player.PlayerId == exiled.PlayerId;
                    
                    var couple = Lovers.getCouple(exiled.Object);
                    if (couple != null) couple.notAckedExiledIsLover = true;
                }
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
            if (!Swapper.exists || __instance.state == MeetingHud.VoteStates.Results) return;

            // reset swap.
            bool reset = false;
            if (dyingPlayerId == Swapper.playerId1 || dyingPlayerId == Swapper.playerId2) {
                reset = true;
                Swapper.playerId1 = Swapper.playerId2 = byte.MaxValue;
            }

            // Only for the swapper: Reset all the buttons and charges value to their original state.
            if (!PlayerControl.LocalPlayer.isRole(RoleId.Swapper)) return;

            // check if dying player was a selected player (but not confirmed yet)
            for (int i = 0; i < __instance.playerStates.Count; i++) {
                reset = reset || selections[i] && __instance.playerStates[i].TargetPlayerId == dyingPlayerId;
                if (reset) break;
            }

            if (!reset) return;

            for (int i = 0; i < selections.Length; i++) {
                selections[i] = false;
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId && Swapper.canOnlySwapOthers)) continue;
                renderers[i].color = Color.red;
                int copyI = i;
                swapperButtonList[i].OnClick.RemoveAllListeners();
                swapperButtonList[i].OnClick.AddListener((System.Action)(() => swapperOnClick(copyI, __instance)));
            }
            meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");

        }
        
        public static GameObject guesserUI;
        public static PassiveButton guesserUIExitButton;
        public static byte guesserCurrentTarget;
        public const int MaxOneScreenRole = 40;
        private static List<Transform> RoleButtons;
        private static List<SpriteRenderer> PageButtons;
        public static int Page;
        static void guesserSelectRole(bool SetPage = true)
        {
            if (SetPage) Page = 1;
            foreach (var RoleButton in RoleButtons)
            {
                int index = 0;
                foreach (var RoleBtn in RoleButtons)
                {
                    if (RoleBtn == null) continue;
                    index++;
                    if (index <= (Page - 1) * MaxOneScreenRole) { RoleBtn.gameObject.SetActive(false); continue; }
                    if ((Page * MaxOneScreenRole) < index) { RoleBtn.gameObject.SetActive(false); continue; }
                    RoleBtn.gameObject.SetActive(true);
                }
            }
        }

        static void guesserOnClick(int buttonTarget, MeetingHud __instance)
        {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted)) return;
            Page = 1;
            RoleButtons = new();
            PageButtons = new();
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

            Transform exitButtonParent = new GameObject().transform;
            exitButtonParent.SetParent(container);
            Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
            exitButtonParent.transform.localScale = new Vector3(0.217f, 0.9f, 1);
            guesserUIExitButton = exitButton.GetComponent<PassiveButton>();
            guesserUIExitButton.OnClick.RemoveAllListeners();
            guesserUIExitButton.OnClick.AddListener((System.Action)(() =>
            {
                __instance.playerStates.ToList().ForEach(x =>
                {
                    x.gameObject.SetActive(true);
                    if (PlayerControl.LocalPlayer.Data.IsDead && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                });
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            static void ReloadPage()
            {
                PageButtons[0].gameObject.SetActive(true);
                PageButtons[1].gameObject.SetActive(true);
                if (((RoleButtons.Count / MaxOneScreenRole) +
                    (RoleButtons.Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page)
                {
                    Page -= 1;
                    PageButtons[1].gameObject.SetActive(false);
                }
                else if (((RoleButtons.Count / MaxOneScreenRole) +
                    (RoleButtons.Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page + 1)
                {
                    PageButtons[1].gameObject.SetActive(false);
                }
                if (Page <= 1)
                {
                    Page = 1;
                    PageButtons[0].gameObject.SetActive(false);
                }
                guesserSelectRole(false);
            }

            void CreatePage(bool IsNext, MeetingHud __instance, Transform container)
            {
                var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
                var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
                var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
                var textTemplate = __instance.playerStates[0].NameText;
                Transform PagebuttonParent = new GameObject().transform;
                PagebuttonParent.SetParent(container);
                Transform Pagebutton = UnityEngine.Object.Instantiate(buttonTemplate, PagebuttonParent);
                Pagebutton.FindChild("ControllerHighlight").gameObject.SetActive(false);
                Transform PagebuttonMask = UnityEngine.Object.Instantiate(maskTemplate, PagebuttonParent);
                TextMeshPro Pagelabel = UnityEngine.Object.Instantiate(textTemplate, Pagebutton);
                Pagebutton.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
                PagebuttonParent.localPosition = IsNext ? new(3.535f, -2.2f, -200) : new(-3.475f, -2.2f, -200);
                PagebuttonParent.localScale = new(0.55f, 0.55f, 1f);
                Pagelabel.color = Color.white;
                Pagelabel.text = IsNext ? "Next" : "Previous";
                Pagelabel.alignment = TextAlignmentOptions.Center;
                Pagelabel.transform.localPosition = new Vector3(0, 0, Pagelabel.transform.localPosition.z);
                Pagelabel.transform.localScale *= 1.6f;
                Pagelabel.autoSizeTextContainer = true;
                Pagebutton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    if (IsNext) Page += 1;
                    else Page -= 1;
                    ReloadPage();
                }));
                PageButtons.Add(Pagebutton.GetComponent<SpriteRenderer>());
            }

            CreatePage(false, __instance, container);
            CreatePage(true, __instance, container);

            Transform selectedButton = null;

            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos)
            {
                if (roleInfo.factionId == FactionId.Modifier) continue;
                if (roleInfo.roleId == RoleId.Pestilence) continue;
                if (roleInfo.roleId == RoleId.Mayor) continue;
                if (HandleGuesser.isGuesserGm && PlayerControl.LocalPlayer.Data.Role.IsImpostor && !HandleGuesser.evilGuesserCanGuessAgent && roleInfo.roleId == RoleId.Agent) continue;

                // remove all roles that cannot spawn due to the settings from the ui.
                if (roleData.nonKillingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.nonKillingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.killingNeutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.killingNeutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0) continue;
                else if (roleInfo.roleId == RoleId.Vampire && (!CustomOptionHolder.draculaCanCreateVampire.getBool() || CustomOptionHolder.draculaSpawnRate.getSelection() == 0)) continue;
                else if (roleInfo.roleId == RoleId.VampireHunter && CustomOptionHolder.draculaSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Pursuer && CustomOptionHolder.lawyerSpawnRate.getSelection() == 0) continue;
                else if (roleInfo.roleId == RoleId.Survivor && CustomOptionHolder.guardianAngelSpawnRate.getSelection() == 0) continue;

                if (Snitch.exists && HandleGuesser.guesserCantGuessSnitch)
                {
                    if (Snitch.allPlayers.Any(x => TasksHandler.taskInfo(x.Data).Item2 - TasksHandler.taskInfo(x.Data).Item1 <= 0) && roleInfo.roleId == RoleId.Snitch) continue;
                }

                if (roleInfo.roleId == RoleId.Agent && GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors <= 1) continue;

                CreateRole(roleInfo);
            }

            void CreateRole(RoleInfo roleInfo)
            {
                if (roleInfo == null) TownOfUsPlugin.Logger.LogMessage("RoleInfo is null while initializing!");
                if (i >= MaxOneScreenRole) i = 0;
                Transform buttonParent = new GameObject().transform;
                buttonParent.SetParent(container);
                Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
                Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
                TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                button.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
                RoleButtons.Add(button);
                int row = i / 5, col = i % 5;
                buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = Helpers.cs(roleInfo.color, roleInfo.name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.7f;
                int copiedIndex = i;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                if (!PlayerControl.LocalPlayer.Data.IsDead && Helpers.playerById(__instance.playerStates[buttonTarget].TargetPlayerId) != null
                    && !Helpers.playerById(__instance.playerStates[buttonTarget].TargetPlayerId).Data.IsDead) button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() => {
                        if (selectedButton != button) {
                            selectedButton = button;
                            RoleButtons.ForEach(x => x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                        } else {
                            PlayerControl focusedTarget = Helpers.playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId);
                            if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null) return;

                            if (!HandleGuesser.killsThroughShield && Medic.isShielded(focusedTarget)) { // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);

                                foreach (var medic in Medic.GetMedic(focusedTarget)) {
                                    MessageWriter murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                                    murderAttemptWriter.Write(medic.player.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                                    RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);
                                }
                                return;
                            }

                            if (!HandleGuesser.killsThroughShield && Mercenary.isShielded(focusedTarget)) { // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);

                                foreach (var mercenary in Mercenary.GetMercenary(focusedTarget)) {
                                    MessageWriter murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MercenaryShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                                    murderAttemptWriter.Write(mercenary.shielded.PlayerId);
                                    murderAttemptWriter.Write(mercenary.player.PlayerId);
                                    murderAttemptWriter.Write(true);
                                    AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                                    RPCProcedure.mercenaryShieldedMurderAttempt(mercenary.shielded.PlayerId, mercenary.player.PlayerId, true);
                                }
                                return;
                            }

                            if (focusedTarget.hasModifier(RoleId.Indomitable)) // Shooting the Indomitable will not allow the guess, notifiy Indomitable about the kill attempt and close the window
                            {
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);
                                if (PlayerControl.LocalPlayer == focusedTarget)
                                {
                                    Helpers.showFlash(Indomitable.color, 1f, "Failed Murder Attempt");
                                }
                                return;
                            }

                            var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(focusedTarget, false).FirstOrDefault();
                            if (mainRoleInfo == null) return;

                            PlayerControl dyingTarget = (mainRoleInfo == roleInfo) ? focusedTarget : PlayerControl.LocalPlayer;

                            if (DoubleShot.players.Any(x => x.player == PlayerControl.LocalPlayer && !x.usedExtraLife && dyingTarget == x.player))
                            {
                                dyingTarget = null;
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);
                                __instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == focusedTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                                DoubleShot.players.DoIf(x => x.player == PlayerControl.LocalPlayer, x => x.usedExtraLife = true);
                                Helpers.showFlash(Palette.ImpostorRed);
                                return;
                            }

                            if (PlayerControl.LocalPlayer.isRole(RoleId.Doomsayer) && PlayerControl.LocalPlayer == dyingTarget)
                            {
                                dyingTarget = null;
                                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                                UnityEngine.Object.Destroy(container.gameObject);
                                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                                Helpers.showFlash(Doomsayer.color);
                                return;
                            }

                            // Reset the GUI
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                            UnityEngine.Object.Destroy(container.gameObject);
                            if (HandleGuesser.hasMultipleShotsPerMeeting)
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
            guesserSelectRole();
            ReloadPage();
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        class PlayerVoteAreaSelectPatch
        {
            static bool Prefix(MeetingHud __instance)
            {
                return !(PlayerControl.LocalPlayer != null && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && guesserUI != null);
            }
        }

        static void amnesiacRemember(int i, MeetingHud __instance)
        {
            __instance.playerStates[0].Cancel();  // This will stop the underlying buttons of the template from showing up
            if (__instance.state == MeetingHud.VoteStates.Results) return;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AmnesiacRemember, Hazel.SendOption.Reliable, -1);
            writer.Write(__instance.playerStates[i].TargetPlayerId);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.amnesiacRemember(__instance.playerStates[i].TargetPlayerId, PlayerControl.LocalPlayer.PlayerId);

            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("RememberMeetingButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("RememberMeetingButton").gameObject); });
        }

        static void populateButtonsPostfix(MeetingHud __instance)
        {
            // Add Swapper Buttons
            if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper) && !PlayerControl.LocalPlayer.Data.IsDead && !IsBlockedBlackmail())
            {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];
                swapperButtonList = new PassiveButton[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId && Swapper.canOnlySwapOthers)) continue;

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
            if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper) && !PlayerControl.LocalPlayer.Data.IsDead && !IsBlockedBlackmail())
            {
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
                if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    meetingExtraButtonLabel.transform.localScale *= 1.7f;
                    meetingExtraButtonLabel.text = Helpers.cs(Color.red, "Confirm Swap");
                }
                PassiveButton passiveButton = meetingExtraButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Swapper) && !PlayerControl.LocalPlayer.Data.IsDead)
                        passiveButton.OnClick.AddListener((Action)(() => swapperConfirm(__instance)));
                }
                meetingExtraButton.parent.gameObject.SetActive(false);
                __instance.StartCoroutine(Effects.Lerp(7.27f, new Action<float>((p) =>
                { // Button appears delayed, so that its visible in the voting screen only!
                    if (p == 1f)
                    {
                        meetingExtraButton.parent.gameObject.SetActive(true);
                    }
                })));
            }

            // Add Guesser Buttons
            bool isGuesser = HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId);
            int remainingShots = HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId);
            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (isGuesser && !PlayerControl.LocalPlayer.Data.IsDead && remainingShots > 0 && !IsBlockedBlackmail())
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (PlayerControl.LocalPlayer != null && !Helpers.isEvil(PlayerControl.LocalPlayer) && playerCompleted < HandleGuesser.tasksToUnlock) continue;
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

            // Add Amnesiac Buttons
            if (PlayerControl.LocalPlayer.isRole(RoleId.Amnesiac) && !PlayerControl.LocalPlayer.Data.IsDead && Amnesiac.rememberMeeting && HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId))
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (!playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "RememberMeetingButton";
                    targetBox.transform.localPosition = new Vector3(-0.5f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Amnesiac.getMeetingButtonSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((System.Action)(() => amnesiacRemember(copiedIndex, __instance)));
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch
        {
            static void Postfix(MeetingHud __instance)
            {
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch
        {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool initialState)
            {
                // Add swapper buttons
                if (initialState)
                {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        public static void startMeeting()
        {
            OnMeetingStart();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
        class OpenMeetingPatch
        {
            public static void Prefix(HudManager __instance)
            {
                startMeeting();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        class StartMeetingPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo meetingTarget)
            {
                startMeeting();
                // Count meetings
                if (meetingTarget == null) meetingsCount++;
                // Save the meeting target
                target = meetingTarget;
                // Reset zoomed out ghosts
                Helpers.toggleZoom(reset: true);
                // Close In-Game Settings Display if open
                HudManagerUpdate.CloseSettings();

                // Blackmail target
                if (Blackmailer.players.Any(x => x.player && x.blackmailed == PlayerControl.LocalPlayer))
                {
                    Coroutines.Start(Helpers.BlackmailShhh());
                }

                if (Detective.exists && PlayerControl.LocalPlayer.isRole(RoleId.Detective) && !PlayerControl.LocalPlayer.Data.IsDead && Detective.examined != null && !Detective.examined.Data.IsDead)
                {
                    string msg = Detective.GetInfo(Detective.examined);
                    
                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{msg}", false);
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
        class MeetingHudUpdatePatch
        {
            public static Sprite Overlay => Blackmailer.getBlackmailOverlaySprite();
            static void Postfix(MeetingHud __instance)
            {
                if (__instance.state >= MeetingHud.VoteStates.Discussion)
                {
                    // Remove first kill shield
                    firstKillPlayer = null;
                }

                if (Blackmailer.players.Any(x => x.player && x.blackmailed))
                {
                    // Blackmailer show overlay
                    var playerState = __instance.playerStates.FirstOrDefault(x => Blackmailer.players.Any(y => y.blackmailed && y.player && y.blackmailed.PlayerId == x.TargetPlayerId));
                    playerState.Overlay.gameObject.SetActive(true);
                    playerState.Overlay.sprite = Overlay;
                    if (__instance.state != MeetingHud.VoteStates.Animating && !Blackmailer.alreadyShook)
                    {
                        Blackmailer.alreadyShook = true;
                        __instance.StartCoroutine(Effects.SwayX(playerState.transform));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(QuickChatMenu), nameof(QuickChatMenu.Open))]
        public class BlockQuickChatAbility
        {
            public static bool Prefix(QuickChatMenu __instance)
            {
                if (Blackmailer.players.Any(x => x.player && PlayerControl.LocalPlayer == x.blackmailed)) {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
        public class BlockChatBlackmailed
        {
            public static bool Prefix(TextBoxTMP __instance)
            {
                if (Blackmailer.players.Any(x => x.player && PlayerControl.LocalPlayer == x.blackmailed)) {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch]
        public class ShowHost
        {
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
            public static void Postfix(MeetingHud __instance)
            {
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