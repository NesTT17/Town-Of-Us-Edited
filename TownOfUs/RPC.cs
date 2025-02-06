using static TownOfUs.TownOfUs;
using static TownOfUs.HudManagerStartPatch;
using static TownOfUs.GameHistory;
using static TownOfUs.TOUMapOptions;
using Hazel;
using System;
using AmongUs.GameOptions;
using HarmonyLib;
using System.Linq;
using Reactor.Utilities.Extensions;
using Assets.CoreScripts;
using TownOfUs.CustomGameModes;
using UnityEngine;
using AmongUs.Data;
using System.Collections.Generic;

namespace TownOfUs
{
    public enum RoleId {
        Morphling,
        Camouflager,
        Snitch,
        Engineer,
        Sheriff,
        Jester,
        Shifter,
        Spy,
        Vigilante,
        Assassin,
        Swapper,
        Mayor,
        Medic,
        Dracula,
        Vampire,
        Poisoner,
        Scavenger,
        Executioner,
        Lawyer,
        Pursuer,
        GuardianAngel,
        FallenAngel,
        Survivor,
        Amnesiac,
        Investigator,
        Veteran,
        Seer,
        Juggernaut,
        Swooper,
        Mercenary,
        Blackmailer,
        Escapist,
        Miner,
        Cleaner,
        Trapper,
        Phantom,
        Grenadier,
        Doomsayer,
        // Modifier ---
        Lover,
        Blind,
        Bait,
        Sleuth,
        Tiebreaker,
        ButtonBarry,
        Indomitable,
        Drunk,
        Sunglasses,
        Torch,
        DoubleShot,
        // Vanilla Roles
        Crewmate, Impostor
    }

    public enum FactionId {
        Crewmate, Neutral, NeutralKiller, Impostor, Modifier
    }

    public enum CustomRPC {
        ResetVaribles = 100,
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
        UseUncheckedVent,
        ShareGhostInfo,
        InitGameSummary,

        // Role functionality
        MorphlingMorph,
        CamouflagerCamouflage,
        EngineerFixLights,
        EngineerUsedRepair,
        EngineerUsedDoors,
        ShareSheriffSelfShot,
        ShifterShift,
        SetFutureShifted,
        GuesserShoot,
        SwapperSwap,
        SetTiebreak,
        MayorReveal,
        MedicSetShielded,
        ShieldedMurderAttempt,
        DraculaCreatesVampire,
        VampirePromotes,
        PoisonerSetPoisoned,
        CleanBody,
        ExecutionerSetTarget,
        ExecutionerPromotesToPursuer,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        SetBlanked,
        GuardianAngelSetTarget,
        GuardianAngelProtect,
        GuardianAngelPromotes,
        GuardianToFa,
        GuardianToSurv,
        SurvivorSafeguard,
        AmnesiacRemember,
        InvestigatorWatchPlayer,
        InvestigatorWatchFlash,
        VeteranAlert,
        SwooperSwoop,
        MercenaryShield,
        MercenaryResetShield,
        MercenaryAddMurder,
        BlackmailPlayer,
        UnblackmailPlayer,
        EscapistMarkLocation,
        EscapistRecall,
        PlaceVent,
        ShowIndomitableFlash,
        PhantomInvis,
        GrenadierFlash,
        DoomsayerObserve,
        
        // Gamemode
        SetGuesserGm,
    }

    public static class RPCProcedure {
        public static void resetVariables() {
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
            reloadPluginOptions();
            Helpers.toggleZoom(reset : true);
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
            MapBehaviourPatch.clearAndReload();
            MinerVent.ClearMinerVents();
            Trapper.traps.ClearTraps();
        }

        public static void HandleShareOptions(byte numberOfOptions, MessageReader reader) {            
            try {
                for (int i = 0; i < numberOfOptions; i++) {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.First(option => option.id == (int)optionId);
                    option.updateSelection((int)selection, i == numberOfOptions - 1);
                }
            } catch (Exception e) {
                TownOfUsPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void setRole(byte roleId, byte playerId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (player.PlayerId == playerId) {
                    switch ((RoleId)roleId) {
                    case RoleId.Morphling:
                        Morphling.morphling = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.morphling);
                        break;
                    case RoleId.Camouflager:
                        Camouflager.camouflager = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.camouflager);
                        break;
                    case RoleId.Snitch:
                        Snitch.snitch = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.snitch);
                        break;
                    case RoleId.Engineer:
                        Engineer.engineer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.engineer);
                        break;
                    case RoleId.Sheriff:
                        Sheriff.sheriff = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.sheriff);
                        break;
                    case RoleId.Jester:
                        Jester.jester = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.jester);
                        break;
                    case RoleId.Shifter:
                        Shifter.shifter = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.shifter);
                        break;
                    case RoleId.Spy:
                        Spy.spy = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.spy);
                        break;
                    case RoleId.Assassin:
                        Guesser.evilGuesser = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.assassin);
                        break;
                    case RoleId.Vigilante:
                        Guesser.niceGuesser = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.vigilante);
                        break;
                    case RoleId.Swapper:
                        Swapper.swapper = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.swapper);
                        break;
                    case RoleId.Mayor:
                        Mayor.mayor = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.mayor);
                        break;
                    case RoleId.Medic:
                        Medic.medic = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.medic);
                        break;
                    case RoleId.Dracula:
                        Dracula.dracula = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.dracula);
                        break;
                    case RoleId.Vampire:
                        Vampire.vampire = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.vampire);
                        break;
                    case RoleId.Poisoner:
                        Poisoner.poisoner = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.poisoner);
                        break;
                    case RoleId.Scavenger:
                        Scavenger.scavenger = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.scavenger);
                        break;
                    case RoleId.Executioner:
                        Executioner.executioner = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.executioner);
                        break;
                    case RoleId.Lawyer:
                        Lawyer.lawyer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.lawyer);
                        break;
                    case RoleId.Pursuer:
                        Pursuer.pursuer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.pursuer);
                        break;
                    case RoleId.GuardianAngel:
                        GuardianAngel.guardianAngel = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.guardianAngel);
                        break;
                    case RoleId.FallenAngel:
                        FallenAngel.fallenAngel = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.fallenAngel);
                        break;
                    case RoleId.Survivor:
                        Survivor.survivor = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.survivor);
                        break;
                    case RoleId.Amnesiac:
                        Amnesiac.amnesiac = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.amnesiac);
                        break;
                    case RoleId.Investigator:
                        Investigator.investigator = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.investigator);
                        break;
                    case RoleId.Veteran:
                        Veteran.veteran = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.veteran);
                        break;
                    case RoleId.Seer:
                        Seer.seer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.seer);
                        break;
                    case RoleId.Juggernaut:
                        Juggernaut.juggernaut = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.juggernaut);
                        break;
                    case RoleId.Swooper:
                        Swooper.swooper = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.swooper);
                        break;
                    case RoleId.Mercenary:
                        Mercenary.mercenary = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.mercenary);
                        break;
                    case RoleId.Blackmailer:
                        Blackmailer.blackmailer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.blackmailer);
                        break;
                    case RoleId.Escapist:
                        Escapist.escapist = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.escapist);
                        break;
                    case RoleId.Miner:
                        Miner.miner = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.miner);
                        break;
                    case RoleId.Cleaner:
                        Cleaner.cleaner = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.cleaner);
                        break;
                    case RoleId.Trapper:
                        Trapper.trapper = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.trapper);
                        break;
                    case RoleId.Phantom:
                        Phantom.phantom = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.phantom);
                        break;
                    case RoleId.Grenadier:
                        Grenadier.grenadier = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.grenadier);
                        break;
                    case RoleId.Doomsayer:
                        Doomsayer.doomsayer = player;
                        PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.doomsayer);
                        break;
                    }
                    if (AmongUsClient.Instance.AmHost && Helpers.roleCanUseVents(player) && !player.Data.Role.IsImpostor) {
                        player.RpcSetRole(RoleTypes.Engineer);
                        player.CoSetRole(RoleTypes.Engineer, true);
                    }
                }
            }
        }

        public static void setModifier(byte modifierId, byte playerId, byte flag) {
            PlayerControl player = Helpers.playerById(playerId);
            switch ((RoleId)modifierId) {
                case RoleId.Lover:
                    if (flag == 0) Lovers.lover1 = player;
                    else Lovers.lover2 = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.lover);
                    break;
                case RoleId.Blind:
                    Blind.blind = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.blind);
                    break;
                case RoleId.Bait:
                    Bait.bait = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.bait);
                    break;
                case RoleId.Sleuth:
                    Sleuth.sleuth = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.sleuth);
                    break;
                case RoleId.Tiebreaker:
                    Tiebreaker.tiebreaker = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.tiebreaker);
                    break;
                case RoleId.ButtonBarry:
                    ButtonBarry.buttonBarry = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.buttonBarry);
                    break;
                case RoleId.Indomitable:
                    Indomitable.indomitable = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.indomitable);
                    break;
                case RoleId.Drunk:
                    Drunk.drunk = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.drunk);
                    break;
                case RoleId.Sunglasses:
                    Sunglasses.sunglasses = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.sunglasses);
                    break;
                case RoleId.Torch:
                    Torch.torch = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.torch);
                    break;
                case RoleId.DoubleShot:
                    DoubleShot.doubleShot = player;
                    PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.doubleShot);
                    break;
            }
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null) {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId) {
            PlayerControl source = Helpers.playerById(sourceId);
            var t = targetId == Byte.MaxValue ? null : Helpers.playerById(targetId).Data;
            if (source != null) source.ReportDeadBody(t);
        }

        public static void uncheckedExilePlayer(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null) target.Exiled();
        }

        public static void dynamicMapOption(byte mapId) {
           GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
        }

        public static void setGameStarting() {
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
        }

        public static void shareGamemode(byte gm) {
            TOUMapOptions.gameMode = (CustomGamemodes) gm;
            LobbyViewSettingsPatch.currentButtons?.ForEach(x => x?.gameObject?.Destroy());
            LobbyViewSettingsPatch.currentButtons?.Clear();
            LobbyViewSettingsPatch.currentButtonTypes?.Clear();
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new MessageReader();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        public static void stopStart(byte playerId) {
            if (AmongUsClient.Instance.AmHost && CustomOptionHolder.anyPlayerCanStopStart.getBool()) {
                GameStartManager.Instance.ResetStartState();
                PlayerControl.LocalPlayer.RpcSendChat($"{Helpers.playerById(playerId).Data.PlayerName} stopped the game start!");
            }
        }

        public enum GhostInfoTypes {
            ChatInfo,
            PoisonerTimer,
            DeathReasonAndKiller,
        }

        public static void receiveGhostInfo (byte senderId, MessageReader reader) {
            PlayerControl sender = Helpers.playerById(senderId);

            GhostInfoTypes infoType = (GhostInfoTypes)reader.ReadByte();
            switch (infoType) {
                case GhostInfoTypes.ChatInfo:
                    string chatInfo = reader.ReadString();
                    if (Helpers.shouldShowGhostInfo())
		    	        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, chatInfo, false);
                    break;
                case GhostInfoTypes.PoisonerTimer:
                    HudManagerStartPatch.poisonerButton.Timer = (float)reader.ReadByte();
                    break;
                case GhostInfoTypes.DeathReasonAndKiller:
                    GameHistory.overrideDeathReasonAndKiller(Helpers.playerById(reader.ReadByte()), (DeadPlayer.CustomDeathReason)reader.ReadByte(), Helpers.playerById(reader.ReadByte()));
                    break;
            }
        }

        public static void initEndGameSummary()
        {
            AdditionalTempData.playerRoles.Clear();
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls) 
            {
                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo
                {
                    PlayerName     = playerControl.Data.PlayerName,
                    DeathReason    = "",
                    Roles          = null,
                    Modifiers      = null,
                    TasksTotal     = 0,
                    TasksCompleted = 0,
                    IsGuesser      = false,
                    Kills          = null,
                });
            }
        }

        public static void erasePlayerRoles(byte playerId, bool ignoreModifier = true) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;

            // Crewmate Roles
            if (player == Snitch.snitch) Snitch.clearAndReload();
            if (player == Engineer.engineer) Engineer.clearAndReload();
            if (player == Sheriff.sheriff) Sheriff.clearAndReload();
            if (player == Shifter.shifter) Shifter.clearAndReload();
            if (player == Spy.spy) Spy.clearAndReload();
            if (player == Swapper.swapper) Swapper.clearAndReload();
            if (player == Mayor.mayor) Mayor.clearAndReload();
            if (player == Medic.medic) Medic.clearAndReload();
            if (player == Investigator.investigator) Investigator.clearAndReload();
            if (player == Veteran.veteran) Veteran.clearAndReload();
            if (player == Seer.seer) Seer.clearAndReload();
            if (player == Trapper.trapper) Trapper.clearAndReload();

            // Neutral Roles
            if (player == Jester.jester) Jester.clearAndReload();
            if (player == Scavenger.scavenger) Scavenger.clearAndReload();
            if (player == Executioner.executioner) Executioner.clearAndReload();
            if (player == Lawyer.lawyer) Lawyer.clearAndReload();
            if (player == Pursuer.pursuer) Pursuer.clearAndReload();
            if (player == GuardianAngel.guardianAngel) GuardianAngel.clearAndReload();
            if (player == Survivor.survivor) Survivor.clearAndReload();
            if (player == Amnesiac.amnesiac) Amnesiac.clearAndReload();
            if (player == Mercenary.mercenary) Mercenary.clearAndReload();
            if (player == Doomsayer.doomsayer) Doomsayer.clearAndReload();

            // Neutral Killing Roles
            if (player == Dracula.dracula) { // Promote Vampire and hence override the the Dracula or erase Dracula
                if (Vampire.vampire != null && !Vampire.vampire.Data.IsDead) {
                    RPCProcedure.vampirePromotes();
                } else {
                    Dracula.clearAndReload();
                }
            }
            if (player == Vampire.vampire) Vampire.clearAndReload();
            if (player == FallenAngel.fallenAngel) FallenAngel.clearAndReload();
            if (player == Juggernaut.juggernaut) Juggernaut.clearAndReload();

            // Impostor Roles
            if (player == Morphling.morphling) Morphling.clearAndReload();
            if (player == Camouflager.camouflager) Camouflager.clearAndReload();
            if (player == Poisoner.poisoner) Poisoner.clearAndReload();
            if (player == Swooper.swooper) Swooper.clearAndReload();
            if (player == Blackmailer.blackmailer) Blackmailer.clearAndReload();
            if (player == Escapist.escapist) Escapist.clearAndReload();
            if (player == Miner.miner) Miner.clearAndReload();
            if (player == Cleaner.cleaner) Cleaner.clearAndReload();
            if (player == Phantom.phantom) Phantom.clearAndReload();
            if (player == Grenadier.grenadier) Grenadier.clearAndReload();
            
            // Double Roles
            if (Guesser.isGuesser(player.PlayerId)) Guesser.clear(player.PlayerId);
            
            if (!ignoreModifier) {
                if (player == Lovers.lover1 || player == Lovers.lover2) Lovers.clearAndReload(); // The whole Lover couple is being erased
                if (player == Blind.blind) Blind.clearAndReload();
                if (player == Bait.bait) Bait.clearAndReload();
                if (player == Sleuth.sleuth) Sleuth.clearAndReload();
                if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
                if (player == ButtonBarry.buttonBarry) ButtonBarry.clearAndReload();
                if (player == Indomitable.indomitable) Indomitable.clearAndReload();
                if (player == Drunk.drunk) Drunk.clearAndReload();
                if (player == Sunglasses.sunglasses) Sunglasses.clearAndReload();
                if (player == Torch.torch) Torch.clearAndReload();
                if (player == DoubleShot.doubleShot) DoubleShot.clearAndReload();
            }
        }

        public static void morphlingMorph(byte playerId) {  
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;

            Morphling.morphTimer = Morphling.duration;
            Morphling.morphTarget = target;
            if (Camouflager.camouflageTimer <= 0f)
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void camouflagerCamouflage(byte setTimer) {
            if (Helpers.isActiveCamoComms() && setTimer != 2) return;
            if (Helpers.isCamoComms()) Camouflager.camoComms = true;
            if (Camouflager.camouflager == null && !Camouflager.camoComms) return;

            if (setTimer == 1) Camouflager.camouflageTimer = Camouflager.duration;
            if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.setLook("", 6, "", "", "", "");
        }

        public static void engineerFixLights() {
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void engineerUsedRepair() {
            Engineer.remainingFixes--;
            if (Helpers.shouldShowGhostInfo()) {
                Helpers.showFlash(Engineer.color, 0.5f, "Engineer Fix"); ;
            }
        }

        public static void engineerUsedDoors() {
            if (Helpers.shouldShowGhostInfo()) {
                Helpers.showFlash(Engineer.color, 0.5f, "Engineer Open Doors"); ;
            }
            foreach (var door in ShipStatus.Instance.AllDoors) {
                if (!(door.Room is SystemTypes.Lounge or SystemTypes.Decontamination)) {
                    door.SetDoorway(true);
                }
            }
        }

        public static void shareSheriffSelfShot(byte killerId) {
            if (Sheriff.sheriff == null) return;
            PlayerControl target = Helpers.playerById(killerId);
            overrideDeathReasonAndKiller(Sheriff.sheriff, DeadPlayer.CustomDeathReason.SheriffMissfire, target);
        }

        public static void shifterShift(byte targetId) {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            Shifter.clearAndReload();

            if (player.isEvil()) {
                oldShifter.Exiled();
                GameHistory.overrideDeathReasonAndKiller(oldShifter, DeadPlayer.CustomDeathReason.Shift, player);
                if (oldShifter == Lawyer.target && AmongUsClient.Instance.AmHost && Lawyer.lawyer != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerPromotesToPursuer();
                }
                if (oldShifter == Executioner.target && AmongUsClient.Instance.AmHost && Executioner.executioner != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExecutionerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.executionerPromotesToPursuer();
                }
                if (oldShifter == GuardianAngel.target && AmongUsClient.Instance.AmHost && GuardianAngel.guardianAngel != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuardianAngelPromotes, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guardianAngelPromotes();
                }
                return;
            }

            setRole((byte)RoleInfo.getRoleInfoForPlayer(player, false).FirstOrDefault().roleId, oldShifter.PlayerId);
            erasePlayerRoles(player.PlayerId);
            PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.crewmate);

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void setFutureShifted(byte playerId) {
            Shifter.futureShift = Helpers.playerById(playerId);
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId) {
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (Lawyer.target != null && dyingTarget == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            if (Executioner.target != null && dyingTarget == Executioner.target) Executioner.targetWasGuessed = true;
            PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check
            if (Lawyer.target != null && dyingLoverPartner == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            if (Executioner.target != null && dyingLoverPartner == Executioner.target) Executioner.targetWasGuessed = true;

            PlayerControl guesser = Helpers.playerById(killerId);

            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer.PlayerId == killerId && dyingTargetId != killerId) {
                Doomsayer.guessedToWin++;
                if (Doomsayer.guessedToWin == Doomsayer.guessesToWin)
                    Doomsayer.triggerDoomsayerWin = true;
            }

            dyingTarget.Exiled();
            GameHistory.overrideDeathReasonAndKiller(dyingTarget, DeadPlayer.CustomDeathReason.Guess, guesser);
            byte partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

            HandleGuesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (MeetingHud.Instance) {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates) {
                    if (pva.TargetPlayerId == dyingTargetId || pva.TargetPlayerId == partnerId) {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                        MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, pva.TargetPlayerId);
                    }

                    //Give players back their vote if target is shot dead
                    if (pva.VotedFor != dyingTargetId && pva.VotedFor != partnerId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();
                }
                if (AmongUsClient.Instance.AmHost) 
                    MeetingHud.Instance.CheckForEndVoting();
            }

            if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget) {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                } else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner) {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
            
            // remove shoot button from targets for all guessers and close their guesserUI
            if (HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer != guesser && !PlayerControl.LocalPlayer.Data.IsDead && HandleGuesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance) {
                MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                if (dyingLoverPartner != null)
                    MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingLoverPartner.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null) {
                    if (MeetingHudPatch.guesserCurrentTarget == dyingTarget.PlayerId)
                        MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                    else if (dyingLoverPartner != null && MeetingHudPatch.guesserCurrentTarget == dyingLoverPartner.PlayerId)
                        MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
            }

            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null && guesser != null) {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                string msg = $"{guesser.Data.PlayerName} guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg, false);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
        }

        public static void setGuesserGm (byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            new GuesserGM(target);
        }

        public static void swapperSwap(byte playerId1, byte playerId2) {
            if (MeetingHud.Instance) {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void setTiebreak() {
            Tiebreaker.isTiebreak = true;
        }

        public static void mayorReveal() {
            Mayor.isRevealed = true;

            MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == Mayor.mayor.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
            if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserCurrentTarget == Mayor.mayor.PlayerId)
                MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
            MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("RevealButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("RevealButton").gameObject); });
        }

        public static void medicSetShielded(byte shieldedId) {
            Medic.usedShield = true;
            Medic.shielded = Helpers.playerById(shieldedId);
        }

        public static void shieldedMurderAttempt() {
            if (Medic.shielded == null || Medic.medic == null) return;

            bool getsNotification = Medic.getsNotification == 0 // Everyone
                || Medic.getsNotification == 1 & PlayerControl.LocalPlayer == Medic.shielded // Shielded
                || Medic.getsNotification == 2 & PlayerControl.LocalPlayer == Medic.medic // Medic
                || Medic.getsNotification == 3 && (PlayerControl.LocalPlayer ==  Medic.shielded || PlayerControl.LocalPlayer == Medic.medic);
            if (getsNotification || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Medic.color, 1f, "Failed Murder Attempt on Shielded Player");
        }

        public static void draculaCreatesVampire(byte targetId) {
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null) return;
            if (Executioner.target == player && Executioner.executioner != null && !Executioner.executioner.Data.IsDead) {
                Lawyer.lawyer = Executioner.executioner;
                Lawyer.target = Executioner.target;
                PlayerGameInfo.AddRole(Executioner.executioner.PlayerId, RoleInfo.lawyer);
                Executioner.clearAndReload();
            }

            if (Dracula.canCreateVampireFromImpostor && player.Data.Role.IsImpostor) {
                Dracula.fakeVampire = player;
            } else {
                bool wasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                if (player == Lawyer.lawyer && Lawyer.target != null) {
                    Transform playerInfoTransform = Lawyer.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == Executioner.executioner && Executioner.target != null) {
                    Transform playerInfoTransform = Executioner.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == GuardianAngel.guardianAngel && GuardianAngel.target != null) {
                    Transform playerInfoTransform = GuardianAngel.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == Swooper.swooper && Swooper.isInvisble) {
                    swooperSwoop(Swooper.swooper.PlayerId, byte.MaxValue);
                }
                if (player == Phantom.phantom && Phantom.isInvisble) {
                    phantomInvis(Phantom.phantom.PlayerId, byte.MaxValue);
                }
                erasePlayerRoles(player.PlayerId, true);
                setRole((byte)RoleId.Vampire, player.PlayerId);
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasImpostor) Vampire.wasTeamRed = true;
                Vampire.wasImpostor = wasImpostor;
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeNewVampCanAssassinate.getBool())
                    setGuesserGm(targetId);
                Dracula.numberOfAllVampires++;
            }
            Dracula.canCreateVampire = false;
        }

        public static void vampirePromotes() {
            Dracula.removeCurrentDracula();
            PlayerGameInfo.AddRole(Vampire.vampire.PlayerId, RoleInfo.dracula);
            Dracula.dracula = Vampire.vampire;
            if (Dracula.numberOfAllVampires < Dracula.maxVampires) Dracula.canCreateVampire = true;
            Dracula.wasTeamRed = Vampire.wasTeamRed;
            Dracula.wasImpostor = Vampire.wasImpostor;
            Vampire.clearAndReload();
            return;
        }

        public static void poisonerSetPoisoned(byte targetId, byte performReset) {
            if (performReset != 0) {
                Poisoner.poisoned = null;
                return;
            }
            if (Poisoner.poisoner == null) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId && !player.Data.IsDead)
                    Poisoner.poisoned = player;
        }

        public static void cleanBody(byte playerId, byte cleaningPlayerId) {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId) {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }     
            }
            if (Scavenger.scavenger != null && cleaningPlayerId == Scavenger.scavenger.PlayerId) {
                Scavenger.eatenBodies++;
                if (Scavenger.eatenBodies == Scavenger.scavengerNumberToWin) {
                    Scavenger.triggerScavengerWin = true;
                }
            }
        }

        public static void executionerSetTarget(byte playerId) {
            Executioner.target = Helpers.playerById(playerId);
        }

        public static void executionerPromotesToPursuer() {
            PlayerControl player = Executioner.executioner;
            PlayerControl target = Executioner.target;
            Executioner.clearAndReload(false);

            Pursuer.pursuer = player;
            PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.pursuer);

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && target != null) {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void lawyerSetTarget(byte playerId) {
            Lawyer.target = Helpers.playerById(playerId);
        }

        public static void lawyerPromotesToPursuer() {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;
            Lawyer.clearAndReload(false);

            Pursuer.pursuer = player;
            PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.pursuer);

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null) {
                Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void setBlanked(byte playerId, byte value) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);
        }

        public static void guardianAngelSetTarget(byte playerId) {
            GuardianAngel.target = Helpers.playerById(playerId);
        }

        public static void guardianAngelProtect() {
            GuardianAngel.remainingProtects--;
            GuardianAngel.protectActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GuardianAngel.duration, new Action<float>((p) => {
                if (p == 1f) GuardianAngel.protectActive = false;
            })));
        }

        public static void guardianAngelPromotes() {
            PlayerControl player = GuardianAngel.guardianAngel;
            PlayerControl target = GuardianAngel.target;
            GuardianAngel.clearAndReload(false);

            if (GuardianAngel.promoteToFA) {
                FallenAngel.fallenAngel = player;
                FallenAngel.target = target;
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.fallenAngel);
            } else {
                Survivor.survivor = player;
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.survivor);
            }

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && target != null) {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void guardianToFa() {
            GuardianAngel.promoteToFA = true;
        }

        public static void guardianToSurv() {
            GuardianAngel.promoteToFA = false;
        }

        public static void survivorSafeguard() {
            Survivor.remainingSafeguards--;
            Survivor.safeguardActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Survivor.duration, new Action<float>((p) => {
                if (p == 1f) Survivor.safeguardActive = false;
            })));
        }

        public static void amnesiacRemember(byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl oldAmnesiac = Amnesiac.amnesiac;
            if (target == null) return;

            Amnesiac.clearAndReload();

            RoleId targetRoleId = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().roleId;
            erasePlayerRoles(target.PlayerId);
            if (targetRoleId == RoleId.Crewmate) {
                PlayerGameInfo.AddRole((byte)oldAmnesiac.PlayerId, RoleInfo.crewmate);
            } else if (targetRoleId == RoleId.Impostor) {
                Helpers.turnToImpostor(oldAmnesiac);
                PlayerGameInfo.AddRole((byte)oldAmnesiac.PlayerId, RoleInfo.impostor);
            } else {
                if (target.Data.Role.IsImpostor) {
                    Helpers.turnToImpostor(oldAmnesiac);
                    setRole((byte)targetRoleId, oldAmnesiac.PlayerId);
                    PlayerGameInfo.AddRole((byte)target.PlayerId, RoleInfo.impostor);
                } else {
                    setRole((byte)targetRoleId, oldAmnesiac.PlayerId);
                    PlayerGameInfo.AddRole((byte)target.PlayerId, RoleInfo.crewmate);
                }
            }

            if (oldAmnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
        }

        public static void investigatorWatchPlayer(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            Investigator.watching = target;
        }

        public static void investigatorWatchFlash(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            if (PlayerControl.LocalPlayer == Investigator.investigator || Helpers.shouldShowGhostInfo()) {
                if (Investigator.seeFlashColor) {
                    Helpers.showFlash(Palette.PlayerColors[target.Data.DefaultOutfit.ColorId], 1f, "Watching player used ability");
                }
                else {
                    Helpers.showFlash(Investigator.color, 1f, "Watching player used ability");
                }
            }
        }

        public static void veteranAlert() {
            Veteran.remainingAlerts--;
            Veteran.isAlertActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Veteran.duration, new Action<float>((p) => {
                if (p == 1f) Veteran.isAlertActive = false;
            })));
        }

        public static void swooperSwoop(byte playerId, byte flag) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue) {
                target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
                target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
                target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);

                if (Camouflager.camouflageTimer <= 0 && !Helpers.isActiveCamoComms() && !Helpers.MushroomSabotageActive()) target.setDefaultLook();
                Swooper.isInvisble = false;
                return;
            }

            target.setLook("", 6, "", "", "", "");
            Color color = Color.clear;
            bool canSee = PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead;
            if (canSee) color.a = 0.1f;
            target.cosmetics.currentBodySprite.BodySprite.color = color;
            target.cosmetics.colorBlindText.gameObject.SetActive(false);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(canSee ? 0.1f : 0f);
            Swooper.invisibleTimer = Swooper.swoopDuration;
            Swooper.isInvisble = true;
        }

        public static void mercenaryShield(byte shieldedId) {
            Mercenary.shielded = Helpers.playerById(shieldedId);
        }

        public static void mercenaryResetShield() {
            Mercenary.shielded = null;
        }

        public static void mercenaryAddMurder() {
            Mercenary.attemptedMurder++;
            if (Mercenary.numOfNeededAttempts == Mercenary.attemptedMurder)
                Mercenary.triggerMercenaryWin = true;
        }

        public static void blackmailPlayer(byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            Blackmailer.blackmailed = target;
        }

        public static void unblackmailPlayer() {
            Blackmailer.blackmailed = null;
            Blackmailer.alreadyShook = false;
        }

        public static void escapistMarkLocation(byte[] buff) {
            if (Escapist.escapist == null) return;
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            Escapist.markLocation(position);
        }

        public static void escapistRecall(bool isFirstJump, byte[] buff) {
            if (Escapist.escapist == null || Escapist.markedLocation == null) return;
            var markedPos = (Vector3)Escapist.markedLocation;
            Escapist.escapist.NetTransform.SnapTo(markedPos);

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            // Create Silhoutte At Start Position:
            if (isFirstJump) {
                Escapist.markLocation(position);
            } else {
                Escapist.markedLocation = null;
            }
        }

        public static void placeVent(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            new MinerVent(position);
        }

        public static void showIndomitableFlash() {
            if (Indomitable.indomitable == null) return;

            bool getsNotification = PlayerControl.LocalPlayer == Indomitable.indomitable;
            if (getsNotification || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Medic.color, 1f, "Failed Murder Attempt on Shielded Player");
        }

        public static void phantomInvis(byte playerId, byte flag) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue) {
                target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
                target.cosmetics.nameText.gameObject.SetActive(true);
                target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
                target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);
                target.Collider.enabled = true;

                if (Camouflager.camouflageTimer <= 0 && !Helpers.isActiveCamoComms() && !Helpers.MushroomSabotageActive()) target.setDefaultLook();
                Phantom.isInvisble = false;
                return;
            }

            target.setLook("", target.Data.DefaultOutfit.ColorId, "", "", "", "");
            Color color = Palette.PlayerColors[target.Data.DefaultOutfit.ColorId];
            bool canSee = PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead;
            if (canSee) color.a = 0.8f;
            else color.a = 0f;
            target.cosmetics.currentBodySprite.BodySprite.color = color;
            target.cosmetics.nameText.gameObject.SetActive(false);
            target.cosmetics.colorBlindText.gameObject.SetActive(false);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(canSee ? 0.1f : 0f);
            target.Collider.enabled = false;
            Phantom.ghostTimer = Phantom.duration;
            Phantom.isInvisble = true;
        }

        public static void grenadierFlash(bool clear = false) {
            if (clear) {
                Grenadier.flashedPlayers.Clear();
                return;
            }

            var closestPlayers = Helpers.getClosestPlayers(Grenadier.grenadier.GetTruePosition(), Grenadier.radius == 0 ? 100f : Grenadier.radius * 0.25f, true);
            Grenadier.flashedPlayers = closestPlayers;
            foreach (PlayerControl player in closestPlayers) {
                if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId) {
                    if (player.Data.Role.IsImpostor && !player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 0.5f);
                    else if (!player.Data.Role.IsImpostor && !player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 1f);
                    else if (player.Data.IsDead && !MeetingHud.Instance)
                        Helpers.grenadierFlash(Grenadier.flash, Grenadier.duration, 0.5f);
                }
            }
        }

        public static void doomsayerObserve(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            Doomsayer.observedPlayers.Add(target);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix([HarmonyArgument(0)]byte callId, [HarmonyArgument(1)]MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId) {
                // Main Controls
                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptions:
                    RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.SetRole:
                    RPCProcedure.setRole(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetModifier:
                    RPCProcedure.setModifier(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.UncheckedMurderPlayer:
                    RPCProcedure.uncheckedMurderPlayer(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.UncheckedExilePlayer:
                    RPCProcedure.uncheckedExilePlayer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                    RPCProcedure.uncheckedCmdReportDeadBody(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.DynamicMapOption:
                    RPCProcedure.dynamicMapOption(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetGameStarting:
                    RPCProcedure.setGameStarting();
                    break;
                case (byte)CustomRPC.ShareGamemode:
                    RPCProcedure.shareGamemode(reader.ReadByte());
                    break;
                case (byte)CustomRPC.StopStart:
                    RPCProcedure.stopStart(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    RPCProcedure.useUncheckedVent(reader.ReadPackedInt32(), reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShareGhostInfo:
                    RPCProcedure.receiveGhostInfo(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.InitGameSummary:
                    RPCProcedure.initEndGameSummary();
                    break;

                // Role functionality
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(reader.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage(reader.ReadByte());
                    break;
                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.EngineerUsedDoors:
                    RPCProcedure.engineerUsedDoors();
                    break;
                case (byte)CustomRPC.ShareSheriffSelfShot:
                    RPCProcedure.shareSheriffSelfShot(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureShifted:
                    RPCProcedure.setFutureShifted(reader.ReadByte());
                    break;
                case (byte)CustomRPC.GuesserShoot:
                    RPCProcedure.guesserShoot(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    RPCProcedure.swapperSwap(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetTiebreak:
                    RPCProcedure.setTiebreak();
                    break;
                case (byte)CustomRPC.MayorReveal:
                    RPCProcedure.mayorReveal();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.DraculaCreatesVampire:
                    RPCProcedure.draculaCreatesVampire(reader.ReadByte());
                    break;
                case (byte)CustomRPC.VampirePromotes:
                    RPCProcedure.vampirePromotes();
                    break;
                case (byte)CustomRPC.PoisonerSetPoisoned:
                    RPCProcedure.poisonerSetPoisoned(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.CleanBody:
                    RPCProcedure.cleanBody(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ExecutionerSetTarget:
                    RPCProcedure.executionerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.ExecutionerPromotesToPursuer:
                    RPCProcedure.executionerPromotesToPursuer();
                    break;
                case (byte)CustomRPC.LawyerSetTarget:
                    RPCProcedure.lawyerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.LawyerPromotesToPursuer:
                    RPCProcedure.lawyerPromotesToPursuer();
                    break;
                case (byte)CustomRPC.SetBlanked:
                    RPCProcedure.setBlanked(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.GuardianAngelSetTarget:
                    RPCProcedure.guardianAngelSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.GuardianAngelProtect:
                    RPCProcedure.guardianAngelProtect();
                    break;
                case (byte)CustomRPC.GuardianAngelPromotes:
                    RPCProcedure.guardianAngelPromotes();
                    break;
                case (byte)CustomRPC.GuardianToFa:
                    RPCProcedure.guardianToFa();
                    break;
                case (byte)CustomRPC.GuardianToSurv:
                    RPCProcedure.guardianToSurv();
                    break;
                case (byte)CustomRPC.SurvivorSafeguard:
                    RPCProcedure.survivorSafeguard();
                    break;
                case (byte)CustomRPC.AmnesiacRemember:
                    RPCProcedure.amnesiacRemember(reader.ReadByte());
                    break;
                case (byte)CustomRPC.InvestigatorWatchPlayer:
                    RPCProcedure.investigatorWatchPlayer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.InvestigatorWatchFlash:
                    RPCProcedure.investigatorWatchFlash(reader.ReadByte());
                    break;
                case (byte)CustomRPC.VeteranAlert:
                    RPCProcedure.veteranAlert();
                    break;
                case (byte)CustomRPC.SwooperSwoop:
                    RPCProcedure.swooperSwoop(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.MercenaryShield:
                    RPCProcedure.mercenaryShield(reader.ReadByte());
                    break;
                case (byte)CustomRPC.MercenaryResetShield:
                    RPCProcedure.mercenaryResetShield();
                    break;
                case (byte)CustomRPC.MercenaryAddMurder:
                    RPCProcedure.mercenaryAddMurder();
                    break;
                case (byte)CustomRPC.BlackmailPlayer:
                    RPCProcedure.blackmailPlayer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UnblackmailPlayer:
                    RPCProcedure.unblackmailPlayer();
                    break;
                case (byte)CustomRPC.EscapistMarkLocation:
                    RPCProcedure.escapistMarkLocation(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.EscapistRecall:
                    RPCProcedure.escapistRecall(reader.ReadByte() == byte.MaxValue, reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.PlaceVent:
                    RPCProcedure.placeVent(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.ShowIndomitableFlash:
                    RPCProcedure.showIndomitableFlash();
                    break;
                case (byte)CustomRPC.PhantomInvis:
                    RPCProcedure.phantomInvis(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.GrenadierFlash:
                    RPCProcedure.grenadierFlash(reader.ReadBoolean());
                    break;
                case (byte)CustomRPC.DoomsayerObserve:
                    RPCProcedure.doomsayerObserve(reader.ReadByte());
                    break;

                // Game mode
                case (byte)CustomRPC.SetGuesserGm:
                    byte guesserGm = reader.ReadByte();
                    RPCProcedure.setGuesserGm(guesserGm);
                    break;
            }
        }
    }
}