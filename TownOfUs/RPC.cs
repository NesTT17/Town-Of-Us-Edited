using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs
{
    public enum RoleId
    {
        // Special Roles
        Sheriff,
        Jester,
        Survivor,
        Juggernaut,
        Seer,
        Engineer,
        Snitch,
        Veteran,
        Camouflager,
        Morphling,
        Pursuer,
        Amnesiac,
        Thief,
        Lawyer,
        Executioner,
        Medic,
        Swapper,
        Investigator,
        Spy,
        Vigilante,
        Assassin,
        Tracker,
        Trapper,
        Detective,
        Mystic,
        GuardianAngel,
        Swooper,
        Arsonist,
        Werewolf,
        Miner,
        Janitor,
        Undertaker,
        Grenadier,
        Blackmailer,
        Politician, Mayor,
        Dracula, Vampire,
        Poisoner,
        Venerer,
        Plaguebearer, Pestilence,
        Doomsayer,
        Glitch,
        Cannibal,
        Scavenger,
        Escapist,
        VampireHunter,
        Oracle,
        Lookout,
        Plumber,
        // Modifiers
        Bait,
        Blind,
        ButtonBarry,
        Shy,
        Flash,
        Mini,
        Indomitable,
        Lover,
        Multitasker,
        Radar,
        Sleuth,
        Tiebreaker,
        Torch,
        Vip,
        Drunk,
        Immovable,
        DoubleShot,
        Ruthless,
        Underdog,
        Saboteur,
        Frosty,
        Satelite,
        SixthSense,
        Taskmaster,
        Disperser,
        // Default Roles
        Crewmate, Impostor,
        None
    }

    public enum CustomRPC
    {
        // Main Controls
        ResetVariables = 100,
        ShareOptions,
        ForceEnd,
        SetRole,
        SetModifier,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,
        DynamicMapOption,
        SetGameStarting,
        ShareGamemode,
        StopStart,
        AddGameInfo,

        // Role functionality
        SetJesterWinner = 120,
        SurvivorSafeguard,
        EngineerFixLights,
        EngineerUsedRepair,
        ShareRoom,
        VeteranAlert,
        CamouflagerCamouflage,
        MorphlingMorph,
        SetBlanked,
        AmnesiacTakeRole,
        ThiefStealsRole,
        LawyerSetTarget,
        LawyerPromotes,
        ExecutionerSetTarget,
        ExecutionerPromotes,
        MedicSetShielded,
        ShieldedMurderAttempt,
        SetFutureShielded,
        SwapperSwap,
        GuesserShoot,
        GuardianAngelProtect,
        GuardianAngelSetTarget,
        GuardianAngelPromotes,
        SwooperSwoop,
        WerewolfRampage,
        PlaceVent,
        CleanBody,
        DragBody,
        DropBody,
        GrenadierFlash,
        BlackmailPlayer,
        UnblackmailPlayer,
        PoliticianCampaign,
        PoliticianTurnMayor,
        MayorBodyguard,
        DraculaCreatesVampire,
        VampirePromotes,
        PoisonerSetPoisoned,
        VenererCamouflage,
        PlaguebearerInfect,
        PlaguebearerTurnPestilence,
        GlitchMimic,
        GlitchHack,
        ShowIndomitableFlash,
        SetTiebreak,
        SixthSenseAbilityTrigger,
        SetPosition,
        VampireHunterPromotes,
        OracleSetConfessor,
        OracleConfess,
        SealVent,
        PlumberFlush,
        DisperserDisperse,

        // Other functionality
        ShareGhostInfo,
        SetGuesserGm,
        SetFirstKill,
    }

    public static class RPCProcedure
    {
        // Main Controls
        public static void resetVariables()
        {
            MinerVent.clearMinerVents();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
            CustomButton.ReloadHotkeys();
            reloadPluginOptions();
            Helpers.toggleZoom(reset: true);
            GameStartManagerUpdatePatch.startingTimer = 0;
            MapBehaviourPatch.clearAndReload();
            HudManagerUpdate.CloseSummary();
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

        public static void forceEnd()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor)
                {

                    GameData.Instance.GetPlayerById(player.PlayerId); // player.RemoveInfected(); (was removed in 2022.12.08, no idea if we ever need that part again, replaced by these 2 lines.) 
                    player.CoSetRole(RoleTypes.Crewmate, true);

                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void setRole(byte roleId, byte playerId)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == playerId)
                {
                    switch ((RoleId)roleId)
                    {
                        case RoleId.Sheriff:
                            _ = new Sheriff(player);
                            break;
                        case RoleId.Jester:
                            _ = new Jester(player);
                            break;
                        case RoleId.Survivor:
                            _ = new Survivor(player);
                            break;
                        case RoleId.Juggernaut:
                            Juggernaut.juggernaut = player;
                            break;
                        case RoleId.Seer:
                            Seer.seer = player;
                            break;
                        case RoleId.Engineer:
                            Engineer.engineer = player;
                            break;
                        case RoleId.Snitch:
                            Snitch.snitch = player;
                            break;
                        case RoleId.Veteran:
                            _ = new Veteran(player);
                            break;
                        case RoleId.Camouflager:
                            Camouflager.camouflager = player;
                            break;
                        case RoleId.Morphling:
                            Morphling.morphling = player;
                            break;
                        case RoleId.Pursuer:
                            _ = new Pursuer(player);
                            break;
                        case RoleId.Amnesiac:
                            _ = new Amnesiac(player);
                            break;
                        case RoleId.Thief:
                            _ = new Thief(player);
                            break;
                        case RoleId.Lawyer:
                            Lawyer.lawyer = player;
                            break;
                        case RoleId.Executioner:
                            Executioner.executioner = player;
                            break;
                        case RoleId.Medic:
                            Medic.medic = player;
                            break;
                        case RoleId.Swapper:
                            Swapper.swapper = player;
                            break;
                        case RoleId.Investigator:
                            Investigator.investigator = player;
                            break;
                        case RoleId.Spy:
                            Spy.spy = player;
                            break;
                        case RoleId.Vigilante:
                            Vigilante.vigilante = player;
                            break;
                        case RoleId.Assassin:
                            Assassin.assassin = player;
                            break;
                        case RoleId.Tracker:
                            Tracker.tracker = player;
                            break;
                        case RoleId.Trapper:
                            Trapper.trapper = player;
                            break;
                        case RoleId.Detective:
                            Detective.detective = player;
                            break;
                        case RoleId.Mystic:
                            Mystic.mystic = player;
                            break;
                        case RoleId.GuardianAngel:
                            GuardianAngel.guardianAngel = player;
                            break;
                        case RoleId.Swooper:
                            Swooper.swooper = player;
                            break;
                        case RoleId.Arsonist:
                            Arsonist.arsonist = player;
                            break;
                        case RoleId.Werewolf:
                            Werewolf.werewolf = player;
                            break;
                        case RoleId.Miner:
                            Miner.miner = player;
                            break;
                        case RoleId.Janitor:
                            Janitor.janitor = player;
                            break;
                        case RoleId.Undertaker:
                            Undertaker.undertaker = player;
                            break;
                        case RoleId.Grenadier:
                            Grenadier.grenadier = player;
                            break;
                        case RoleId.Blackmailer:
                            Blackmailer.blackmailer = player;
                            break;
                        case RoleId.Politician:
                            Politician.politician = player;
                            Politician.campaignedPlayers.Add(player);
                            break;
                        case RoleId.Mayor:
                            Mayor.mayor = player;
                            break;
                        case RoleId.Dracula:
                            Dracula.dracula = player;
                            break;
                        case RoleId.Vampire:
                            Vampire.vampire = player;
                            break;
                        case RoleId.Poisoner:
                            Poisoner.poisoner = player;
                            break;
                        case RoleId.Venerer:
                            Venerer.venerer = player;
                            break;
                        case RoleId.Plaguebearer:
                            Plaguebearer.plaguebearer = player;
                            Plaguebearer.infectedPlayers.Add(player);
                            break;
                        case RoleId.Pestilence:
                            Pestilence.pestilence = player;
                            break;
                        case RoleId.Doomsayer:
                            Doomsayer.doomsayer = player;
                            break;
                        case RoleId.Glitch:
                            Glitch.glitch = player;
                            break;
                        case RoleId.Cannibal:
                            Cannibal.cannibal = player;
                            break;
                        case RoleId.Scavenger:
                            Scavenger.scavenger = player;
                            break;
                        case RoleId.Escapist:
                            Escapist.escapist = player;
                            break;
                        case RoleId.VampireHunter:
                            VampireHunter.vampireHunter = player;
                            if (!VampireHunter.canStakeRoundOne) VampireHunter.canStake = false;
                            else VampireHunter.canStake = true;
                            break;
                        case RoleId.Oracle:
                            Oracle.oracle = player;
                            break;
                        case RoleId.Lookout:
                            Lookout.lookout = player;
                            break;
                        case RoleId.Plumber:
                            Plumber.plumber = player;
                            break;
                    }
                    PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.roleInfoById[(RoleId)roleId]);
                }
            }
        }

        public static void setModifier(byte modifierId, byte playerId, byte flag)
        {
            PlayerControl player = Helpers.playerById(playerId);
            switch ((RoleId)modifierId)
            {
                case RoleId.Bait:
                    Bait.bait.Add(player);
                    break;
                case RoleId.Blind:
                    Blind.blind = player;
                    break;
                case RoleId.ButtonBarry:
                    ButtonBarry.buttonBarry = player;
                    break;
                case RoleId.Shy:
                    Shy.shy.Add(player);
                    break;
                case RoleId.Flash:
                    Flash.flash.Add(player);
                    break;
                case RoleId.Mini:
                    Mini.mini = player;
                    break;
                case RoleId.Indomitable:
                    Indomitable.indomitable = player;
                    break;
                case RoleId.Lover:
                    if (flag == 0) Lovers.lover1 = player;
                    else Lovers.lover2 = player;
                    break;
                case RoleId.Multitasker:
                    Multitasker.multitasker.Add(player);
                    break;
                case RoleId.Radar:
                    Radar.radar = player;
                    break;
                case RoleId.Sleuth:
                    Sleuth.sleuth = player;
                    break;
                case RoleId.Tiebreaker:
                    Tiebreaker.tiebreaker = player;
                    break;
                case RoleId.Torch:
                    Torch.torch.Add(player);
                    break;
                case RoleId.Vip:
                    Vip.vip.Add(player);
                    break;
                case RoleId.Drunk:
                    Drunk.drunk.Add(player);
                    break;
                case RoleId.Immovable:
                    Immovable.immovable.Add(player);
                    break;
                case RoleId.DoubleShot:
                    DoubleShot.doubleShot = player;
                    break;
                case RoleId.Ruthless:
                    Ruthless.ruthless = player;
                    break;
                case RoleId.Underdog:
                    Underdog.underdog = player;
                    break;
                case RoleId.Saboteur:
                    Saboteur.saboteur = player;
                    break;
                case RoleId.Frosty:
                    Frosty.frosty = player;
                    break;
                case RoleId.Satelite:
                    Satelite.satelite = player;
                    break;
                case RoleId.SixthSense:
                    SixthSense.sixthSense = player;
                    break;
                case RoleId.Taskmaster:
                    Taskmaster.taskMaster = player;
                    break;
                case RoleId.Disperser:
                    Disperser.disperser = player;
                    break;
            }
            PlayerGameInfo.AddModifier(player.PlayerId, RoleInfo.roleInfoById[(RoleId)modifierId]);
        }

        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            if (!(bool)GameStartManagerPatch.playerVersions?.ContainsKey(clientId))
            {
                GameStartManagerPatch.versionSent = false;  // Force a resend of own data
                GameStartManagerUpdatePatch.sendGamemode = true;
            }
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter)
        {
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
            TOUEdMapOptions.gameMode = (CustomGamemodes)gm;
            LobbyViewSettingsPatch.currentButtons?.ForEach(x => x.gameObject?.Destroy());
            LobbyViewSettingsPatch.currentButtons?.Clear();
            LobbyViewSettingsPatch.currentButtonTypes?.Clear();
        }

        public static void stopStart(byte playerId)
        {
            if (!CustomOptionHolder.anyPlayerCanStopStart.getBool())
                return;
            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
            if (AmongUsClient.Instance.AmHost)
            {
                GameStartManager.Instance.ResetStartState();
                PlayerControl.LocalPlayer.RpcSendChat($"{Helpers.playerById(playerId).Data.PlayerName} stopped the game start!");
            }
        }

        public static void addGameInfo(byte playerId, InfoType info)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;

            if (!PlayerGameInfo.Mapping.TryGetValue(player.PlayerId, out PlayerGameInfo gameInfo))
                PlayerGameInfo.Mapping.Add(player.PlayerId, gameInfo = new());

            switch (info)
            {
                case InfoType.AddKill:
                    gameInfo.Kills++;
                    break;
                case InfoType.AddCorrectShot:
                    gameInfo.CorrectShots++;
                    break;
                case InfoType.AddMisfire:
                    gameInfo.Misfires++;
                    break;
                case InfoType.AddCorrectGuess:
                    gameInfo.CorrectGuesses++;
                    break;
                case InfoType.AddIncorrectGuess:
                    gameInfo.IncorrectGuesses++;
                    break;
                case InfoType.AddAbilityKill:
                    gameInfo.AbilityKills++;
                    break;
                case InfoType.AddEat:
                    gameInfo.Eats++;
                    break;
                case InfoType.AddClean:
                    gameInfo.Cleans++;
                    break;
            }
        }
        
        // Role functionality
        public static void setJesterWinner(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            Jester.winningJesterPlayer = player;
            Jester.triggerJesterWin = true;
        }

        public static void survivorSafeguard(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null || !player.IsSurvivor(out Survivor survivor)) return;
            survivor.safeguardActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Survivor.duration, new Action<float>((p) =>
            {
                if (p == 1f) survivor.safeguardActive = false;
            })));
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
                Helpers.showFlash(Engineer.color, 0.5f, "Engineer Fix");
            }
        }

        public static void shareRoom(byte playerId, byte roomId)
        {
            if (Snitch.playerRoomMap.ContainsKey(playerId)) Snitch.playerRoomMap[playerId] = roomId;
            else Snitch.playerRoomMap.Add(playerId, roomId);
        }

        public static void veteranAlert(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null || !player.IsVeteran(out Veteran veteran)) return;
            veteran.alertActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Veteran.duration, new Action<float>((p) =>
            {
                if (p == 1f) veteran.alertActive = false;
            })));
        }

        public static void camouflagerCamouflage()
        {
            if (Camouflager.camouflager == null) return;

            Camouflager.camouflageTimer = Camouflager.duration;
            if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                player.setLook("", 15, "", "", "", "");
                player.cosmetics.colorBlindText.text = "";
            }
        }

        public static void morphlingMorph(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;

            Morphling.morphTimer = Morphling.duration;
            Morphling.morphTarget = target;
            if (Camouflager.camouflageTimer <= 0f)
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void setBlanked(byte playerId, byte value)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);
        }

        public static void amnesiacTakeRole(byte playerId, byte amnesiacId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl amnesiac = Helpers.playerById(amnesiacId);
            if (target == null || amnesiac == null) return;
            Amnesiac.RemoveAmnesiac(amnesiac.PlayerId);

            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(target).Where(info => info.factionId != FactionId.Modifier).FirstOrDefault();
            if (roleInfo.roleId == RoleId.Crewmate || roleInfo.roleId == RoleId.Impostor)
            {
                if (roleInfo.roleId == RoleId.Crewmate)
                {
                    PlayerGameInfo.AddRole(amnesiac.PlayerId, RoleInfo.crewmate);
                }
                else if (roleInfo.roleId == RoleId.Impostor)
                {
                    Helpers.TurnToImpostor(amnesiac);
                    PlayerGameInfo.AddRole(amnesiac.PlayerId, RoleInfo.impostor);
                }
                if (amnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
                return;
            }
            else if (roleInfo.roleId == RoleId.Dracula)
            {
                setRole((byte)RoleId.Vampire, amnesiac.PlayerId);
                Dracula.formerDraculas.Add(target);
                Dracula.canCreateVampire = false;
                if (amnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            }
            else if (roleInfo.roleId == RoleId.Vampire)
            {
                setRole((byte)roleInfo.roleId, amnesiac.PlayerId);
                Dracula.formerDraculas.Add(target);
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeVampireGetsGuesser.getBool() && !HandleGuesser.isGuesser(amnesiac.PlayerId))
                    setGuesserGm(amnesiac.PlayerId);
                if (amnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            }
            else
            {
                if (target.Data.Role.IsImpostor) Helpers.TurnToImpostor(amnesiac);
                if (Amnesiac.resetRole) Helpers.erasePlayerRoles(target.PlayerId, true, true);
                setRole((byte)roleInfo.roleId, amnesiac.PlayerId);
                if (amnesiac == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            }
        }

        public static void thiefStealsRole(byte playerId, byte thiefId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl thief = Helpers.playerById(thiefId);
            if (target == null || thief == null) return;
            Thief.RemoveThief(thief.PlayerId);

            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(target).Where(info => info.factionId != FactionId.Modifier).FirstOrDefault();
            if (roleInfo.roleId == RoleId.Impostor)
            {
                Helpers.TurnToImpostor(thief);
                PlayerGameInfo.AddRole(thief.PlayerId, RoleInfo.impostor);
                if (thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
                return;
            }
            else if (roleInfo.roleId == RoleId.Dracula)
            {
                setRole((byte)roleInfo.roleId, thief.PlayerId);
                Dracula.formerDraculas.Add(target);
            }
            else if (roleInfo.roleId == RoleId.Vampire)
            {
                setRole((byte)roleInfo.roleId, thief.PlayerId);
                Dracula.formerDraculas.Add(target);
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeVampireGetsGuesser.getBool() && !HandleGuesser.isGuesser(thief.PlayerId))
                    setGuesserGm(thief.PlayerId);
            }
            else
            {
                if (target.Data.Role.IsImpostor) Helpers.TurnToImpostor(thief);
                Helpers.erasePlayerRoles(target.PlayerId, true, true);
                setRole((byte)roleInfo.roleId, thief.PlayerId);
            }
            if (Lawyer.lawyer != null && target == Lawyer.target)
                Lawyer.target = thief;
            if (Executioner.executioner != null && target == Executioner.target)
                Executioner.target = thief;
            if (GuardianAngel.guardianAngel != null && target == GuardianAngel.target)
                GuardianAngel.target = thief;
            if (thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            Thief.formerThieves.Add(thief);
        }

        public static void lawyerSetTarget(byte playerId)
        {
            Lawyer.target = Helpers.playerById(playerId);
        }

        public static void lawyerPromotes()
        {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;
            Lawyer.clearAndReload(false);

            if (Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Amnesiac)
            {
                setRole((byte)RoleId.Amnesiac, player.PlayerId);
            }
            if (Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Pursuer)
            {
                setRole((byte)RoleId.Pursuer, player.PlayerId);
            }
            if (Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Survivor)
            {
                setRole((byte)RoleId.Survivor, player.PlayerId);
            }
            if (Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Thief)
            {
                setRole((byte)RoleId.Thief, player.PlayerId);
            }
            if (Lawyer.becomeOnTargetDeath == Lawyer.BecomeOptions.Crewmate)
            {
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.crewmate);
            }

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null)
            {
                Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void executionerSetTarget(byte playerId)
        {
            Executioner.target = Helpers.playerById(playerId);
        }

        public static void executionerPromotes()
        {
            PlayerControl player = Executioner.executioner;
            PlayerControl target = Executioner.target;
            Executioner.clearAndReload(false);

            if (Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Amnesiac)
            {
                setRole((byte)RoleId.Amnesiac, player.PlayerId);
            }
            if (Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Pursuer)
            {
                setRole((byte)RoleId.Pursuer, player.PlayerId);
            }
            if (Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Survivor)
            {
                setRole((byte)RoleId.Survivor, player.PlayerId);
            }
            if (Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Thief)
            {
                setRole((byte)RoleId.Thief, player.PlayerId);
            }
            if (Executioner.becomeOnTargetDeath == Executioner.BecomeOptions.Crewmate)
            {
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.crewmate);
            }

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && target != null)
            {
                Transform playerInfoTransform = target.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void medicSetShielded(byte shieldedId)
        {
            Medic.usedShield = true;
            Medic.shielded = Helpers.playerById(shieldedId);
            Medic.futureShielded = null;
        }

        public static void shieldedMurderAttempt()
        {
            if (Medic.shielded == null || Medic.medic == null) return;
            bool playerGetsNotification = Medic.getsNotification == 0 && !PlayerControl.LocalPlayer.isFlashedByGrenadier()
                || (Medic.getsNotification == 1 && !PlayerControl.LocalPlayer.isFlashedByGrenadier() && (PlayerControl.LocalPlayer == Medic.medic || PlayerControl.LocalPlayer == Medic.shielded))
                || (Medic.getsNotification == 2 && !PlayerControl.LocalPlayer.isFlashedByGrenadier() && PlayerControl.LocalPlayer == Medic.medic)
                || (Medic.getsNotification == 3 && !PlayerControl.LocalPlayer.isFlashedByGrenadier() && PlayerControl.LocalPlayer == Medic.shielded);
            if (playerGetsNotification || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Medic.color, 1f, "Failed Murder Attempt on Shielded Player");
            if (!MeetingHud.Instance)
            {
                if (!Medic.unbreakableShield)
                {
                    Medic.exShielded = Medic.shielded;
                    Medic.shielded = null;
                }
            }
        }

        public static void setFutureShielded(byte playerId)
        {
            Medic.futureShielded = Helpers.playerById(playerId);
            Medic.usedShield = true;
        }

        public static void swapperSwap(byte playerId1, byte playerId2)
        {
            if (MeetingHud.Instance)
            {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
        {
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (Lawyer.target != null && dyingTarget == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            if (Executioner.target != null && dyingTarget == Executioner.target) Executioner.targetWasGuessed = true;
            if (GuardianAngel.target != null && dyingTarget == GuardianAngel.target) GuardianAngel.targetWasGuessed = true;
            PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null;
            if (Lawyer.target != null && dyingLoverPartner == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            if (Executioner.target != null && dyingLoverPartner == Executioner.target) Executioner.targetWasGuessed = true;
            if (GuardianAngel.target != null && dyingLoverPartner == GuardianAngel.target) GuardianAngel.targetWasGuessed = true;

            PlayerControl guesser = Helpers.playerById(killerId);
            if (guesser.IsThief(out Thief thief) && thief.thief.PlayerId == killerId && Thief.canStealWithGuess)
            {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                if (!thief.thief.Data.IsDead && !Thief.isFailedThiefKill(dyingTarget, guesser, roleInfo))
                {
                    thiefStealsRole(dyingTarget.PlayerId, guesser.PlayerId);
                }
            }

            if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == guesser && guesser != dyingTarget)
            {
                Doomsayer.guessedToWin++;
                if (Doomsayer.guessedToWin == Doomsayer.guessesToWin)
                {
                    if (CustomOptionHolder.doomsayerWinEndsGame.getBool()) Doomsayer.triggerDoomsayerWin = true;
                    else
                    {
                        Doomsayer.triggerGuessed = true;
                        if (PlayerControl.LocalPlayer == Doomsayer.doomsayer)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Doomsayer.doomsayer.Data, Doomsayer.doomsayer.Data);
                            if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                        }
                        Doomsayer.doomsayer.Exiled();
                    }
                }
            }

            if (Cannibal.cannibal != null && Cannibal.cannibal == guesser && guesser != dyingTarget && Cannibal.canEatWithGuess)
            {
                Cannibal.eatenBodies++;
                if (Cannibal.eatenBodies == Cannibal.cannibalNumberToWin)
                {
                    if (CustomOptionHolder.cannibalWinEndsGame.getBool()) Cannibal.triggerCannibalWin = true;
                    else
                    {
                        Cannibal.triggerEaten = true;
                        if (PlayerControl.LocalPlayer == Cannibal.cannibal)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Cannibal.cannibal.Data, Cannibal.cannibal.Data);
                            if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                        }
                        Cannibal.cannibal.Exiled();
                    }
                }
            }

            bool lawyerDiedAdditionally = false;
            if (Lawyer.lawyer != null && Lawyer.lawyer.PlayerId == killerId && Lawyer.target != null && Lawyer.target.PlayerId == dyingTargetId)
            {
                // Lawyer guessed client.
                if (PlayerControl.LocalPlayer == Lawyer.lawyer)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Lawyer.lawyer.Data, Lawyer.lawyer.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                Lawyer.lawyer.Exiled();
                lawyerDiedAdditionally = true;
                GameHistory.overrideDeathReasonAndKiller(Lawyer.lawyer, DeadPlayer.CustomDeathReason.LawyerSuicide, guesser);
            }

            bool executionerDiedAdditionally = false;
            if (Executioner.executioner != null && Executioner.executioner.PlayerId == killerId && Executioner.target != null && Executioner.target.PlayerId == dyingTargetId)
            {
                // Executioner guessed target.
                if (PlayerControl.LocalPlayer == Executioner.executioner)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Executioner.executioner.Data, Executioner.executioner.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                Executioner.executioner.Exiled();
                executionerDiedAdditionally = true;
                GameHistory.overrideDeathReasonAndKiller(Executioner.executioner, DeadPlayer.CustomDeathReason.Guess, guesser);
            }

            bool gaDiedAdditionally = false;
            if (GuardianAngel.guardianAngel != null && GuardianAngel.guardianAngel.PlayerId == killerId && GuardianAngel.target != null && GuardianAngel.target.PlayerId == dyingTargetId)
            {
                // Guardian Angel guessed client.
                if (PlayerControl.LocalPlayer == GuardianAngel.guardianAngel)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(GuardianAngel.guardianAngel.Data, GuardianAngel.guardianAngel.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                GuardianAngel.guardianAngel.Exiled();
                gaDiedAdditionally = true;
                GameHistory.overrideDeathReasonAndKiller(GuardianAngel.guardianAngel, DeadPlayer.CustomDeathReason.Guess, guesser);
            }

            dyingTarget.Exiled();
            overrideDeathReasonAndKiller(dyingTarget, DeadPlayer.CustomDeathReason.Guess, guesser);
            byte partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

            if (guesser == dyingTarget) addGameInfo(guesser.PlayerId, InfoType.AddIncorrectGuess);
            else if (guesser != dyingTarget)
            {
                if (!guesser.isEvil() && !dyingTarget.isEvil()) addGameInfo(guesser.PlayerId, InfoType.AddIncorrectGuess);
                else addGameInfo(guesser.PlayerId, InfoType.AddCorrectGuess);
            }

            HandleGuesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == dyingTargetId || lawyerDiedAdditionally && Lawyer.lawyer.PlayerId == pva.TargetPlayerId || executionerDiedAdditionally && Executioner.executioner.PlayerId == pva.TargetPlayerId || gaDiedAdditionally && GuardianAngel.guardianAngel.PlayerId == pva.TargetPlayerId)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                        MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, pva.TargetPlayerId);
                    }

                    //Give players back their vote if target is shot dead
                    if (pva.VotedFor != dyingTargetId && (!lawyerDiedAdditionally || Lawyer.lawyer.PlayerId != pva.VotedFor || !executionerDiedAdditionally || Executioner.executioner.PlayerId != pva.VotedFor || !gaDiedAdditionally || GuardianAngel.guardianAngel.PlayerId != pva.VotedFor)) continue;
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
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
                else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
            }

            // remove shoot button from targets for all guessers and close their guesserUI
            if (HandleGuesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer != guesser && !PlayerControl.LocalPlayer.Data.IsDead && GuesserGM.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance)
            {
                MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                if (dyingLoverPartner != null)
                    MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingLoverPartner.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null)
                {
                    if (MeetingHudPatch.guesserCurrentTarget == dyingTarget.PlayerId)
                        MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                    else if (dyingLoverPartner != null && MeetingHudPatch.guesserCurrentTarget == dyingLoverPartner.PlayerId)
                        MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                }
            }

            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null && guesser != null)
            {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                string msg = $"{guesser.Data.PlayerName} guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
        }

        public static void guardianAngelProtect()
        {
            GuardianAngel.isProtectActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GuardianAngel.duration, new Action<float>((p) =>
            {
                if (p == 1f) GuardianAngel.isProtectActive = false;
            })));
        }

        public static void guardianAngelSetTarget(byte playerId)
        {
            GuardianAngel.target = Helpers.playerById(playerId);
        }

        public static void guardianAngelPromotes()
        {
            PlayerControl player = GuardianAngel.guardianAngel;
            PlayerControl client = GuardianAngel.target;
            GuardianAngel.clearAndReload(false);

            if (GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Amnesiac)
            {
                setRole((byte)RoleId.Amnesiac, player.PlayerId);
            }
            if (GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Pursuer)
            {
                setRole((byte)RoleId.Pursuer, player.PlayerId);
            }
            if (GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Survivor)
            {
                setRole((byte)RoleId.Survivor, player.PlayerId);
            }
            if (GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Thief)
            {
                setRole((byte)RoleId.Thief, player.PlayerId);
            }
            if (GuardianAngel.becomeOnTargetDeath == GuardianAngel.BecomeOptions.Crewmate)
            {
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.crewmate);
            }

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null)
            {
                Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void swooperSwoop(byte playerId, byte flag)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue)
            {
                target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
                target.cosmetics.colorBlindText.gameObject.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
                target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(1f);

                if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive()) target.setDefaultLook();
                Swooper.isInvisible = false;
                return;
            }

            target.setLook("", 6, "", "", "", "");
            Color color = Color.clear;
            bool canSee = PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead;
            if (canSee) color.a = 0.1f;
            target.cosmetics.currentBodySprite.BodySprite.color = color;
            target.cosmetics.colorBlindText.gameObject.SetActive(false);
            target.cosmetics.colorBlindText.color = target.cosmetics.colorBlindText.color.SetAlpha(canSee ? 0.1f : 0f);
            Swooper.invisibleTimer = Swooper.duration;
            Swooper.isInvisible = true;
        }

        public static void werewolfRampage()
        {
            Werewolf.isRampageActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Werewolf.duration, new Action<float>((p) =>
            {
                if (p == 1f) Werewolf.isRampageActive = false;
            })));
        }

        public static void placeVent(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new MinerVent(position);
        }

        public static void cleanBody(byte playerId, byte cleaningPlayerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }

            if (Cannibal.cannibal != null && Cannibal.cannibal.PlayerId == cleaningPlayerId)
            {
                Cannibal.eatenBodies++;
                if (Cannibal.eatenBodies == Cannibal.cannibalNumberToWin)
                {
                    if (CustomOptionHolder.cannibalWinEndsGame.getBool()) Cannibal.triggerCannibalWin = true;
                    else
                    {
                        Cannibal.triggerEaten = true;
                        if (PlayerControl.LocalPlayer == Cannibal.cannibal)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Cannibal.cannibal.Data, Cannibal.cannibal.Data);
                            if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                        }
                        Cannibal.cannibal.Exiled();
                    }
                }
            }
        }

        public static void dragBody(byte playerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId) {
                    Undertaker.dragedBody = array[i];
                }
            }
        }

        public static void dropBody()
        {
            if (Undertaker.undertaker == null || Undertaker.dragedBody == null) return;
            var deadBody = Undertaker.dragedBody;
            Undertaker.dragedBody = null;
            deadBody.transform.position = new Vector3(Undertaker.undertaker.GetTruePosition().x, Undertaker.undertaker.GetTruePosition().y, Undertaker.undertaker.transform.position.z);
        }

        public static void grenadierFlash(bool clear = false)
        {
            if (clear)
            {
                Grenadier.flashedPlayers.Clear();
                return;
            }

            var closestPlayers = Helpers.getClosestPlayers(Grenadier.grenadier.GetTruePosition(), Grenadier.radius == 0 ? 100f : Grenadier.radius * 0.25f, true);
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

        public static void blackmailPlayer(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Blackmailer.blackmailed = target;
        }

        public static void unblackmailPlayer()
        {
            if (Blackmailer.blackmailed != null)
                Blackmailer.blackmailed = null;
            Blackmailer.alreadyShook = false;
        }

        public static void politicianCampaign(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Politician.campaignedPlayers.Add(target);
        }

        public static void politicianTurnMayor()
        {
            PlayerControl player = Politician.politician;
            Politician.clearAndReload();
            setRole((byte)RoleId.Mayor, player.PlayerId);
        }

        public static void mayorBodyguard()
        {
            Mayor.isBodyguardActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Mayor.duration, new Action<float>((p) =>
            {
                if (p == 1f) Mayor.isBodyguardActive = false;
            })));
        }

        public static void draculaCreatesVampire(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null) return;
            if (Executioner.target == player && Executioner.executioner != null && !Executioner.executioner.Data.IsDead)
            {
                PlayerControl exe = Executioner.executioner;
                PlayerControl tar = Executioner.target;
                Executioner.clearAndReload(false);
                setRole((byte)RoleId.Lawyer, exe.PlayerId);
                lawyerSetTarget(tar.PlayerId);
            }

            if (!Dracula.canCreateVampireFromNeutralBenign && player.isNeutralBenign() || !Dracula.canCreateVampireFromNeutralEvil && player.isNeutralEvil() || !Dracula.canCreateVampireFromNeutralKilling && player.isNeutralKilling() || !Dracula.canCreateVampireFromImpostor && player.Data.Role.IsImpostor)
            {
                Dracula.fakeVampire = player;
            }
            else
            {
                bool wasImpostor = player.Data.Role.IsImpostor;
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                if (player == Lawyer.lawyer && Lawyer.target != null)
                {
                    Transform playerInfoTransform = Lawyer.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == Executioner.executioner && Executioner.target != null)
                {
                    Transform playerInfoTransform = Executioner.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == GuardianAngel.guardianAngel && GuardianAngel.target != null)
                {
                    Transform playerInfoTransform = GuardianAngel.target.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                if (player == Medic.medic && Medic.shielded != null)
                {
                    Medic.exShielded = Medic.shielded;
                }
                Helpers.erasePlayerRoles(player.PlayerId);
                setRole((byte)RoleId.Vampire, player.PlayerId);
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasImpostor) Vampire.wasTeamRed = true;
                Vampire.wasImpostor = wasImpostor;
                if (player == PlayerControl.LocalPlayer) SoundEffectsManager.play("draculaBite");
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeVampireGetsGuesser.getBool())
                    setGuesserGm(targetId);
            }
            Dracula.canCreateVampire = false;
            Dracula.currentCreatedVampires++;
        }

        public static void vampirePromotes()
        {
            Dracula.removeCurrentDracula();
            Dracula.dracula = Vampire.vampire;
            Dracula.canCreateVampire = Dracula.currentCreatedVampires < Dracula.maxNumOfVampires;
            Dracula.wasTeamRed = Vampire.wasTeamRed;
            Dracula.wasImpostor = Vampire.wasImpostor;
            PlayerGameInfo.AddRole(Vampire.vampire.PlayerId, RoleInfo.dracula);
            Vampire.clearAndReload();
            return;
        }

        public static void poisonerSetPoisoned(byte targetId, byte performReset)
        {
            if (performReset != 0)
            {
                Poisoner.poisoned = null;
                return;
            }

            if (Poisoner.poisoner == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId && !player.Data.IsDead)
                {
                    Poisoner.poisoned = player;
                }
            }
        }

        public static void venererCamouflage()
        {
            if (Venerer.venerer == null) return;

            Venerer.abilityTimer = Venerer.duration;
            if (Camouflager.camouflageTimer <= 0f)
            {
                Venerer.venerer.setLook("", 15, "", "", "", "");
                Venerer.venerer.cosmetics.colorBlindText.text = "";
            }
        }

        public static void plaguebearerInfect(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Plaguebearer.infectedPlayers.Add(target);
        }

        public static void plaguebearerTurnPestilence()
        {
            PlayerControl player = Plaguebearer.plaguebearer;
            Plaguebearer.clearAndReload();
            setRole((byte)RoleId.Pestilence, player.PlayerId);
        }

        public static void glitchMimic(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (Glitch.glitch == null || target == null) return;

            Glitch.morphTimer = Glitch.morphDuration;
            Glitch.morphPlayer = target;
            if (Camouflager.camouflageTimer <= 0f)
                Glitch.glitch.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void glitchHack(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (Glitch.glitch == null || target == null) return;

            Glitch.hackedPlayer = target;
            Coroutines.Start(Helpers.Hack(target));
        }

        public static void showIndomitableFlash()
        {
            if (Indomitable.indomitable == PlayerControl.LocalPlayer) Helpers.showFlash(Indomitable.color);
        }

        public static void setTiebreak()
        {
            Tiebreaker.isTiebreak = true;
        }

        public static void sixthSenseAbilityTrigger()
        {
            if (SixthSense.sixthSense == PlayerControl.LocalPlayer) Helpers.showFlash(SixthSense.color);
        }

        public static void setPosition(byte playerId, float x, float y)
        {
            PlayerControl target = Helpers.playerById(playerId);
            target.transform.localPosition = new Vector3(x, y, 0);
            target.transform.position = new Vector3(x, y, 0);
        }

        public static void vampireHunterPromotes()
        {
            PlayerControl player = VampireHunter.vampireHunter;
            VampireHunter.clearAndReload();

            if (VampireHunter.becomeOnVampiresDeath == VampireHunter.BecomeOptions.Crewmate)
                PlayerGameInfo.AddRole(player.PlayerId, RoleInfo.crewmate);
            else if (VampireHunter.becomeOnVampiresDeath == VampireHunter.BecomeOptions.Sheriff)
                setRole((byte)RoleId.Sheriff, player.PlayerId);
            else if (VampireHunter.becomeOnVampiresDeath == VampireHunter.BecomeOptions.Veteran)
                setRole((byte)RoleId.Veteran, player.PlayerId);
        }

        public static void oracleSetConfessor(byte targetId)
        {
            Oracle.confessor = Helpers.playerById(targetId);
        }

        public static void oracleConfess()
        {
            if (Oracle.confessor == null) return;

            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(Oracle.confessor, false).FirstOrDefault();
            if (roleInfo == null) return;

            bool showsCorrectFaction = UnityEngine.Random.RandomRangeInt(1, 101) <= Oracle.accuracy;
            FactionId revealedFactionId;
            if (showsCorrectFaction)
            {
                if (roleInfo.factionId == FactionId.NeutralBenign || roleInfo.factionId == FactionId.NeutralKilling) revealedFactionId = FactionId.NeutralEvil;
                else revealedFactionId = roleInfo.factionId;
            }
            else
            {
                List<FactionId> possibleFaction = new List<FactionId> { FactionId.Crewmate, FactionId.Impostor, FactionId.NeutralBenign };
                possibleFaction.Remove(roleInfo.factionId);
                revealedFactionId = possibleFaction[UnityEngine.Random.RandomRangeInt(0, possibleFaction.Count)];
            }
            Oracle.revealedFactionId = revealedFactionId;
        }

        public static void sealVent(int ventId)
        {
            Vent vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            if (PlayerControl.LocalPlayer == Plumber.plumber)
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
                rend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }
            ventsToSeal.Add(vent);
        }

        public static void plumberFlush()
        {
            if (PlayerControl.LocalPlayer.inVent)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                Helpers.showFlash(Plumber.color, 1f, "Flushed by Plumber");
            }
        }

        public static void disperserDisperse()
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

            var SpawnPositions = Disperser.disperseToVents ? MapData.FindVentSpawnPositions() : GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
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
            SoundEffectsManager.play("disperserDisperse");
        }

        // Other functionality
        public enum GhostInfoTypes { DetectiveOrMedicInfo, DeathReasonAndKiller, BlankUsed, PoisonTimer, BountyTarget }
        public static void receiveGhostInfo(byte senderId, MessageReader reader)
        {
            PlayerControl sender = Helpers.playerById(senderId);

            GhostInfoTypes infoType = (GhostInfoTypes)reader.ReadByte();
            switch (infoType)
            {
                case GhostInfoTypes.DetectiveOrMedicInfo:
                    string detectiveInfo = reader.ReadString();
                    if (Helpers.shouldShowGhostInfo())
		    	        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, detectiveInfo);
                    break;
                case GhostInfoTypes.DeathReasonAndKiller:
                    overrideDeathReasonAndKiller(Helpers.playerById(reader.ReadByte()), (DeadPlayer.CustomDeathReason)reader.ReadByte(), Helpers.playerById(reader.ReadByte()));
                    break;
                case GhostInfoTypes.BlankUsed:
                    Pursuer.blankedList.Remove(sender);
                    break;
                case GhostInfoTypes.PoisonTimer:
                    poisonButton.Timer = (float)reader.ReadByte();
                    break;
                case GhostInfoTypes.BountyTarget:
                    Scavenger.bounty = Helpers.playerById(reader.ReadByte());
                    break;
            }
        }

        public static void setGuesserGm(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            new GuesserGM(target);
        }

        public static void setFirstKill(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            firstKillPlayer = target;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static bool Prefix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            return callId < 100;
        }

        static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId)
            {
                // Main Controls
                case (byte)CustomRPC.ResetVariables:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptions:
                    RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
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
                case (byte)CustomRPC.VersionHandshake:
                    byte major = reader.ReadByte();
                    byte minor = reader.ReadByte();
                    byte patch = reader.ReadByte();
                    float timer = reader.ReadSingle();
                    if (!AmongUsClient.Instance.AmHost && timer >= 0f) GameStartManagerPatch.timer = timer;
                    int versionOwnerId = reader.ReadPackedInt32();
                    byte revision = 0xFF;
                    Guid guid;
                    if (reader.Length - reader.Position >= 17) { // enough bytes left to read
                        revision = reader.ReadByte();
                        // GUID
                        byte[] gbytes = reader.ReadBytes(16);
                        guid = new Guid(gbytes);
                    } else {
                        guid = new Guid(new byte[16]);
                    }
                    RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    int ventId = reader.ReadPackedInt32();
                    byte ventingPlayer = reader.ReadByte();
                    byte isEnter = reader.ReadByte();
                    RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
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
                case (byte)CustomRPC.AddGameInfo:
                    RPCProcedure.addGameInfo(reader.ReadByte(), (InfoType)reader.ReadByte());
                    break;

                // Role functionality
                case (byte)CustomRPC.SetJesterWinner:
                    RPCProcedure.setJesterWinner(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SurvivorSafeguard:
                    RPCProcedure.survivorSafeguard(reader.ReadByte());
                    break;
                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.ShareRoom:
                    byte roomPlayer = reader.ReadByte();
                    byte roomId = reader.ReadByte();
                    RPCProcedure.shareRoom(roomPlayer, roomId);
                    break;
                case (byte)CustomRPC.VeteranAlert:
                    RPCProcedure.veteranAlert(reader.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetBlanked:
                    var pid = reader.ReadByte();
                    var blankedValue = reader.ReadByte();
                    RPCProcedure.setBlanked(pid, blankedValue);
                    break;
                case (byte)CustomRPC.AmnesiacTakeRole:
                    RPCProcedure.amnesiacTakeRole(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ThiefStealsRole:
                    RPCProcedure.thiefStealsRole(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.LawyerSetTarget:
                    RPCProcedure.lawyerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.LawyerPromotes:
                    RPCProcedure.lawyerPromotes();
                    break;
                case (byte)CustomRPC.ExecutionerSetTarget:
                    RPCProcedure.executionerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.ExecutionerPromotes:
                    RPCProcedure.executionerPromotes();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.SetFutureShielded:
                    RPCProcedure.setFutureShielded(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = reader.ReadByte();
                    byte playerId2 = reader.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.GuesserShoot:
                    byte killerId = reader.ReadByte();
                    byte dyingTarget = reader.ReadByte();
                    byte guessedTarget = reader.ReadByte();
                    byte guessedRoleId = reader.ReadByte();
                    RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                    break;
                case (byte)CustomRPC.GuardianAngelProtect:
                    RPCProcedure.guardianAngelProtect();
                    break;
                case (byte)CustomRPC.GuardianAngelSetTarget:
                    RPCProcedure.guardianAngelSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.GuardianAngelPromotes:
                    RPCProcedure.guardianAngelPromotes();
                    break;
                case (byte)CustomRPC.SwooperSwoop:
                    RPCProcedure.swooperSwoop(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.WerewolfRampage:
                    RPCProcedure.werewolfRampage();
                    break;
                case (byte)CustomRPC.PlaceVent:
                    RPCProcedure.placeVent(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.CleanBody:
                    RPCProcedure.cleanBody(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.DragBody:
                    RPCProcedure.dragBody(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DropBody:
                    RPCProcedure.dropBody();
                    break;
                case (byte)CustomRPC.GrenadierFlash:
                    RPCProcedure.grenadierFlash(reader.ReadBoolean());
                    break;
                case (byte)CustomRPC.BlackmailPlayer:
                    RPCProcedure.blackmailPlayer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UnblackmailPlayer:
                    RPCProcedure.unblackmailPlayer();
                    break;
                case (byte)CustomRPC.PoliticianCampaign:
                    RPCProcedure.politicianCampaign(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PoliticianTurnMayor:
                    RPCProcedure.politicianTurnMayor();
                    break;
                case (byte)CustomRPC.MayorBodyguard:
                    RPCProcedure.mayorBodyguard();
                    break;
                case (byte)CustomRPC.DraculaCreatesVampire:
                    RPCProcedure.draculaCreatesVampire(reader.ReadByte());
                    break;
                case (byte)CustomRPC.VampirePromotes:
                    RPCProcedure.vampirePromotes();
                    break;
                case (byte)CustomRPC.PoisonerSetPoisoned:
                    byte poisonedId = reader.ReadByte();
                    byte reset = reader.ReadByte();
                    RPCProcedure.poisonerSetPoisoned(poisonedId, reset);
                    break;
                case (byte)CustomRPC.VenererCamouflage:
                    RPCProcedure.venererCamouflage();
                    break;
                case (byte)CustomRPC.PlaguebearerInfect:
                    RPCProcedure.plaguebearerInfect(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaguebearerTurnPestilence:
                    RPCProcedure.plaguebearerTurnPestilence();
                    break;
                case (byte)CustomRPC.GlitchMimic:
                    RPCProcedure.glitchMimic(reader.ReadByte());
                    break;
                case (byte)CustomRPC.GlitchHack:
                    RPCProcedure.glitchHack(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShowIndomitableFlash:
                    RPCProcedure.showIndomitableFlash();
                    break;
                case (byte)CustomRPC.SetTiebreak:
                    RPCProcedure.setTiebreak();
                    break;
                case (byte)CustomRPC.SixthSenseAbilityTrigger:
                    RPCProcedure.sixthSenseAbilityTrigger();
                    break;
                case (byte)CustomRPC.SetPosition:
                    RPCProcedure.setPosition(reader.ReadByte(), reader.ReadSingle(), reader.ReadSingle());
                    break;
                case (byte)CustomRPC.VampireHunterPromotes:
                    RPCProcedure.vampireHunterPromotes();
                    break;
                case (byte)CustomRPC.OracleSetConfessor:
                    RPCProcedure.oracleSetConfessor(reader.ReadByte());
                    break;
                case (byte)CustomRPC.OracleConfess:
                    RPCProcedure.oracleConfess();
                    break;
                case (byte)CustomRPC.SealVent:
                    RPCProcedure.sealVent(reader.ReadPackedInt32());
                    break;
                case (byte)CustomRPC.PlumberFlush:
                    RPCProcedure.plumberFlush();
                    break;
                case (byte)CustomRPC.DisperserDisperse:
                    RPCProcedure.disperserDisperse();
                    break;

                // Other functionality
                case (byte)CustomRPC.ShareGhostInfo:
                    RPCProcedure.receiveGhostInfo(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.SetGuesserGm:
                    byte guesserGm = reader.ReadByte();
                    RPCProcedure.setGuesserGm(guesserGm);
                    break;
                case (byte)CustomRPC.SetFirstKill:
                    byte firstKill = reader.ReadByte();
                    RPCProcedure.setFirstKill(firstKill);
                    break;
            }
        }
    }
}