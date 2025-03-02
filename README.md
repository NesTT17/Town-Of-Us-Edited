## *Note: Imagined by the Town Of Us Discord Russian-speaking community, this Among Us mod modifies Town Of Us by adding lots of options to improve it.*

![LOGO](./Images/TOU-logo.png)

<p align="center">
  <img src="https://badgen.net/static/AmongUs/2024.11.26/red"> 
</p>

<p align="center">
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/releases"><img src="https://img.shields.io/github/v/release/NesTT17/Town-Of-Us-Edited?style=plastic"></a>
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/releases"><img src="https://img.shields.io/github/release-date/NesTT17/Town-Of-Us-Edited?style=plastic"></a>
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/releases"><img src="https://img.shields.io/github/downloads/NesTT17/Town-Of-Us-Edited/total?style=plastic"></a>
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/releases/latest"><img src="https://img.shields.io/github/downloads/nestt17/town-of-us-edited/latest/total?style=plastic"></a>
</p>

<p align="center">
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/issues"><img src="https://img.shields.io/github/issues-raw/NesTT17/Town-Of-Us-Edited?style=plastic"></a>
  <a href="https://github.com/NesTT17/Town-Of-Us-Edited/issues?q=is%3Aissue+is%3Aclosed"><img src="https://img.shields.io/github/issues-closed-raw/NesTT17/Town-Of-Us-Edited?style=plastic"></a>
</p>

<p align="center">
  <a href="https://github.com/NesTT17"><img src="https://img.shields.io/github/followers/nestt17?style=social"></a>
</p>

| **Crewmate Roles** | **Neutral Roles**  | **Neutral Killing Roles** | **Impostor Roles** | **Modifiers**  |
|:-----------------------------:|:---------------------------------:|:----------------------------------------:|:---------------------------------:|:-----------------------------:|
| [Detective](#detective)  | [Amnesiac](#amnesiac) | [Dracula](#dracula) | [Blackmailer](#blackmailer) | [Bait](#bait)  |
| [Engineer](#engineer) | [Doomsayer](#doomsayer) | [Fallen Angel](#fallen-angel) | [Camouflager](#camouflager) | [Blind](#blind) |
| [Investigator](#investigator)  | [Executioner](#executioner) | [Glitch](#glitch) | [Cleaner](#cleaner) | [Indomitable](#indomitable) |
| [Mayor](#mayor) | [Guardian Angel](#guardian-angel)| [Juggernaut](#juggernaut) | [Escapist](#escapist) | [Sunglasses](#sunglasses)  |
| [Medic](#medic) | [Jester](#jester) | [Vampire](#vampire) | [Grenadier](#grenadier)  | [Torch](#torch) |
| [Mystic](#mystic)  | [Lawyer](#lawyer) | [Werewolf](#werewolf)  | [Morphling](#morphling)  | [Button Barry](#button-barry) |
| [Seer](#seer) | [Mercenary](#mercenary) | | [Miner](#miner) | [Drunk](#drunk) |
| [Sheriff](#sheriff) | [Pursuer](#pursuer)  | | [Phantom](#phantom) | [Lovers](#lover) |
| [Shifter](#shifter) | [Scavenger](#scavenger) | | [Poisoner](#poisoner) | [Sleuth](#sleuth) |
| [Snitch](#snitch)  | [Survivor](#survivor) | | [Swooper](#swooper) | [Tiebreaker](#tiebreaker)  |
| [Spy](#spy)  | | | [Venerer](#venerer) | [Double Shot](#double-shot) |
| [Swapper](#swapper) | | | [Bounty Hunter](#bounty-hunter) | [Disperser](#disperser) |
| [Tracker](#tracker) | | | [Bomber](#bomber)  | [Armored](#armored)  |
| [Trapper](#trapper) | | | | |
| [Veteran](#veteran) | | | | |
| [Oracle](#oracle)  | | | | |
| [Vampire Hunter](#vampire-hunter) | | | | |

-----------------------

# Releases
| Among Us - Version| Mod Version | Link |
|----------|-------------|-----------------|
| v2024.11.26 | v1.1.2 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.1.2) |
| v2024.11.26 | v1.1.1 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.1.1) |
| v2024.11.26 | v1.1.0 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.1.0) |
| v2024.11.26 | v1.0.2 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.0.2) |
| v2024.11.26 | v1.0.1 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.0.1) |

# Roles
# Crewmate Roles

## Detective
The Detective is a Crewmate that can examine other players for suspicious behaviour.\
If the player the Detective examines has killed recently the Detective will be alerted about it.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Detective | The percentage probability of the Detective appearing | Percentage | 0% |
| Initial Examine Cooldown | The initial cooldown of the Detective's Examine button | Time | 30s |
| Examine Cooldown | The cooldown of the Detective's Examine button | Time | 10s |
| How Long Players Stay Bloody For | How long players remain bloody after a kill | Time | 30s |
| Show Detective Reports | Whether the Detective should get information when reporting a body | Toggle | True |
| Time Where Detective Reports Will Have Role | If a body has been dead for shorter than this amount, the Detective's report will contain the killer's role | Time | 15s |
| Time Where Detective Reports Will Have Faction | If a body has been dead for shorter than this amount, the Detective's report will contain the killer's faction | Time | 30s |
| Show Examine Reports | Whether the Detective should get information about their last exmaine target | Toggle | True |

-----------------------

## Engineer
The Engineer is a Crewmate that can fix sabotages from anywhere on the map.\
They can use vents to get across the map easily.\

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Engineer | The percentage probability of the Engineer appearing | Percentage | 0% |
| Doors Open CD | The cooldown of the Engineer's Open Doors button | Time | 30s |
| Number Of Sabotage Fixes | The number of times the Engineer can fix a sabotage | Number | 1 |

-----------------------

## Investigator
The Investigator is a Crewmate that can see the footprints of players.\
Every footprint disappears after a set amount of time.\
The Watching ability allows Investigator to see when a player is using their role's ability (except Kill, Report, Vent).

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Investigator | The percentage probability of the Investigator appearing | Percentage | 0% |
| Can See Target Player Color | Can see who the Watching player is using the ability on | Toggle | False |
| Anonymous Footprint | When enabled, all footprints are grey instead of the player's colors | Toggle | False |
| Footprint Interval | The time interval between two footprints | Time | 0.1s |
| Footprint Duration | The amount of time that the footprint stays on the ground for | Time | 10s |

-----------------------

## Mayor
The Mayor is a Crewmate that can reveal themself to everyone.\
Once revealed the Mayor cannot be assassinated, gains an additional 2 votes and everyone can see that they are the Mayor.\
As a consequence of revealing, they have half vision when lights are on.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Mayor | The percentage probability of the Mayor appearing | Percentage | 0% |

-----------------------

## Medic
The Medic is a Crewmate that can give any player a shield that will make them immortal until the Medic dies.\
A Shielded player cannot be killed by anyone, unless by suicide.\
If the Medic reports a dead body, they can get a report containing clues to the Killer's identity.\
A report can contain the name of the killer or the color type (Darker/Lighter)

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Medic | The percentage probability of the Medic appearing | Percentage | 0% |
| Show Shielded Player | Who should be able to see who is Shielded | Everyone / Shielded / Medic / Shielded + Medic / Nobody | Everyone |
| Show Murder Attempt | Who will receive an indicator when someone tries to Kill them | Everyone / Shielded / Medic / Shielded + Medic / Nobody | Everyone |
| Gets Dead Body Info On Report | Whether the Medic should get information when reporting a body | Toggle | True |
| Time Where Medic Reports Will Have Name | If a body has been dead for shorter than this amount, the Medic's report will contain the killer's name | Time | 0s |
| Time Where Medic Reports Will Have Color Type | If a body has been dead for shorter than this amount, the Medic's report will have the type of color | Time | 15s |

-----------------------

## Mystic
The Mystic is a Crewmate that gets an alert revealing when someone has died.\
On top of this, the Mystic briefly gets an arrow pointing in the direction of the body.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Mystic | The percentage probability of the Mystic appearing | Percentage | 0% |
| Arrow Duration | The duration of the arrows pointing to the bodies | Time | 0.5s |

-----------------------

## Oracle
The Oracle is a Crewmate that can get another player to confess information to them.\
The Oracle has 2 abilities, the first is that when they die, the person confessin to them will reveal their alignment.\
The second, is that every meeting the Oracle receives a confession about who might be evil.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Oracle | The percentage probability of the Oracle appearing | Percentage | 0% |
| Reveal Accuracy | The percentage probability of the Oracle's confessed player telling the truth | Percentage | 0% |
| Confess Cooldown | The Cooldown of the Oracle's Confess button | Time | 30s |
| Non-Killing Neutrals Shows Evil | Neutral Non-Killing roles show up as Evil | Toggle | False |
| Killing Neutrals Shows Evil | Neutral Killing roles show up as Evil | Toggle | False |

-----------------------

## Seer
The Seer is a Crewmate that can reveal the alliance of other players.\
Based on settings, the Seer can find out whether a player is a Good or an Evil role.\
A player's name will change color depending on faction and role.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Seer | The percentage probability of the Seer appearing | Percentage | 0% |
| Seer Cooldown | The Cooldown of the Seer's Reveal button | Time | 30s |
| Non-Killing Neutrals Shows Evil | Neutral Non-Killing roles show up as Evil | Toggle | False |
| Killing Neutrals Shows Evil | Neutral Killing roles show up as Evil | Toggle | False |

-----------------------

## Sheriff
The Sheriff is a Crewmate that has the ability to eliminate the Impostors using their kill button.\
However, if they kill a Crewmate or a Neutral player they can't kill, they instead die themselves.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Sheriff | The percentage probability of the Sheriff appearing | Percentage | 0% |
| Sheriff Kill Cooldown | The cooldown on the Sheriff's kill button | Time | 30s |
| Sheriff Can Kill Neutrals | Whether the Sheriff is able to kill Neutrals | Toggle | False |
| Sheriff Can Kill Killing Neutrals | Whether the Sheriff is able to kill Killing Neutrals | Toggle | False |

-----------------------

## Shifter
The Shifter is a Crewmate that can shift with another player.\
If the other player is Crewmate as well, Shifter gets Crewmate's role, but other player gets Vanilla Crewmate.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Shifter | The percentage probability of the Shifter appearing | Percentage | 0% |

-----------------------

## Snitch
The Snitch is a Crewmate that can get arrows pointing towards the Impostors, once all their tasks are finished.\
The names of the Impostors will also show up as red on their screen.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Snitch | The percentage probability of the Snitch appearing | Percentage | 0% |
| Reveal Players In Meeting | Whether the Snitch sees the player's names red or gray in Meetings | Toggle | False |
| Task Count Where The Snitch Will Be Revealed | The number of tasks remaining when the Snitch is revealed to Impostors/Neutrals | Number | 1 |
| Reveal Neutral Non-Killing Roles | Whether the Snitch also Reveals Neutral Non-Killing Roles | Toggle | False |
| Reveal Neutral Killing Roles | Whether the Snitch also Reveals Neutral Killing Roles | Toggle | False |

-----------------------

## Spy
The Spy is a Crewmate that gains more information when on the Admin Table and Vitals.\
On Admin Table, the Spy can see the colors of every person on the map.\
On Vitals, the Spy can see how long dead players have been dead for.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Spy | The percentage probability of the Spy appearing | Percentage | 0% |

-----------------------

## Swapper
The Swapper is a Crewmate that can swap the votes on 2 players during a meeting.\
All the votes for the first player will instead be counted towards the second player and vice versa.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Swapper | The percentage probability of the Swapper appearing | Percentage | 0% |
| Swapper Can Call Emergency Meeting | Whether the Swapper Can Press the Button | Toggle | False |
| Swapper Can Only Swap Others | Sets whether the Swapper can swap themself or not | Toggle | False |

-----------------------

## Tracker
The Tracker is a Crewmate that can track other players by tracking them during a round.\
Once they track someone, Tracker can see tracked players on his minimap

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Tracker | The percentage probability of the Tracker appearing | Percentage | 0% |
| Track Cooldown | The cooldown on the Tracker's track button | Time | 30s |
| Max Tracks | The number of new people they can track each round | Number | 3 |
| Tracks Reset After Meeting | Whether tracks are removed after each meeting | Toggle | False |

-----------------------

## Trapper
The Trapper is a Crewmate that can place traps around the map.\
When players enter a trap they trigger the trap.\
In the following meeting,  a trap will have display how many evil roles triggered the trap.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Trapper | The percentage probability of the Trapper appearing | Percentage | 0% |
| Min Amount of Time in Trap to Register | How long a player must stay in the trap for it to trigger | Time | 1s |
| Trap Cooldown | The cooldown on the Trapper's trap button | Time | 30s |
| Traps Removed Each Round | Whether the Trapper's traps are removed after each meeting | Toggle | True |
| Maximum Number of Traps Per Game | The number of traps they can place in a game | Number | 5 |
| Trap Size | The size of each trap | Factor | 0.25x |
| Minimum Number of Roles required to Trigger Trap | The number of players that must enter the trap for it to be triggered | Number | 3 |
| Neutrals Shows Evil | Neutrals counts evil in trap | Toggle | False |
| Killing Neutrals Shows Evil | Killing Neutrals counts evil in trap | Toggle | False |

-----------------------

## Vampire Hunter
The Vampire Hunter is a Crewmate role which can hunt Vampires.\
Their job is to kill all Vampires.\
Once all Vampires are dead they turn into a Veteran after the following meeting.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Vampire Hunter | The percentage probability of the Vampire Hunter appearing | Percentage | 0% |
| Stake Cooldown | The cooldown of the Vampire Hunter's Stake button | Time | 30s |
| Max Failed Stakes Per Game | The amount of times the Stake ability can be used per game incorrectly | Number | 5 |
| Can Stake Round One | If the Vampire Hunter can stake players on the first round | Toggle | False |
| Self Kill On Failure To Kill A Vamp With All Stakes | Whether the Vampire Hunter will die if they fail to stake any Vampires | Toggle | False |

-----------------------

## Veteran
The Veteran is a Crewmate that can go on alert.\
When the Veteran is on alert, anyone, whether crew, neutral or impostor, if they interact with the Veteran, they die.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Veteran | The percentage probability of the Veteran appearing | Percentage | 0% |
| Alert Cooldown | The cooldown on the Veteran's alert button. | Time | 5s |
| Alert Duration | The duration of the alert | Time | 30s |
| Number Of Alerts | The number of times the Veteran can alert throughout the game | Number | 3 |

-----------------------

## Vigilante
The Guesser can be a Crewmate or an Impostor (depending on the settings).
The Guesser can shoot players during the meeting, by guessing its role. If the guess is wrong, the Guesser dies instead.
You can select how many players can be shot per game and if multiple players can be shot during a single meeting.
The guesses Impostor and Crewmate are only right, if the player is part of the corresponding team and has no special role.
You can only shoot during the voting time.
Depending on the options, the Guesser can't guess the shielded player and depending on the Medic options the Medic/shielded player might be notified (no one will die, independently of what the Guesser guessed).

# Neutral Non-Killing Roles

## Amnesiac
The Amnesiac is a Neutral role with no win condition.\
They have zero tasks and are essentially roleless.\
However, they can remember a role by finding a dead player.\
Once they remember their role, they go on to try win with their new win condition.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Amnesiac | The percentage probability of the Amnesiac appearing | Percentage | 0% |
| Show Arrows To Dead Bodies | Whether the Amnesiac has arrows pointing to dead bodies | Toggle | False |
| Arrow Appears Delay | The delay of the arrows appearing after the person died | Time | 5s |

-----------------------

## Doomsayer
The Doomsayer is a Neutral role with its own win condition.\
Their goal is to assassinate a certain number of players.\
Once done so they win the game.\
They have an additional observe ability that hints towards certain player's roles.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Doomsayer | The percentage probability of the Doomsayer appearing | Percentage | 0% |
| Observe Cooldown | The Cooldown of the Doomsayer's Observe button | Time | 30s |
| Num Of Correct Guesses To Win | The amount of kills in order for the Doomsayer to win | Number | 3 |

-----------------------

## Executioner
The Executioner is a Neutral role with its own win condition.\
Their goal is to vote out a player, specified in the beginning of a game.\
If that player gets voted out, they win the game.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Executioner | The percentage probability of the Executioner appearing | Percentage | 0% |
| Executioner Vision | - | Multiplier | 1x |
| Executioner Can Call Emergency Meeting | Whether the Executioner Can Press the Button | Toggle | True |

-----------------------

## Guardian Angel
The Guardian Angel is a Neutral role which aligns with the faction of their target.\
Their job is to protect their target at all costs.\
If their target loses, they lose.

**NOTE**
- If target was killed in round (except voting out, guessing on meeting), Guardian Angel also die

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Guardian Angel | The percentage probability of the Guardian Angel appearing | Percentage | 0% |
| Protect Cooldown | The cooldown of the Guardian Angel's Protect button | Time | 30s |
| Protect Duration | How long The Guardian Angel's Protect lasts | Time | 10s |
| Number Of Protects | The amount of times the Protect ability can be used | Number | 5 |
| Show Protected Player | Who should be able to see who is Protected | Everyone / Protected / SelfGA / Protected + GA / Nobody | Everyone |

-----------------------

## Jester
The Jester is a Neutral role with its own win condition.\
If they are voted out after a meeting, the game finishes and they win.\
However, the Jester does not win if the Crewmates, Impostors or another Neutral role wins.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Jester | The percentage probability of the Jester appearing | Percentage | 0% |
| Jester Can Call Emergency Meeting | Whether the Jester Can Press the Button | Toggle | True |
| Jester Has Impostor Vision | Whether the Jester Has Impostor Vision | Toggle | False |

-----------------------

## Lawyer
The Lawyer is a Neutral role that has a client.\
The client might be a Killer which is no Lover.\
Depending on the options, the client can also be a Jester.\
The Lawyer needs their client to win in order to win the game.\
Their client doesn't know that it is their client.\
If their client gets voted out, the Lawyer dies with the client.\
If their client dies, the Lawyer changes their role and becomes the [Pursuer](#pursuer), which has a different goal to win the game.\
\
How the Lawyer wins:
- Lawyer dead/alive, client alive and client wins: The Lawyer wins together with the team of the client.
- If their client is Jester and the Jester gets voted out, the Lawyer wins together with the Jester.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Lawyer | The percentage probability of the Lawyer appearing | Percentage | 0% |
| Lawyer Vision | - | Multiplier | 1x |
| Lawyer Knows Target Role | - | Toggle | False |
| Lawyer Can Call Emergency Meeting | - | Toggle | False |
| Lawyer Target Can Be The Jester | - | Toggle | False |

-----------------------

## Mercenary
The Mercenary is a Neutral role with its own win condition.\
The Mercenary can use their Protect ability like a Medic shield.\
Unlike the Medic, the Mercenary shield resets on each round.\
To win, killers must attempt to kill a protected player a certain number of times.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Mercenary | The percentage probability of the Mercenary appearing | Percentage | 0% |
| Murders Required To Win | The amount of murder attempts in order for the Mercenary to win | Number | 3 |

-----------------------

## Pursuer
The Pursuer is still a neutral role, but has a different goal to win the game; they have to be alive when the game ends and the Crew wins.\
In order to achieve this goal, the Pursuer has an ability called "Blank", where they can fill a killer's (this also includes the Sheriff) weapon with a blank. So, if the killer attempts to kill someone, the killer will miss their target, and their cooldowns will be triggered as usual.\
If the killer fires the "Blank", shields (e.g. Medic shield or Time Master shield) will not be triggered.\
The Pursuer has tasks (which can already be done while being a Lawyer/Executioner), that count towards the task win for the Crewmates. If the Pursuer dies, their tasks won't be counted anymore.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Pursuer Blank Cooldown | The cooldown of the Pursuer's Blank button | Time | 30s |
| Pursuer Number Of Blanks | The amount of times the Blank ability can be used | Number | 5 |

-----------------------

## Scavenger
The Scavenger is a neutral role that must eat a specified number of corpses (depending on the options) in order to win.\
Depending on the options, when a player dies, the Scavenger gets an arrow pointing to the corpse.\
If there is a Scavenger in the game, there can't be a Cleaner.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Vulture | The percentage probability of the Vulture appearing | Percentage | 0% |
| Vulture Cooldown | The cooldown of the Vulure's Eat button | Time | 30s |
| Number Of Corpses Needed To Be Eaten | Corpes needed to be eaten to win the game | Number | 3 |
| Vulture Can Use Vents | - | Toggle | False |
| Show Arrows Pointing Towards The Corpes | Whether the Vulture has arrows pointing to dead bodies | Toggle | False |

-----------------------

## Survivor
The Survivor is a Neutral role which can win by simply surviving.\

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Safeguard Cooldown | The cooldown of the Survivor's Safeguard button | Time | 30s |
| Safeguard Duration | How long The Survivor's Safeguard lasts | Time | 10s |
| Number Of Safeguards | The amount of times the Safeguard ability can be used | Number | 5 |

-----------------------

# Neutral Killing Roles
## Dracula
The Dracula is part of an extra team, that tries to eliminate all the other players.\
The Dracula has no tasks and can kill Impostors, Crewmates and Neutrals.\
The Dracula can select another player to be their Vampire.
Creating a Vampire removes all tasks of the Vampire and adds them to the team Dracula. The Vampire loses their current role (except if they're a Lover, then they play in two teams).
The "Create Vampire Action" may only be used once per Dracula or once per game (depending on the options).
The Dracula can also promote Impostors to be their Vampire, but depending on the options the Impostor will either really turn into the Vampire and leave the team Impostors or they will just look like the Vampire to the Dracula and remain as they were.\
Also if a Spy or Impostor gets converted, they still will appear red to the Impostors.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Dracula | The percentage probability of the Dracula appearing | Percentage | 0% |
| Dracula/Vampire Bite Cooldown | The cooldown of the Dracula/Vampire's Bite button | Time | 30s |
| Dracula Can Vent | Whether the Dracula can Vent | Toggle | False |
| Max Number Of Vampires | The maximum amount of players that can be Vampires | Number | 2 |
| Dracula/Vampire Has Impostor Vision | Whether the Dracula/Vampire Has Impostor Vision | Toggle | False |
| Dracula Can Create Vampire From Impostor | Yes/No (to prevent a Dracula from turning an Impostor into a Vampire, if they use the ability on an Impostor they see the Impostor as Vampire, but the Impostor isn't converted to Vampire. If this option is set to "No" Jackal and Vampire can kill each other.) | Toggle | False |


-----------------------

## Fallen Angel
The Fallen Angel is a Neutral role with its own win condition.\
The Fallen Angel needs to be the last killer alive to win the game.

## Glitch
The Glitch is a Neutral role with its own win condition.\
The Glitch's aim is to kill everyone and be the last person standing.\
The Glitch can Hack players, resulting in them being unable to report bodies and do tasks.\
Hacking prevents the hacked player from doing anything but walk around the map.\
The Glitch can Mimic someone, which results in them looking exactly like the other person.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Glitch | The percentage probability of Glitch appearing | Percentage | 0% |
| Mimic Cooldown | The cooldown of Glitch's Mimic button | Time | 30s |
| Mimic Duration | How long Glitch can Mimic a player | Time | 10s |
| Hack Cooldown | The cooldown of Glitch's Hack button | Time | 30s |
| Hack Duration | How long Glitch can Hack a player | Time | 10s |
| Glitch Kill Cooldown | The cooldown of Glitch's Kill button | Time | 30s |
| Glitch can Vent | Whether the Glitch can Vent | Toggle | False |

-----------------------

## Juggernaut
The Juggernaut is a Neutral role with its own win condition.\
The Juggernaut's special ability is that their kill cooldown reduces with each kill.\
This means in theory the Juggernaut can have a 0 second kill cooldown!\
The Juggernaut is also a hidden role, meaning it will show up randomly and can not be toggled by percentages like other roles.\
The Juggernaut needs to be the last killer alive to win the game.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Initial Kill Cooldown | The initial cooldown of the Juggernaut's Kill button | Time | 30s |
| Cooldown Reduce On Kill | The amount of time removed from the Juggernaut's Kill Cooldown Per Kill | Time | 5s |
| Juggernaut Has Impostor Vision | Whether the Juggernaut Has Impostor Vision | Toggle | False |
| Juggernaut Can Use Vents | Whether the Juggernaut can Vent | Toggle | False |

-----------------------

## Vampire
Gets assigned to a player during the game by the "Create Vampire Action" of the Dracula and joins the Dracula in their quest to eliminate all other players.\
Upon the death of the Dracula (depending on the options), they might get promoted to Dracula themself and potentially even assign a Vampire of their own.\

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Dracula/Vampire Bite Cooldown | The cooldown of the Dracula/Vampire's Bite button | Time | 30s |
| Vampire Can Vent | Whether the Vampire can Vent | Toggle | False |

-----------------------

## Werewolf
The Werewolf is a Neutral role with its own win condition.\
Although the Werwolf has a kill button, they can't use it unless they are Rampaged.\
Once the Werewolf rampages they gain Impostor vision and the ability to kill.\
However, unlike most killers their kill cooldown is really short.\
The Werewolf needs to be the last killer alive to win the game.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Werewolf | The percentage probability of the Werewolf appearing | Percentage | 0% |
| Rampage Cooldown | The cooldown of the Werewolf's Rampage button | Time | 30s |
| Rampage Duration | The duration of the Werewolf's Rampage | Time | 10s |
| Rampage Kill Cooldown | The cooldown of the Werewolf's Kill button | Time | 3s |
| Can Vent when Rampaged | Whether the Werewolf can Vent when Rampaged | Toggle | False |

-----------------------

# Impostor Roles
## Bomber
The Bomber is an Impostor who has the ability to plant bombs instead of kill.\
After a bomb is planted, the bomb will detonate a fixed time period as per settings.\
Once the bomb detonates it will kill all crewmates (and Impostors!) inside the radius.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Bomber | The percentage probability of the Bomber appearing | Percentage | 0% |
| Detonate Delay | The delay of the detonation after bomb has been planted | Time | 5s |
| Max Kills In Detonation | Maximum number of kills in the detonation | Time | 5s |
| Detonate Radius | How wide the detonate radius is | Multiplier | 0.25x |

-----------------------

## Bounty Hunter
The Bounty Hunter is an Impostor, that continuously get bounties (the targeted player doesn't get notified).\
The target of the Bounty Hunter swaps after every meeting and after a configurable amount of time.\
If the Bounty Hunter kills their target, their kill cooldown will be a lot less than usual.\
Killing a player that's not their current target results in an increased kill cooldown.\
Depending on the options, there'll be an arrow pointing towards the current target.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Scavenger | The percentage probability of the Scavenger appearing | Percentage | 0% |
| Duration After Which Bounty Changes | - | Time | 40s |
| Cooldown After Killing Bounty | The kill cooldown the Scavenger has on a correct kill | Time | 2.5s |
| Additional Cooldown After Killing Others | Time will be added to the normal impostor cooldown if the Bounty Hunter kills a not-bounty player | Time | 20s |
| Show Arrow Pointing Towards The Bounty | If set to true an arrow will appear (only visiable for the Bounty Hunter) | Toggle | False |
| Bounty Hunter Arrow Update Interval | Sets how often the position is being updated | Time | 5s |

-----------------------

## Blackmailer
The Blackmailer is an Impostor that can silence people in meetings.\
During each round, the Blackmailer can go up to someone and blackmail them.\
This prevents the blackmailed person from speaking and possibly voting during the next meeting.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Blackmailer | The percentage probability of the Blackmailer appearing | Percentage | 0% |
| Initial Blackmail Cooldown | The initial cooldown of the Blackmailer's Blackmail button | Time | 10s |

-----------------------

## Camouflager
The Camouflager is an Impostor which can additionally activate a camouflage mode.\
The camouflage mode lasts for x-seconds (configurable) and while it is active, all player names/pets/hats are hidden and all players have the same color.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Camouflager | The percentage probability of the Camouflager appearing | Percentage | 
| Camouflager Cooldown | The cooldown of the Camouflager's Camo button | Time | 30s |
| Camo Duration | How long The Camouflager's Camo lasts | Time | 10s |

-----------------------

## Cleaner
The Cleaner is an Impostor who has the ability to clean up dead bodies.\
The Kill and Clean cooldown are shared, preventing them from immediately cleaning their own kills.\
If there is a Cleaner in the game, there can't be a Scavenger.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Cleaner | The percentage probability of the Cleaner appearing | Percentage | 0% |
| Clean Cooldown | The cooldown of the Cleaner's Clean button | Time | 30s |

-----------------------

## Escapist
The Escapist an Impostor who has the ability mark a position and later recall (teleport) to this position.\
After the initial recall, the Escapist has a fixed amount of time (option) to do whatever they want, before automatically recalling back to the starting point of the first recall.\

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Escapist | The percentage probability of the Escapist appearing | Percentage | 0% |
| Recall Duration | - | Time | 5s |
| Mark Location Cooldown | The cooldown of the Escapist's Recall button | Time | 30s |
| Marked Location Stays After Meeting | - | Toggle | False |

-----------------------

## Grenadier
The Grenadier is an Impostor that can throw smoke grenades.\
During the game, the Grenadier has the option to throw down a smoke grenade which blinds crewmates so they can't see.\
However, a sabotage and a smoke grenade can not be active at the same time.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Grenadier | The percentage probability of the Grenadier appearing | Percentage | 0% |
| Grenadier Cooldown | The cooldown of the Grenadier's Flash button | Time | 30s |
| Flash Duration | How long the Flash Grenade lasts for | Time | 10s |
| Flash Radius | How wide the flash radius is | All Map / 0.25x - 3x | All Map |
| Indicate Flashed Crewmates | Whether the Grenadier can see who has been flashed | Toggle | False |

-----------------------

## Miner
The Miner is an Impostor that can place 3 vents that are invisible at first to other players.\
If the Miner has placed all of their vents they will be converted into a vent network after the next Meeting, usable only by the Miner themself, but the vents are also revealed to the others.\


### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Miner | The percentage probability of the Miner appearing | Percentage | 0% |
| Place Vent Cooldown | The cooldown of the Miner's Mine button | Time | 30s |

-----------------------

## Morphling
The Morphling is an Impostor that can Morph into another player.\
At the beginning of the game and after every meeting, they can choose someone to Sample.\
They can then Morph into that person at any time for a limited amount of time.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Morphling | The percentage probability of the Morphling appearing | Percentage | 0% |
| Morph Cooldown | The cooldown of the Morphling's Morph button | Time | 30s |
| Morph Duration | How long the Morph lasts for | Time | 10s |

-----------------------

## Phantom
The Phantom is an Impostor role that can temporarily turn invisible and walk through walls.\
If there is a Phantom in the game, there can't be a Swooper.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Phantom | The percentage probability of the Phantom appearing | Percentage | 0% |
| Invis Cooldown | The cooldown of the Phantom's Invis button | Time | 30s |
| Invis Duration | How long the Invising lasts for | Time | 10s |

-----------------------

## Poisoner
The Poisoner is an Impostor who has to poison another play instead of kill.\
When they poison a player, the poisoned player dies either upon the start of the next meeting or after a set duration.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Poisoner | The percentage probability of the Poisoner appearing | Percentage | 0% |
| Poison Kill Delay | The delay of the kill after being poisoned | Time | 5s |
| Poison Cooldown | The cooldown of the Poisoner's Poison button | Time | 30s |

-----------------------

## Swooper
The Swooper is an Impostor that can temporarily turn invisible.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Swooper | The percentage probability of the Swooper appearing | Percentage | 0% |
| Swooper Cooldown | The cooldown of the Swooper's Swoop button | Time | 30s |
| Swooper Duration | How long the Swooping lasts for | Time | 10s |

-----------------------

## Venerer
The Venerer is an Impostor that gains abilities through killing.\
After their first kill, the Venerer can camouflage themself.\
After their second kill, the Venerer can sprint.\
After their third kill, every other player is slowed while their ability is activated.\
All abilities are activated by the one button and have the same duration.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Venerer | The percentage probability of the Venerer appearing | Percentage | 0% |
| Ability Cooldown | The cooldown of the Venerer's Ability button | Time | 25s |
| Ability Duration | How long the Venerer's ability lasts for | Time | 10s |
| Sprint Speed | How fast the speed increase of the Venerer is when sprinting | Multiplier | 1.125x |
| Freeze Speed | How slow the speed decrease of other players is when the Venerer's ability is active | Multiplier | 0.5x |

-----------------------

# Modifiers
## Bait
### **Applied to: Crewmates**
Killing the Bait makes the killer auto self-report.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Bait | The percentage probability of the Bait appearing | Percentage | 0% |
| Bait Report Delay Min | The minimum time the killer of the Bait reports the body | Time | 0s |
| Bait Report Delay Max | The maximum time the killer of the Bait reports the body | Time | 0s |

-----------------------

## Blind
### **Applied to: Crewmates**
The Blind's report button does not light up.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Blind | The percentage probability of the Blind appearing | Percentage | 0% |

-----------------------

## Indomitable
### **Applied to: Crewmates**
Player with Indomitable modifier can not be guessed on meeting.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Indomitable | The percentage probability of the Indomitable appearing | Percentage | 0% |

-----------------------

## Sunglasses
The Sunglasses will lower the Crewmate's vision by a small percentage. The percentage is configurable in the options.\
The vision will also be affected when lights out.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Sunglasses | The percentage probability of the Sunglasses appearing | Percentage | 0% |
| Vision With Sunglasses | - | Percentage | -30% |

-----------------------

## Torch
### **Applied to: Crewmates**
The Torch's vision doesn't get reduced when the lights are sabotaged.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Torch | The percentage probability of the Torch appearing | Percentage | 0% |

-----------------------

## Armored
### **Applied to: All**
The Armored is a Modifier that protects the player from the first shot that would have killed them.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Armored | The percentage probability of the Armored appearing | Percentage | 0% |

-----------------------

## Button Barry
### **Applied to: All except Glitch**
Button Barry has the ability to call a meeting from anywhere on the map, even during sabotages.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Button Barry | The percentage probability of Button Barry appearing | Percentage | 0% |

-----------------------

## Drunk
### **Applied to: All**
The Drunk's controls are inverted.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Drunk | The percentage probability of the Drunk appearing | Percentage | 0% |

-----------------------

## Lover
### **Applied to: All**
There are always two Lovers which are linked together.\
Their primary goal is it to stay alive together until the end of the game.\
If one Lover dies (and the option is activated), the other Lover suicides.\
You can specify the chance of one Lover being an Impostor.\
The Lovers never know the role of their partner, they only see who their partner is.\
The Lovers win, if they are both alive when the game ends. They can also win with their original team (e.g. a dead Impostor Lover can win with the Impostors, an Neutral Killer Lover can still achieve an Neutral Killer win).\
If one of the Lovers is a killer (i.e. Neutral Killer/Impostor), they can achieve a "Lovers solo win" where only the Lovers win.\
If there is no killer among the Lovers (e.g. an Neutral Killer Lover + Crewmate Lover) and they are both alive when the game ends, they win together with the Crewmates.\
If there's an Impostor/Neutral Killer + Crewmate Lover in the game, the tasks of a Crewmate Lover won't be counted (for a task win) as long as they're alive.\
If the Lover dies, their tasks will also be counted.\
You can enable an exclusive chat only for Lovers.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Lovers | The percentage probability of the Lovers appearing | Percentage | 0% |
| Chance That One Lover Is Killer | The chances of one lover being an Evil Killer | Percentage | 0% |
| Both Lovers Die | Whether the other Lover automatically dies if the other does | Toggle | True |
| Enable Lover Chat | - | Toggle | True |

-----------------------

## Sleuth
### **Applied to: All**
The Sleuth is a crewmate who gains knowledge from reporting dead bodies.\
During meetings the Sleuth can see the roles of all players in which they've reported.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Sleuth | The percentage probability of the Sleuth appearing | Percentage | 0% |

-----------------------

## Tiebreaker
### **Applied to: All**
If any vote is a draw, the Tiebreaker's vote will go through.\
If they voted another player, they will get voted out.\
If the Tiebreaker is the Mayor, it applies to the Mayor's __first__ vote.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Tiebreaker | The percentage probability of the Tiebreaker appearing | Percentage | 0% |

-----------------------

## Disperser
### **Applied to: Impostors**
The Disperser is an Impostor who has a 1 time use ability to send all players to a random place.\
Does not appear on Airship.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Disperser | The percentage probability of the Disperser appearing | Percentage | 0% |

-----------------------

## Double Shot
### **Applied to: Impostors**
Double Shot is an Impostor who gets an extra life when assassinating.\
Once they use their life they are indicated with a red flash\
and can no longer guess the person who they guessed wrong for the remainder of that meeting.

### Game Options
| Name | Description | Type | Default |
|----------|:-------------:|:------:|:------:|
| Double Shot | The percentage probability of Double Shot appearing | Percentage | 0% |

-----------------------