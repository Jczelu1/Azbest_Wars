# Asbestos Wars

## 1. Game Description
Asbestos Wars is a real-time strategy game (RTS), created for the "Invent your game 2025" competition.

It is a PvE game in which the player controls an army of units and fights against a bot army.

The goal of the game is to complete all levels. Good luck.

## 2. System Requirements
This version of the game is designed for Windows.

## 3. Compilation Instructions
- Open (or install) Unity Hub
- In the upper right corner, click "Add" and select the "Azbest Wars Project" folder.
- Install Unity 6000.0.39f1 if it is not installed.
- Open "Azbest Wars Project."
- After launching the project editor, click File->Build and Run.
- Select the folder where the compiled game will be located.
- The game should launch automatically.

## 4. Launch Instructions
- The compiled version of the game is located in the "Azbest Wars" folder.
- To launch the game, open the "Azbest Wars.exe" file.

## 5. How to Play
- I recommend completing the in-game tutorial.
- Button assignments can be viewed by clicking "Controls" in the game.

## 6. Details

### a. Game Technology
- The game was created using the Unity engine version 6000.0.39f1
- All game code is written in C#.
- The game uses Unity's DOTS (Data Oriented Technology Stack) technology, which allows for significantly improved game performance.
- In addition to the interface, the game uses the ECS (Entity Component System), which is part of DOTS, to handle game elements.
- DOTS also allows for the use of multiple CPU threads, which the game uses, for example, to find unit paths.

### b. Graphics and Music
All graphics and music in the game (except for the fonts) were created by me for this game.
The fonts are from Google Fonts and are available under the OFL license.

### c. General Game Operation
- All maps in the game are 128x128 grids.
- The core element of the game is the "Unit." Units can move and attack opposing units.
- Units have two modes: defense mode and attack mode. If defense mode is enabled, the unit will only move when the player moves it. If attack mode is enabled, the unit will move towards enemies, at a distance sufficient for attack. The third mode is to stop the unit; after stopping, the unit's mode is set to defense mode.
- There are three types of buildings in the game: Resource Buildings (produce resources), Spawners (produce units for resources), and Zones (contain other buildings and are captured by units).
- The basic unit of time in the game is the "Tick," which by default lasts 0.5 seconds, but the player can change the time speed to x0.5, x1, x2, or x4. During the first tick, actions such as attacking units, moving units properly, producing units, and capturing zones occur.
- Another unit of time is the "Subtick," which is executed four times per tick. Subticks are primarily responsible for animations and are not required for game operation, so subticks are disabled at x4 speed for better performance.
- The player can select units, move units, change unit modes, and select unit production type and quantity.
- The bot has the same capabilities as the player; more on the bot's operation later.

### d. Units
#### Unit Operation
Units are produced by spawners for resources. They have different costs and production times.
Units can move 1 square per tick, including diagonally if it is unoccupied.
Units search for paths in two ways:
- A* pathfinding - for moving a unit by the player (or bot), and for finding a bypass if the previously found path is occupied by other units.
- Breath first search - for attack mode, i.e., moving towards the enemy. Units in attack mode find enemies up to 12 squares away. Units cannot attack while moving.
Units can attack once per tick; some units must wait one or more ticks after attacking or moving before they can attack again.
Units have four attack types:
- Close Attack 1: The unit attacks an enemy 1 square away and has a 20% chance of a critical hit, which deals double damage.
- Close Attack 2: The unit attacks enemies 1 square away and attacks three adjacent squares simultaneously. The unit chooses the three squares with the most units. It has a 20% chance of a critical hit.
- Close Attack 3: The unit attacks an enemy 2 squares away and has a 20% chance of a critical hit.
- Ranged Attack: The unit attacks an enemy a varying number of squares away. The range varies between units. If the distance from the enemy is greater than half the range, there is a 40% chance of missing. If the enemy is closer, there's a 20% chance of missing. Units have different amounts of HP. Remaining HP is displayed as a life bar below each unit.

#### Unit List
There are 12 unit types in the game:
| Name | Cost | Production Time | HP | Attack Type | Dmg/Tick | Dmg | Range | Cooldown after Attack | Cooldown after Moving |
|:----------------------:|:-----:|:-------------:|:----:|:----------:|:---------:|:---------:|:--------:|:-------:|:------:|:------:|:----------------:|:-----:|
| Peasant with Sword | 50 | 4 | 8 | Close Attack 1 | 1 | 1 | 1 | 0 | 0 |
| Peasant with Axe | 60 | 4 | 8 | Close Attack 2 | 0.5 | 1 | 1 | 1 | 0 |

| Peasant with Scythe | 60 | 4 | 6 | Close Attack 3 | 1 | 1 | 2 | 0 | 1 |
| Peasant with Bow | 60 | 4 | 4 | Long Attack | 0.33 | 1 | 6 | 2 | 1 |
| Soldier with Sword | 100 | 6 | 16 | Close Attack 1 | 2 | 2 | 1 | 0 | 0 |
| Soldier with Axe | 120 | 6 | 16 | Close Attack 2 | 1 | 2 | 1 | 1 | 0 |
| Soldier with Spear | 120 | 6 | 12 | Close Attack 3 | 2 | 2 | 2 | 0 | 1 |
| Archer | 120 | 6 | 8 | Long Attack | 0.66 | 2 | 8 | 2 | 1 |
| Knight | 360 | 12 | 48 | Close Attack 1 | 4 | 8 | 1 | 1 | 0 |
| Viking | 240 | 10 | 24 | Close Attack 2 | 2 | 2 | 1 | 0 | 0 |
| Guard | 240 | 10 | 32 | Close Attack 3 | 3 | 6 | 2 | 1 | 1 |
| Crossbowman | 300 | 8 | 12 | Long Attack | 1.2 | 6 | 10 | 4 | 0 |

### e. Buildings
There are 3 types of buildings in the game:
#### Resource Buildings
Resource buildings produce a certain amount of asbestos per tick. Building List:
- House (2 Asbestos/Tick)
- Mill (4 Asbestos/Tick)
- Mine (8 Asbestos/Tick)

#### Spawners
Spawners produce units for resources. The player can set the number and type of units produced.
There is only one Spawner in the game: Camp

#### Zones
There is only one type of zone in the game. Zones can contain the above-mentioned buildings. Zones are captured by units. Zones have varying amounts of required capture points (100-1000). Capture points are scored per tick and are equal to the number of the capturing party's units in the zone minus the number of defending units, with a maximum value of 10 per tick. If the capturing party has fewer units in the zone than the defender, or the capturing party has zero units in the zone, the capture points are reset.
Zones are the only reference point on the map for the Bot, which only sends units to zones.

### f. Bot
The bot, or "AI," is quite simple. The bot has the same capabilities as the player, meaning it can select units, move units, change unit modes, and choose the type and amount of unit production. Additionally, the bot gains the same amount of resources from buildings as the player.

- The bot has several parameters that vary between levels.
- Initially, the bot cycles through all the Spawners it controls. If one is not currently producing units (has queue = 0), the bot randomly selects the unit type and number (3 - 6) until it finds a combination for which it has enough resources. The bot also has a random chance to not select any type and enter a saving mode, lasting a number of ticks depending on the parameters. Produced units will be assigned to a "Formation." After selection, the bot randomly selects whether to further expand the formation or send it into battle.
- Then, the bot cycles through all the Formations. If the formation has been completed and has not yet been assigned a target, the bot selects a target from the list of zones. The closest target is selected, but there is a chance (dependent on a parameter) that the target will be rejected. The chance that the bot will send a formation to a target it already controls is also a parameter. The bot sets units to attack mode if it sends them to a zone it doesn't control; otherwise, it has a 50% chance of setting units to defense mode.
- If the formation has captured the zone it was sent to, the bot can immediately send units to the next zone or leave them in the captured zone. The chance for this is also a parameter.
- There is also a small chance that the bot will "correct" the units' position, meaning it will move them back into the zone, and an even smaller chance that the bot will change the formation's target.
- Finally, the bot selects units from the formation for which the new target has been set and moves them.
- If the bot's unit has been attacked, its mode is set to attack mode.


