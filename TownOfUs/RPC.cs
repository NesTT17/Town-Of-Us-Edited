using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
using UnityEngine;

namespace TownOfUs
{
    public enum FactionId
    {
        Crewmate, BenignNeutral, EvilNeutral, KillingNeutral, Impostor, Modifier
    }

    public enum CustomRPC
    {
        ResetVariables = 100,
        ShareOptions,
        SetRole,
        SetModifier,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,
        DynamicMapOption,
        SetGameStarting,
        ShareGamemode,
        StopStart,
        SetFirstKill,
        ShareGhostInfo,
        SystemSpreadPlayers,
        SystemCleanBody,

        EngineerFixLights = 120,
        EngineerUsedRepair,
        JesterWin,
        VeteranAlert,
        CamouflagerCamouflage,
        MorphlingMorph,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        LawyerWin,
        SetBlanked,
        PoliticianCampaign,
        PoliticianTurnMayor,
        MayorBodyguard,
        PlaguebearerInfect,
        PlaguebearerTurnPestilence,
        WerewolfRampage,
        DraculaCreatesVampire,
        VampirePromotes,
        SwapperSwap,
        VampireHunterPromotes,
        MedicSetShielded,
        ShieldedMurderAttempt,
        SetGuesserGm,
        GuesserShoot,
        GuardianAngelSetTarget,
        SurvivorSafeguard,
        GuardianAngelProtect,
        GuardianAngelPromotes,
        AmnesiacRemember,
        ExecutionerSetTarget,
        ExecutionerPromotesToAmnesiac,
        ExecutionerWin,
        PoisonerSetPoisoned,
        CleanBody,
        SwooperSwoop,
        SetFutureShifted,
        ShifterShift,
        OracleConfess,
        MercenarySetShielded,
        MercenaryShieldedMurderAttempt,
        MercenaryDonArmor,
        BlackmailPlayer,
        UnblackmailPlayer,
        GrenadierFlash,
        GlitchMimic,
        GlitchHack,
        DraftModePickOrder,
        DraftModePick,
        TransporterTransport,
        ThiefStealsRole,
        DetectiveExamine,
        DetectiveResetExamine,
        VenererCamo,
    }

    public static class RPCProcedure
    {
        public static void resetVariables()
        {
            RoleDraftEx.Clear();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
            reloadPluginOptions();
            Helpers.toggleZoom(reset: true);
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
            MapBehaviourPatch.clearAndReload();
            Helpers.isMovedVentOnPolus = false;
            IntroCutsceneOnDestroyPatch.isCooldownReseted = false;
        }

        public static void HandleShareOptions(byte numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.First(option => option.id == (int)optionId);
                    option.updateSelection((int)selection, i == numberOfOptions - 1);
                }
            }
            catch (Exception e)
            {
                TownOfUsPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void setRole(byte roleId, byte playerId)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == playerId)
                {
                    player.setRole((RoleId)roleId);
                    PlayerGameInfo.addRole(player.PlayerId, RoleInfo.roleInfoById[(RoleId)roleId]);
                    if (AmongUsClient.Instance.AmHost && Helpers.roleCanUseVents(player) && !player.Data.Role.IsImpostor)
                    {
                        player.RpcSetRole(RoleTypes.Engineer);
                        player.CoSetRole(RoleTypes.Engineer, true);
                    }
                }
            }
        }

        public static void setModifier(byte modifierId, byte playerId, byte flag)
        {
            PlayerControl player = Helpers.playerById(playerId);
            player.addModifier((RoleId)modifierId);
            PlayerGameInfo.addModifier(player.PlayerId, RoleInfo.roleInfoById[(RoleId)modifierId]);
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = Helpers.playerById(sourceId);
            var t = targetId == Byte.MaxValue ? null : Helpers.playerById(targetId).Data;
            if (source != null) source.ReportDeadBody(t);
        }

        public static void uncheckedExilePlayer(byte targetId)
        {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null) target.Exiled();
        }

        public static void dynamicMapOption(byte mapId)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
        }

        public static void setGameStarting()
        {
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
        }

        public static void shareGamemode(byte gm)
        {
            gameMode = (CustomGamemodes)gm;
            LobbyViewSettingsPatch.currentButtons?.ForEach(x => x?.gameObject?.Destroy());
            LobbyViewSettingsPatch.currentButtons?.Clear();
            LobbyViewSettingsPatch.currentButtonTypes?.Clear();
        }

        public static void stopStart(byte playerId)
        {
            if (AmongUsClient.Instance.AmHost && CustomOptionHolder.anyPlayerCanStopStart.getBool())
            {
                GameStartManager.Instance.ResetStartState();
                PlayerControl.LocalPlayer.RpcSendChat($"{Helpers.playerById(playerId).Data.PlayerName} stopped the game start!");
            }
        }

        public static void setFirstKill(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            firstKillPlayer = target;
        }

        public enum GhostInfoTypes { ChatInfo, BlankUsed, DeathReasonAndKiller, }
        public static void receiveGhostInfo(byte senderId, MessageReader reader)
        {
            PlayerControl sender = Helpers.playerById(senderId);

            GhostInfoTypes infoType = (GhostInfoTypes)reader.ReadByte();
            switch (infoType)
            {
                case GhostInfoTypes.ChatInfo:
                    string chatInfo = reader.ReadString();
                    if (Helpers.shouldShowGhostInfo())
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, chatInfo, false);
                    break;
                case GhostInfoTypes.BlankUsed:
                    Pursuer.blankedList.Remove(sender);
                    break;
                case GhostInfoTypes.DeathReasonAndKiller:
                    GameHistory.overrideDeathReasonAndKiller(Helpers.playerById(reader.ReadByte()), (DeadPlayer.CustomDeathReason)reader.ReadByte(), Helpers.playerById(reader.ReadByte()));
                    break;
            }
        }

        public static void updateMeeting(byte targetId, bool dead = true)
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == targetId && pva.AmDead != dead)
                    {
                        pva.SetDead(pva.DidReport, dead);
                        pva.Overlay.gameObject.SetActive(dead);
                    }

                    // Give players back their vote if target is shot dead
                    if (dead)
                    {
                        if (pva.VotedFor != targetId) continue;
                        pva.UnsetVote();
                        var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                        if (!voteAreaPlayer.AmOwner) continue;
                        MeetingHud.Instance.ClearVote();
                    }
                }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
        }

        public static void systemSpreadPlayers()
        {
            randomPlayersTP();
        }

        public static void systemCleanBody()
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
                UnityEngine.Object.Destroy(array[i].gameObject);
        }

        public static void engineerFixLights()
        {
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void engineerUsedRepair()
        {
            if (Helpers.shouldShowGhostInfo())
            {
                Helpers.showFlash(Engineer.color, 0.5f, "Engineer Used Repair");
            }
        }

        public static void jesterWin(byte playerId)
        {
            Jester.players.FirstOrDefault(x => x.player.PlayerId == playerId).triggerJesterWin = true;
        }

        public static void veteranAlert(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var veteran = Veteran.getRole(player);
            if (player == null || veteran == null) return;
            veteran.isAlertActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Veteran.alertDuration, new Action<float>((p) =>
            {
                if (p == 1f) veteran.isAlertActive = false;
            })));
        }

        public static void camouflagerCamouflage()
        {
            if (!Camouflager.exists) return;

            Camouflager.camouflageTimer = Camouflager.duration;
            if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.setLook("", 6, "", "", "", "");
        }

        public static void morphlingMorph(byte playerId, byte morphlingId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            Morphling morphling = Morphling.getRole(Helpers.playerById(morphlingId));
            if (morphling == null || target == null) return;

            morphling.morphTimer = Morphling.duration;
            morphling.morphTarget = target;
            if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive())
                morphling.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void lawyerSetTarget(byte playerId)
        {
            Lawyer.target = Helpers.playerById(playerId);
        }

        public static void lawyerPromotesToPursuer(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Lawyer.eraseRole(player);
            player.setRole(RoleId.Pursuer);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.pursuer);
        }

        public static void lawyerWin()
        {
            Lawyer.triggerLawyerWin = true;
        }

        public static void setBlanked(byte playerId, byte value)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);
        }

        public static void politicianCampaign(byte sourceId, byte targetId)
        {
            if (!Politician.exists) return;
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source == null || target == null) return;
            Politician.campaignPlayer(source, target);
        }

        public static void politicianTurnMayor(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Politician.eraseRole(player);
            player.setRole(RoleId.Mayor);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.mayor);
        }

        public static void mayorBodyguard(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var mayor = Mayor.getRole(player);
            if (player == null || mayor == null) return;
            mayor.isBodyguardActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Mayor.bodyguardDuration, new Action<float>((p) =>
            {
                if (p == 1f) mayor.isBodyguardActive = false;
            })));
        }

        public static void plaguebearerInfect(byte sourceId, byte targetId)
        {
            if (!Plaguebearer.exists) return;
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source == null || target == null) return;
            Plaguebearer.infectPlayer(source, target);
        }

        public static void plaguebearerTurnPestilence(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Plaguebearer.eraseRole(player);
            player.setRole(RoleId.Pestilence);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.pestilence);
        }

        public static void werewolfRampage(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var werewolf = Werewolf.getRole(player);
            if (player == null || werewolf == null) return;
            werewolf.isRampageActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Werewolf.duration, new Action<float>((p) =>
            {
                if (p == 1f) werewolf.isRampageActive = false;
            })));
        }

        public static void draculaCreatesVampire(byte targetId, byte draculaId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            var dracula = Dracula.getRole(Helpers.playerById(draculaId));
            if (player == null || dracula == null) return;
            if (Executioner.exists && Executioner.target == player && Executioner.livingPlayers.Count > 0)
            {
                executionerPromotesToAmnesiac(Executioner.livingPlayers.ToArray().FirstOrDefault().PlayerId);
            }

            if (!Dracula.canConvertBenignNeutral && player.isBenignNeutral() || !Dracula.canConvertEvilNeutral && player.isEvilNeutral() || !Dracula.canConvertKillingNeutral && player.isKillingNeutral() || !Dracula.canConvertImpostor && player.Data.Role.IsImpostor)
            {
                dracula.fakeVampire = player;
            }
            else
            {
                if (player.isRole(RoleId.Swooper))
                {
                    var swooper = Swooper.getRole(player);
                    if (swooper.isInvisble)
                    {
                        swooperSwoop(swooper.player.PlayerId, byte.MaxValue);
                    }
                }
                bool wasAgent = player.isRole(RoleId.Agent);
                bool wasImpostor = player.Data.Role.IsImpostor;
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                player.eraseAllRoles();
                player.setRole(RoleId.Vampire);
                PlayerGameInfo.addRole(player.PlayerId, RoleInfo.vampire);
                var vampire = Vampire.getRole(player);
                vampire.dracula = dracula;
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasAgent || wasImpostor) vampire.wasTeamRed = true;
                vampire.wasAgent = wasAgent;
                vampire.wasImpostor = wasImpostor;
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeNewVampCanAssassinate.getBool() && !HandleGuesser.isGuesser(targetId))
                    setGuesserGm(targetId);
            }
            Dracula.canCreateVampire = false;
            Dracula.convertedVampires++;
        }

        public static void vampirePromotes(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var vampire = Vampire.getRole(player);
            if (vampire == null) return;
            bool wasTeamRed = vampire.wasTeamRed;
            bool wasAgent = vampire.wasAgent;
            bool wasImpostor = vampire.wasImpostor;
            Vampire.eraseRole(player);
            player.setRole(RoleId.Dracula);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.dracula);
            var dracula = Dracula.getRole(player);
            dracula.removeCurrentDracula();
            Dracula.canCreateVampire = Dracula.convertedVampires < Dracula.maxVampires;
            dracula.wasTeamRed = wasTeamRed;
            dracula.wasAgent = wasAgent;
            dracula.wasImpostor = wasImpostor;
            return;
        }

        public static void swapperSwap(byte playerId1, byte playerId2)
        {
            if (MeetingHud.Instance)
            {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void vampireHunterPromote(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            VampireHunter.eraseRole(player);
            if (VampireHunter.promotesTo == 0) PlayerGameInfo.addRole(player.PlayerId, RoleInfo.crewmate);
            else if (VampireHunter.promotesTo == 1)
            {
                player.setRole(RoleId.Sheriff);
                PlayerGameInfo.addRole(player.PlayerId, RoleInfo.sheriff);
            }
            else if (VampireHunter.promotesTo == 2)
            {
                player.setRole(RoleId.Veteran);
                PlayerGameInfo.addRole(player.PlayerId, RoleInfo.veteran);
            }
        }

        public static void medicSetShielded(byte shieldedId, byte medicId)
        {
            var medic = Medic.getRole(Helpers.playerById(medicId));
            medic.usedShield = true;
            medic.shielded = Helpers.playerById(shieldedId);
        }

        public static void shieldedMurderAttempt(byte medicId)
        {
            var medic = Medic.getRole(Helpers.playerById(medicId));
            if (medic == null || medic.shielded == null) return;

            bool getsNotification = Medic.getsNotification == 0 // Everyone
                || Medic.getsNotification == 1 & PlayerControl.LocalPlayer == medic.shielded // Shielded
                || Medic.getsNotification == 2 & PlayerControl.LocalPlayer == medic.player // Medic
                || Medic.getsNotification == 3 && (PlayerControl.LocalPlayer == medic.shielded || PlayerControl.LocalPlayer == medic.player) /* Medic + Shielded */;
            if (getsNotification && !PlayerControl.LocalPlayer.isFlashedByGrenadier() || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Medic.color, 1f, "Failed Murder Attempt on Shielded Player");
        }

        public static void setGuesserGm(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            new GuesserGM(target);
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
        {
            PlayerControl killer = Helpers.playerById(killerId);
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (Lawyer.target != null && dyingTarget == Lawyer.target) Lawyer.targetWasGuessed = true;
            if (GuardianAngel.target != null && dyingTarget == GuardianAngel.target) GuardianAngel.targetWasGuessed = true;

            PlayerControl guesser = Helpers.playerById(killerId);
            if (killer.isRole(RoleId.Thief) && Thief.canStealWithGuess)
            {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                if (!killer.Data.IsDead && !Thief.isFailedThiefKill(dyingTarget, guesser, roleInfo))
                {
                    thiefStealsRole(dyingTarget.PlayerId, killerId);
                }
            }

            bool lawyerDiedAdditionally = false;
            if (killer.isRole(RoleId.Lawyer) && Lawyer.target != null && Lawyer.target.PlayerId == dyingTargetId)
            {
                // Lawyer guessed client.
                if (PlayerControl.LocalPlayer == killer)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, killer.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                killer.Exiled();
                lawyerDiedAdditionally = true;
                overrideDeathReasonAndKiller(killer, DeadPlayer.CustomDeathReason.LawyerSuicide, guesser);
            }
            bool gaDiedAdditionally = false;
            if (killer.isRole(RoleId.GuardianAngel) && GuardianAngel.target != null && GuardianAngel.target.PlayerId == dyingTargetId)
            {
                // Guardian Angel guessed target.
                if (PlayerControl.LocalPlayer == killer)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, killer.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                killer.Exiled();
                gaDiedAdditionally = true;
                overrideDeathReasonAndKiller(killer, DeadPlayer.CustomDeathReason.GASuicide, guesser);
            }
            bool exeDiedAdditionally = false;
            if (killer.isRole(RoleId.Executioner) && Executioner.target != null && Executioner.target.PlayerId == dyingTargetId)
            {
                // Executioner guessed target.
                if (PlayerControl.LocalPlayer == killer)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, killer.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                killer.Exiled();
                exeDiedAdditionally = true;
            }

            if (killer.isRole(RoleId.Scavenger) && Scavenger.canEatWithGuess)
            {
                var scavenger = Scavenger.getRole(killer);
                scavenger.eatenBodies++;
                if (scavenger.eatenBodies == Scavenger.scavengerNumberToWin)
                {
                    scavenger.triggerScavengerWin = true;
                }
            }

            dyingTarget.Exiled();
            GameHistory.overrideDeathReasonAndKiller(dyingTarget, DeadPlayer.CustomDeathReason.Guess, guesser);
            HandleGuesser.remainingShots(killerId, true);

            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == dyingTargetId || lawyerDiedAdditionally && Helpers.playerById(pva.TargetPlayerId).isRole(RoleId.Lawyer) || gaDiedAdditionally && Helpers.playerById(pva.TargetPlayerId).isRole(RoleId.GuardianAngel) || exeDiedAdditionally && Helpers.playerById(pva.TargetPlayerId).isRole(RoleId.Executioner))
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                        MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, pva.TargetPlayerId);
                    }

                    //Give players back their vote if target is shot dead
                    if (pva.VotedFor != dyingTargetId && (!lawyerDiedAdditionally || Lawyer.allPlayers.ToArray().Where(x => x.PlayerId != dyingTarget.PlayerId).FirstOrDefault().PlayerId != pva.VotedFor || !gaDiedAdditionally || GuardianAngel.allPlayers.ToArray().Where(x => x.PlayerId != dyingTarget.PlayerId).FirstOrDefault().PlayerId != pva.VotedFor || !exeDiedAdditionally || Executioner.allPlayers.ToArray().Where(x => x.PlayerId != dyingTarget.PlayerId).FirstOrDefault().PlayerId != pva.VotedFor)) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();

                }
                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
            if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
            {
                if (PlayerControl.LocalPlayer == dyingTarget)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                }
            }

            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null && guesser != null)
            {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                string msg = $"{guesser.Data.PlayerName} guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg, false);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
        }

        public static void survivorSafeguard(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var survivor = Survivor.getRole(player);
            if (player == null || survivor == null) return;
            survivor.isSafeguardActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Survivor.duration, new Action<float>((p) =>
            {
                if (p == 1f) survivor.isSafeguardActive = false;
            })));
        }

        public static void guardianAngelSetTarget(byte targetId)
        {
            GuardianAngel.target = Helpers.playerById(targetId);
        }

        public static void guardianAngelProtect(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var ga = GuardianAngel.getRole(player);
            if (player == null || ga == null) return;
            ga.isProtectActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GuardianAngel.duration, new Action<float>((p) =>
            {
                if (p == 1f) ga.isProtectActive = false;
            })));
        }

        public static void guardianAngelPromotes(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            GuardianAngel.eraseRole(player);
            player.setRole(RoleId.Survivor);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.survivor);
        }

        public static void amnesiacRemember(byte targetId, byte amnesiacId)
        {
            PlayerControl target = Helpers.playerById(targetId);
            PlayerControl amnesiac = Helpers.playerById(amnesiacId);
            if (target == null || amnesiac == null) return;

            amnesiac.eraseRole(RoleId.Amnesiac);
            RoleId targetRoleId = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().roleId;
            if (Amnesiac.resetRole)
            {
                if (targetRoleId == RoleId.Crewmate)
                {
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.crewmate);
                }
                else if (targetRoleId == RoleId.Impostor)
                {
                    Helpers.turnToImpostor(amnesiac);
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.impostor);
                }
                else
                {
                    target.eraseRole(targetRoleId);
                    if (target.Data.Role.IsImpostor)
                    {
                        Helpers.turnToImpostor(amnesiac);
                        PlayerGameInfo.addRole(target.PlayerId, RoleInfo.impostor);
                    }
                    else PlayerGameInfo.addRole(target.PlayerId, RoleInfo.crewmate);
                    amnesiac.setRole(targetRoleId);
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.roleInfoById[targetRoleId]);
                }
            }
            else
            {
                if (targetRoleId == RoleId.Crewmate)
                {
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.crewmate);
                }
                else if (targetRoleId == RoleId.Impostor)
                {
                    Helpers.turnToImpostor(amnesiac);
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.impostor);
                }
                else
                {
                    if (target.Data.Role.IsImpostor)
                    {
                        Helpers.turnToImpostor(amnesiac);
                        PlayerGameInfo.addRole(target.PlayerId, RoleInfo.impostor);
                    }
                    amnesiac.setRole(targetRoleId);
                    PlayerGameInfo.addRole(amnesiac.PlayerId, RoleInfo.roleInfoById[targetRoleId]);
                }
            }

            if (target == Lawyer.target)
                Lawyer.target = amnesiac;
            if (target == GuardianAngel.target)
                GuardianAngel.target = amnesiac;
            if (amnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
        }

        public static void executionerSetTarget(byte playerId)
        {
            Executioner.target = Helpers.playerById(playerId);
        }

        public static void executionerPromotesToAmnesiac(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Executioner.eraseRole(player);
            player.setRole(RoleId.Amnesiac);
            PlayerGameInfo.addRole(player.PlayerId, RoleInfo.amnesiac);
        }

        public static void executionerWin()
        {
            Executioner.triggerExecutionerWin = true;
        }

        public static void poisonerSetPoisoned(byte targetId, byte performReset, byte poisonerId)
        {
            var poisoner = Poisoner.getRole(Helpers.playerById(poisonerId));
            if (poisoner == null) return;
            if (performReset != 0)
            {
                poisoner.poisoned = null;
                return;
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId && !player.Data.IsDead)
                {
                    poisoner.poisoned = player;
                }
            }
        }

        public static void cleanBody(byte playerId, byte cleaningPlayerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            PlayerControl cleanPlayer = Helpers.playerById(cleaningPlayerId);

            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
            if (cleanPlayer.isRole(RoleId.Scavenger))
            {
                var scavenger = Scavenger.getRole(cleanPlayer);
                scavenger.eatenBodies++;
                if (scavenger.eatenBodies == Scavenger.scavengerNumberToWin)
                {
                    scavenger.triggerScavengerWin = true;
                }
            }
        }

        public static void swooperSwoop(byte playerId, byte flag)
        {
            PlayerControl target = Helpers.playerById(playerId);
            var swooper = Swooper.getRole(target);
            if (target == null || swooper == null) return;

            if (flag == byte.MaxValue)
            {
                target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
                target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
                target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);

                if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive()) target.setDefaultLook();
                swooper.isInvisble = false;
                return;
            }

            target.setLook("", 6, "", "", "", "");
            Color color = Color.clear;
            bool canSee = PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead;
            if (canSee) color.a = 0.1f;
            target.cosmetics.currentBodySprite.BodySprite.color = color;
            target.cosmetics.colorBlindText.gameObject.SetActive(false);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(canSee ? 0.1f : 0f);
            swooper.swoopTimer = Swooper.duration;
            swooper.isInvisble = true;
        }

        public static void shifterShift(byte targetId)
        {
            PlayerControl oldShifter = Shifter.allPlayers.FirstOrDefault();
            PlayerControl target = Helpers.playerById(targetId);
            if (target == null || oldShifter == null) return;

            Shifter.futureShift = null;

            // Suicide (exile) when evil role
            if (target.isEvil() || target.isRole(RoleId.Agent))
            {
                if (!oldShifter.Data.IsDead)
                {
                    oldShifter.Exiled();
                    overrideDeathReasonAndKiller(oldShifter, DeadPlayer.CustomDeathReason.Shift, target);
                }
                if (oldShifter == Lawyer.target && AmongUsClient.Instance.AmHost)
                {
                    foreach (var lawyer in Lawyer.allPlayers)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                        writer.Write(lawyer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        lawyerPromotesToPursuer(lawyer.PlayerId);
                    }
                }
                if (oldShifter == GuardianAngel.target && AmongUsClient.Instance.AmHost)
                {
                    foreach (var ga in GuardianAngel.allPlayers)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                        writer.Write(ga.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        guardianAngelPromotes(ga.PlayerId);
                    }
                }
                if (oldShifter == Executioner.target && AmongUsClient.Instance.AmHost)
                {
                    foreach (var exe in Executioner.allPlayers)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToAmnesiac, Hazel.SendOption.Reliable, -1);
                        writer.Write(exe.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        executionerPromotesToAmnesiac(exe.PlayerId);
                    }
                }
                return;
            }

            oldShifter.eraseModifier(RoleId.Shifter);

            // Switch shield
            if (Shifter.shiftsMedicShield)
            {
                if (Medic.isShielded(target))
                {
                    foreach (var medic in Medic.GetMedic(target))
                    {
                        medic.shielded = oldShifter;
                    }
                }
                else if (Medic.isShielded(oldShifter))
                {
                    foreach (var medic in Medic.GetMedic(oldShifter))
                    {
                        medic.shielded = target;
                    }
                }
            }

            RoleId oldShifterRoleId = RoleInfo.getRoleInfoForPlayer(oldShifter, false).FirstOrDefault().roleId;
            RoleId targetRoleId = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().roleId;

            target.eraseRole(targetRoleId);
            oldShifter.eraseRole(oldShifterRoleId);

            target.setRole(oldShifterRoleId);
            PlayerGameInfo.addRole(target.PlayerId, RoleInfo.roleInfoById[oldShifterRoleId]);

            oldShifter.setRole(targetRoleId);
            PlayerGameInfo.addRole(oldShifter.PlayerId, RoleInfo.roleInfoById[targetRoleId]);

            if (Lawyer.target == target) Lawyer.target = oldShifter;
            if (GuardianAngel.target == target) GuardianAngel.target = oldShifter;
            if (Executioner.target == target) Executioner.target = oldShifter;

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == target)
                CustomButton.ResetAllCooldowns();
        }

        public static void setFutureShifted(byte targetId)
        {
            Shifter.futureShift = Helpers.playerById(targetId);
        }

        public static void oracleConfess(byte targetId, byte oracleId)
        {
            var oracle = Oracle.getRole(Helpers.playerById(oracleId));
            oracle.confessor = Helpers.playerById(targetId);
            if (oracle.confessor == null) return;

            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(oracle.confessor, false).FirstOrDefault();
            if (roleInfo == null) return;

            bool showsCorrectFaction = UnityEngine.Random.RandomRangeInt(1, 101) <= Oracle.accuracy;
            FactionId revealedFactionId;

            if (showsCorrectFaction)
            {
                if (roleInfo.factionId == FactionId.KillingNeutral || roleInfo.factionId == FactionId.BenignNeutral) revealedFactionId = FactionId.EvilNeutral;
                else revealedFactionId = roleInfo.factionId;
            }
            else
            {
                List<FactionId> possibleFaction = new List<FactionId> { FactionId.Crewmate, FactionId.Impostor, FactionId.BenignNeutral };
                possibleFaction.Remove(roleInfo.factionId);
                revealedFactionId = possibleFaction[UnityEngine.Random.RandomRangeInt(0, possibleFaction.Count)];
            }

            oracle.revealedFactionId = revealedFactionId;

            var results = oracle.GetInfo(oracle.confessor);
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(oracle.player, $"{results}");

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OracleConfess, SendOption.Reliable, -1);
            writer.Write(oracle.confessor.PlayerId);
            writer.Write(oracle.player.PlayerId);
            writer.Write((int)revealedFactionId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            // Ghost Info
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
            writer.Write(oracle.confessor.PlayerId);
            writer.Write((byte)GhostInfoTypes.ChatInfo);
            writer.Write(results);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
        }

        public static void mercenarySetShielded(byte shieldedId, byte mercenaryId)
        {
            var mercenary = Mercenary.getRole(Helpers.playerById(mercenaryId));
            mercenary.usedAbility = true;
            mercenary.shielded = Helpers.playerById(shieldedId);
        }

        public static void mercenaryShieldedMurderAttempt(byte shieldedId, byte mercenaryId, bool isMeeting)
        {
            var mercenary = Mercenary.getRole(Helpers.playerById(mercenaryId));
            if (mercenary == null || mercenary.shielded == null) return;

            bool getsNotification = Mercenary.getsNotification == 0 // Everyone
                || Mercenary.getsNotification == 1 & PlayerControl.LocalPlayer == mercenary.shielded // Shielded
                || Mercenary.getsNotification == 2 & PlayerControl.LocalPlayer == mercenary.player // Mercenary
                || Mercenary.getsNotification == 3 && (PlayerControl.LocalPlayer == mercenary.shielded || PlayerControl.LocalPlayer == mercenary.player) /* Mercenary + Shielded */;
            if (getsNotification && !PlayerControl.LocalPlayer.isFlashedByGrenadier() || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Mercenary.color, 1f, "Failed Murder Attempt on Shielded Player");

            if (mercenary.shielded.PlayerId == shieldedId && mercenary.player.PlayerId == mercenaryId && !isMeeting)
            {
                mercenary.shielded = null;
                mercenary.usedAbility = false;
                mercenary.exShielded = Helpers.playerById(shieldedId);
                mercenary.brilders += 1;
            }
        }

        public static void mercenaryDonArmor(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var mercenary = Mercenary.getRole(player);
            if (player == null || mercenary == null) return;
            mercenary.isArmorActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Mercenary.duration, new Action<float>((p) =>
            {
                if (p == 1f) mercenary.isArmorActive = false;
            })));
        }

        public static void blackmailPlayer(byte playerId, byte blackmailerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl blackmailer = Helpers.playerById(blackmailerId);
            var blackmailerRole = Blackmailer.getRole(blackmailer);
            if (blackmailerRole == null) return;
            blackmailerRole.blackmailed = target;
        }

        public static void unblackmailPlayer()
        {
            foreach (var p in Blackmailer.players)
                p.blackmailed = null;
            Blackmailer.alreadyShook = false;
        }

        public static void grenadierFlash(bool clear = false)
        {
            if (clear)
            {
                Grenadier.flashedPlayers.Clear();
                return;
            }

            var closestPlayers = Helpers.getClosestPlayers(Grenadier.livingPlayers.FirstOrDefault().GetTruePosition(), Grenadier.radius == 0 ? 100f : Grenadier.radius * 0.25f, true);
            Grenadier.flashedPlayers = closestPlayers;
            foreach (PlayerControl player in closestPlayers)
            {
                if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
                {
                    if (player.Data.Role.IsImpostor && !player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 0.5f);
                    else if (!player.Data.Role.IsImpostor && !player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 1f);
                    else if (player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 0.5f);
                }
            }
            Grenadier.flashTimer = Grenadier.duration;
        }

        public static void glitchMimic(byte playerId, byte glitchId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            Glitch glitch = Glitch.getRole(Helpers.playerById(glitchId));
            if (glitch == null || target == null) return;

            Glitch.morphTimer = Glitch.morphDuration;
            Glitch.morphPlayer = target;
            if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive())
                glitch.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void glitchHack(byte playerId, byte glitchId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            Glitch glitch = Glitch.getRole(Helpers.playerById(glitchId));
            if (glitch == null || target == null) return;

            Glitch.hackedPlayer = target;
            Coroutines.Start(Helpers.Hack(target));
        }

        public static void thiefStealsRole(byte playerId, byte thiefId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl thief = Helpers.playerById(thiefId);
            if (target == null) return;
            thief.eraseRole(RoleId.Thief);

            RoleInfo targetRoleInfo = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
            if (targetRoleInfo.roleId == RoleId.Mayor || targetRoleInfo.roleId == RoleId.Sheriff || targetRoleInfo.roleId == RoleId.VampireHunter || targetRoleInfo.roleId == RoleId.Veteran)
            {
                target.eraseRole(targetRoleInfo.roleId);
                PlayerGameInfo.addRole(target.PlayerId, RoleInfo.crewmate);

                thief.setRole(targetRoleInfo.roleId);
                PlayerGameInfo.addRole(thief.PlayerId, RoleInfo.roleInfoById[targetRoleInfo.roleId]);
            }
            else if (targetRoleInfo.factionId == FactionId.Impostor)
            {
                Helpers.turnToImpostor(thief);
                target.eraseRole(targetRoleInfo.roleId);
                PlayerGameInfo.addRole(target.PlayerId, RoleInfo.impostor);

                thief.setRole(targetRoleInfo.roleId);
                PlayerGameInfo.addRole(thief.PlayerId, RoleInfo.roleInfoById[targetRoleInfo.roleId]);
            }
            else if (targetRoleInfo.factionId == FactionId.KillingNeutral)
            {
                thief.setRole(targetRoleInfo.roleId);
                PlayerGameInfo.addRole(thief.PlayerId, RoleInfo.roleInfoById[targetRoleInfo.roleId]);
            }

            if (target == GuardianAngel.target)
                GuardianAngel.target = thief;

            if (target == Executioner.target)
                Executioner.target = thief;

            if (target == Lawyer.target)
                Lawyer.target = thief;

            if (thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            Thief.formerThiefs.Add(thief);
        }

        public static void detectiveExamine(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            Detective.examined = target;
        }

        public static void detectiveResetExamine()
        {
            Detective.examined = null;
        }

        public static void venererCamo(byte venererId)
        {
            Venerer venerer = Venerer.getRole(Helpers.playerById(venererId));
            if (venerer == null) return;

            venerer.morphTimer = Venerer.duration;
            if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive())
            {
                venerer.player.setLook("", 15, "", "", "", "");
                venerer.player.cosmetics.colorBlindText.text = "";
            }
        }

        public static void randomPlayersTP()
        {
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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId)
            {
                case (byte)CustomRPC.ResetVariables:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptions:
                    RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = reader.ReadByte();
                    byte playerId = reader.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;
                case (byte)CustomRPC.SetModifier:
                    byte modifierId = reader.ReadByte();
                    byte pId = reader.ReadByte();
                    byte flag = reader.ReadByte();
                    RPCProcedure.setModifier(modifierId, pId, flag);
                    break;
                case (byte)CustomRPC.UncheckedMurderPlayer:
                    byte source = reader.ReadByte();
                    byte target = reader.ReadByte();
                    byte showAnimation = reader.ReadByte();
                    RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                    break;
                case (byte)CustomRPC.UncheckedExilePlayer:
                    byte exileTarget = reader.ReadByte();
                    RPCProcedure.uncheckedExilePlayer(exileTarget);
                    break;
                case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                    byte reportSource = reader.ReadByte();
                    byte reportTarget = reader.ReadByte();
                    RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                    break;
                case (byte)CustomRPC.DynamicMapOption:
                    byte mapId = reader.ReadByte();
                    RPCProcedure.dynamicMapOption(mapId);
                    break;
                case (byte)CustomRPC.SetGameStarting:
                    RPCProcedure.setGameStarting();
                    break;
                case (byte)CustomRPC.ShareGamemode:
                    byte gm = reader.ReadByte();
                    RPCProcedure.shareGamemode(gm);
                    break;
                case (byte)CustomRPC.StopStart:
                    RPCProcedure.stopStart(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetFirstKill:
                    byte firstKill = reader.ReadByte();
                    RPCProcedure.setFirstKill(firstKill);
                    break;
                case (byte)CustomRPC.ShareGhostInfo:
                    RPCProcedure.receiveGhostInfo(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.SystemSpreadPlayers:
                    RPCProcedure.systemSpreadPlayers();
                    break;
                case (byte)CustomRPC.SystemCleanBody:
                    RPCProcedure.systemCleanBody();
                    break;
                
                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.JesterWin:
                    RPCProcedure.jesterWin(reader.ReadByte());
                    break;
                case (byte)CustomRPC.VeteranAlert:
                    RPCProcedure.veteranAlert(reader.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.LawyerSetTarget:
                    RPCProcedure.lawyerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.LawyerPromotesToPursuer:
                    RPCProcedure.lawyerPromotesToPursuer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.LawyerWin:
                    RPCProcedure.lawyerWin();
                    break;
                case (byte)CustomRPC.SetBlanked:
                    var pid = reader.ReadByte();
                    var blankedValue = reader.ReadByte();
                    RPCProcedure.setBlanked(pid, blankedValue);
                    break;
                case (byte)CustomRPC.PoliticianCampaign:
                    RPCProcedure.politicianCampaign(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.PoliticianTurnMayor:
                    RPCProcedure.politicianTurnMayor(reader.ReadByte());
                    break;
                case (byte)CustomRPC.MayorBodyguard:
                    RPCProcedure.mayorBodyguard(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaguebearerInfect:
                    RPCProcedure.plaguebearerInfect(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaguebearerTurnPestilence:
                    RPCProcedure.plaguebearerTurnPestilence(reader.ReadByte());
                    break;
                case (byte)CustomRPC.WerewolfRampage:
                    RPCProcedure.werewolfRampage(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DraculaCreatesVampire:
                    RPCProcedure.draculaCreatesVampire(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.VampirePromotes:
                    RPCProcedure.vampirePromotes(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = reader.ReadByte();
                    byte playerId2 = reader.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.VampireHunterPromotes:
                    RPCProcedure.vampireHunterPromote(reader.ReadByte());
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetGuesserGm:
                    byte guesserGm = reader.ReadByte();
                    RPCProcedure.setGuesserGm(guesserGm);
                    break;
                case (byte)CustomRPC.GuesserShoot:
                    byte killerId = reader.ReadByte();
                    byte dyingTarget = reader.ReadByte();
                    byte guessedTarget = reader.ReadByte();
                    byte guessedRoleId = reader.ReadByte();
                    RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                    break;
                case (byte)CustomRPC.SurvivorSafeguard:
                    RPCProcedure.survivorSafeguard(reader.ReadByte());
                    break;
                case (byte)CustomRPC.GuardianAngelSetTarget:
                    RPCProcedure.guardianAngelSetTarget(reader.ReadByte());
                    break;
                case (byte)CustomRPC.GuardianAngelProtect:
                    RPCProcedure.guardianAngelProtect(reader.ReadByte());
                    break;
                case (byte)CustomRPC.GuardianAngelPromotes:
                    RPCProcedure.guardianAngelPromotes(reader.ReadByte());
                    break;
                case (byte)CustomRPC.AmnesiacRemember:
                    RPCProcedure.amnesiacRemember(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ExecutionerSetTarget:
                    RPCProcedure.executionerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.ExecutionerPromotesToAmnesiac:
                    RPCProcedure.executionerPromotesToAmnesiac(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ExecutionerWin:
                    RPCProcedure.executionerWin();
                    break;
                case (byte)CustomRPC.PoisonerSetPoisoned:
                    byte poisonedId = reader.ReadByte();
                    byte reset = reader.ReadByte();
                    RPCProcedure.poisonerSetPoisoned(poisonedId, reset, reader.ReadByte());
                    break;
                case (byte)CustomRPC.CleanBody:
                    RPCProcedure.cleanBody(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.SwooperSwoop:
                    RPCProcedure.swooperSwoop(reader.ReadByte(), reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.SetFutureShifted:
                    RPCProcedure.setFutureShifted(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(reader.ReadByte());
                    break;
                case (byte)CustomRPC.OracleConfess:
                    byte confessorId = reader.ReadByte();
                    byte oracleId = reader.ReadByte();
                    var oracle = Oracle.getRole(Helpers.playerById(oracleId));
                    oracle.confessor = Helpers.playerById(confessorId);
                    if (oracle.confessor == null) break;
                    int factionId = reader.ReadInt32();
                    if (Enum.IsDefined(typeof(FactionId), factionId))
                    {
                        oracle.revealedFactionId = (FactionId)factionId;
                    }
                    else
                    {
                        oracle.revealedFactionId = FactionId.Crewmate;
                    }
                    break;
                case (byte)CustomRPC.MercenarySetShielded:
                    RPCProcedure.mercenarySetShielded(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.MercenaryShieldedMurderAttempt:
                    RPCProcedure.mercenaryShieldedMurderAttempt(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean()); 
                    break;
                case (byte)CustomRPC.MercenaryDonArmor:
                    RPCProcedure.mercenaryDonArmor(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.BlackmailPlayer:
                    RPCProcedure.blackmailPlayer(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.UnblackmailPlayer:
                    RPCProcedure.unblackmailPlayer();
                    break;
                case (byte)CustomRPC.GrenadierFlash:
                    RPCProcedure.grenadierFlash(reader.ReadBoolean());
                    break;
                case (byte)CustomRPC.GlitchMimic:
                    RPCProcedure.glitchMimic(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.GlitchHack:
                    RPCProcedure.glitchHack(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.DraftModePickOrder:
                    RoleDraftEx.receivePickOrder(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.DraftModePick:
                    RoleDraftEx.receivePick(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.TransporterTransport:
                    Coroutines.Start(Helpers.TransportPlayers(reader.ReadByte(), reader.ReadByte()));
                    break;
                case (byte)CustomRPC.ThiefStealsRole:
                    byte thiefTargetId = reader.ReadByte();
                    RPCProcedure.thiefStealsRole(thiefTargetId, reader.ReadByte());
                    break;
                case (byte)CustomRPC.DetectiveExamine:
                    RPCProcedure.detectiveExamine(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DetectiveResetExamine:
                    RPCProcedure.detectiveResetExamine();
                    break;
                case (byte)CustomRPC.VenererCamo:
                    RPCProcedure.venererCamo(reader.ReadByte());
                    break;
            }
        }
    }
}