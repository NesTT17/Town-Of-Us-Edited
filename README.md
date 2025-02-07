## *Note: Imagined by the Town Of Us Discord Russian-speaking community, this Among Us mod modifies Town Of Us by adding lots of options to improve it.*

![LOGO](./Images/TOU-logo.png)

| **Crewmate Roles**            | **Neutral Roles**                 | **Neutral Killing Roles**                | **Impostor Roles**                | **Modifiers**                 |
|:-----------------------------:|:---------------------------------:|:----------------------------------------:|:---------------------------------:|:-----------------------------:|
| [Engineer](#engineer)         | [Amnesiac](#amnesiac)             | [Dracula](#dracula)                      | [Blackmailer](#blackmailer)       | [Bait](#bait)                 |
| [Investigator](#investigator) | [Doomsayer](#doomsayer)           | [Fallen Angel](#fallen-angel)            | [Camouflager](#camouflager)       | [Blind](#blind)               |
| [Mayor](#mayor)               | [Executioner](#executioner)       | [Juggernaut](#juggernaut)                | [Cleaner](#cleaner)               | [Indomitable](#indomitable)   |
| [Medic](#medic)               | [Guardian Angel](#guardian-angel) | [Vampire](#vampire)                      | [Escapist](#escapist)             | [Sunglasses](#sunglasses)     |
| [Seer](#seer)                 | [Jester](#jester)                 |                                          | [Grenadier](#grenadier)           | [Torch](#torch)               |
| [Sheriff](#sheriff)           | [Lawyer](#lawyer)                 |                                          | [Morphling](#morphling)           | [Button Barry](#button-barry) |
| [Shifter](#shifter)           | [Mercenary](#mercenary)           |                                          | [Miner](#miner)                   | [Drunk](#drunk)               |
| [Snitch](#snitch)             | [Pursuer](#pursuer)               |                                          | [Phantom](#phantom)               | [Lovers](#lovers)             |
| [Spy](#spy)                   | [Scavenger](#scavenger)           |                                          | [Poisoner](#poisoner)             | [Sleuth](#sleuth)             |
| [Swapper](#swapper)           | [Survivor](#survivor)             |                                          | [Swooper](#swooper)               | [Tiebreaker](#tiebreaker)     |
| [Trapper](#trapper)           |                                   |                                          |                                   | [Double Shot](#double-shot)   |
| [Veteran](#veteran)           |                                   |                                          |                                   |                               |
-----------------------

# Releases
| Among Us - Version| Mod Version | Link |
|----------|-------------|-----------------|
| v2024.11.26 | v1.0.1 | [Download](https://github.com/NesTT17/Town-Of-Us-Edited/releases/tag/v1.0.1) |

# Roles
## Crewmate Roles
## Engineer

The Engineer is a Crewmate that can fix sabotages from anywhere on the map.\
The Engineer has ability to open all doors on the map except decontamination doors.\
They can use vents to get across the map easily.
### Game Options
| Name | Description |
|----------|:-------------:|
| Engineer | The percentage probability of the Engineer appearing |
| Doors Open CD | The cooldown of the Engineer's Open Doors Button |
| Number Of Sabotage Fixes | The number of times the Engineer can fix a sabotage |

-----------------------
## Investigator
The Investigator is a Crewmate that can see the footprints of players.\
The Investigator has ability to 'Watching' player. If 'Watching player' use ability except Kill, Report, Vent, Investigator will receive flash.\
Every footprint disappears after a set amount of time.

**NOTE:** In v1.0.1 'Watching` player don't reset after meeting

### Game Options
| Name | Description |
|----------|:-------------:|
| Investigator | The percentage probability of the Investigator appearing |
| Can See Target Player Color | The flash has the color of whoever the ability is used on |
| Anonymous Footprint | When enabled, all footprints are grey instead of the player's colors |
| Footprint Interval | The time interval between two footprints |
| Footprint Duration | The amount of time that the footprint stays on the ground for |

-----------------------
## Mayor

The Mayor is a Crewmate that can reveal themself to everyone.\
Once revealed the Mayor cannot be assassinated, gains an additional 2 votes and everyone can see that they are the Mayor.\
As a consequence of revealing, they have half vision when lights are on.
### Game Options
| Name | Description |
|----------|:-------------:|
| Mayor | The percentage probability of the Mayor appearing |

-----------------------
## Medic

The Medic is a Crewmate that can give any player a shield that will make them immortal until the Medic dies.\
A Shielded player cannot be killed by anyone, unless by suicide.\
If the Medic reports a dead body, they can get a report containing clues to the Killer's identity.\
A report can contain the name of the killer or the color type (Darker/Lighter)
### Game Options
| Name | Description |
|----------|:-------------:|
| Medic | The percentage probability of the Medic appearing |
| Show Shielded Player | Who should be able to see who is Shielded |
| Show Murder Attempt | Who will receive an indicator when someone tries to Kill Shielded player |
| Gets Dead Body Info On Report | Whether the Medic should get information when reporting a body | Toggle | True |
| Time Where Medic Reports Will Have Name | If a body has been dead for shorter than this amount, the Medic's report will contain the killer's name |
| Time Where Medic Reports Will Have Color Type | If a body has been dead for shorter than this amount, the Medic's report will have the type of color | 

-----------------------
## Seer

The Seer is a Crewmate that can reveal the alliance of other players.\
Based on settings, the Seer can find out whether a player is a Good or an Evil role.\
A player's name will change color depending on faction and role.
### Game Options
| Name | Description |
|----------|:-------------:|
| Seer | The percentage probability of the Seer appearing |
| Reveal Cooldown | The Cooldown of the Seer's Reveal button |
| Neutrals Shows Evil | Neutral roles show up as Red |
| Killing Neutrals Shows Evil | Neutral Killing roles show up as Red |
-----------------------
## Sheriff

The Sheriff is a Crewmate that has the ability to eliminate the Impostors using their kill button.\
However, if they kill a Crewmate or a Neutral player they can't kill, they instead die themselves.
### Game Options
| Name | Description |
|----------|:-------------:|
| Sheriff | The percentage probability of the Sheriff appearing |
| Sheriff Can Kill Neutrals | Whether the Sheriff is able to kill the Neutral roles |
| Sheriff Can Kill Killing Neutrals | Whether the Sheriff is able to kill the Neutral Killing roles |
| Sheriff Cooldown | The cooldown on the Sheriff's kill button |
-----------------------
## Shifter

The Shifter is a Crewmate that can shift with another player. If the other player is Crewmate as well, they will swap their roles.\
Swapping roles with an Impostor or Neutral fails and the Shifter commits suicide after the next meeting (there won't be a body).\
The Shift will always be performed at the end of the next meeting right before a player is exiled. The target needs to be chosen during the round.\
Even if the Shifter or the target dies before the meeting, the Shift will still be performed.
### Game Options
| Name | Description |
|----------|:-------------:|
| Shifter | The percentage probability of the Shifter appearing |
-----------------------
## Snitch

The Snitch is a Crewmate that can get arrows pointing towards the Impostors, once all their tasks are finished.\
The names of the Impostors will also show up as red on their screen.
### Game Options
| Name | Description |
|----------|:-------------:|
| Snitch | The percentage probability of the Snitch appearing |
| Reveal Players On Meeting | Whether the Snitch sees the Impostor's/Neutral's names in Meetings |
| Tasks Count Where The Snitch Will Be Revealed | The number of tasks remaining when the Snitch is revealed to Impostors |
| Reveal Neutral Non-Killing Roles | Whether the Snitch also Reveals Neutral Roles |
| Reveal Neutral Killing Roles | Whether the Snitch also Reveals Neutral Killing Roles |
-----------------------
## Spy

The Spy is a Crewmate that gains more information when on the Admin Table and Vitals.\
On Admin Table, the Spy can see the colors of every person on the map.\
On Vitals, the Spy can see how long dead players have been dead for.
### Game Options
| Name | Description |
|----------|:-------------:|
| Spy | The percentage probability of the Spy appearing |
-----------------------
## Swapper

The Swapper is a Crewmate that during meetings can exchange votes that two people get (i.e. all votes that player A got will be given to player B and vice versa).\
### Game Options
| Name | Description |
|----------|:-------------:|
| Swapper | The percentage probability of the Swapper appearing |
| Swapper Can Call Emergency Meeting | Option to disable the emergency button for the Swapper |
| Swapper Can Only Swap Others | Sets whether the Swapper can swap themself or not |
-----------------------
## Trapper

The Trapper is a Crewmate that can place traps around the map.\
When players enter a trap they trigger the trap.\
In the following meeting, trapper receives how many evil roles trigger the trap.
### Game Options
| Name | Description |
|----------|:-------------:|
| Trapper | The percentage probability of the Trapper appearing |
| Min Amount of Time in Trap to Register | How long a player must stay in the trap for it to trigger |
| Trap Cooldown | The cooldown on the Trapper's trap button |
| Traps Removed Each Round | Whether the Trapper's traps are removed after each meeting |
| Maximum Number of Traps Per Game | The number of traps they can place in a game |
| Trap Size | The size of each trap |
| Minimum Number of Roles required to Trigger Trap | The number of players that must enter the trap for it to be triggered |
| Neutrals Shows Evil | Neutral roles count as evil |
| Killing Neutrals Shows Evil | Neutral Killing roles count as evil |
-----------------------
## Veteran

The Veteran is a Crewmate that can go on alert.\
When the Veteran is on alert, anyone, whether crew, neutral or impostor, if they interact with the Veteran, they die.
### Game Options
| Name | Description |
|----------|:-------------:|
| Veteran | The percentage probability of the Veteran appearing |
| Alert Cooldown | The cooldown on the Veteran's alert button. |
| Alert Duration | The duration of the alert |
| Number of Alerts | The number of times the Veteran can alert throughout the game |
-----------------------


<p align="center">This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC.</p>
<p align="center">© Innersloth LLC.</p>
