# Hackathon 7.0 (prisoner-and-ball game)

A top-down Unity game. The player is chained to a heavy **Ball**, lures **Prey** out of **Ponds** with **Bait**, sells it for **Money**, buys **Chisel Kits** to break the Ball, and finally survives the **Escape** from the **Ghost**.

## Language

### Player and progression

**Player**:
The character the user moves with WASD; always in exactly one **Player State**.
_Avoid_: character, hero

**Player State**:
The mode the **Player** is in: `Regular`, `Interacting`, `Shopping`, `Catching` or `Dead`. Only `Regular` allows walking and grabbing.
_Avoid_: mode, status

**Ball**:
The heavy sphere chained to the **Player**'s feet by a **Chain** that pulls behind them as they walk. It shrinks with each **Chisel Kit** and is destroyed by the fifth.
_Avoid_: ball follower, weight

**Chain**:
The rope that ties the **Ball** to the **Player** and has a fixed length; drawn as a sagging line.
_Avoid_: rope

**Chisel Kit**:
The one upgrade sold in the **Store**. Each purchase shrinks the **Ball**, raises the **Player**'s speed, and returns them to `Regular`. There are 5 in total.
_Avoid_: upgrade, chiesel kit

**Money**:
The **Player**'s single currency. Starts at 500, earned by **Selling** **Prey**, spent on **Bait** and **Chisel Kits**.
_Avoid_: coins, balance

**Death**:
What happens when a **Catch** fails or the **Ghost** reaches the **Player**: the screen fades out, the **Player** respawns at the world origin and returns to `Regular`. They lose nothing.
_Avoid_: game over, lose

### Buildings

**Building**:
A fixed world object the **Player** enters by facing it and pressing E (the **Interact** input). While inside it, the **Player** is `Interacting` and a dedicated camera takes over. Examples: **Pond**, **Store**, **Lift**.
_Avoid_: station, structure

**Use Indicator**:
A child object of each **Building** that shows when the **Player** is `Regular` and pressing E would start an interaction with it. At most one is shown at a time.

**Store**:
The **Building** where the **Player** buys **Bait** and **Chisel Kits**. Its trigger volume also **Sells** any **Prey** dropped into it.
_Avoid_: shop

**Pond**:
A **Building** made of a door and a dark corridor behind it (not water). The **Player** burns **Bait** at it to draw out a **Prey** and starts a **Catch**. There are several in the map.
_Avoid_: lake, room

**Corridor Darkness**:
Layered dark haze filling a **Pond**'s corridor. It hides everything except emissive materials, so a **Prey**'s glowing eyes appear first. The flashlight does not cut through it.
_Avoid_: fog, shadow

**Lift**:
A **Building** with a lever and a **Lift Platform**. Pulling the lever raises the platform and anything on it to the **Sell Height**, then lowers it again.
_Avoid_: elevator

**Lift Platform**:
The moving surface of the **Lift**. **Prey** that lands on it is attached so it rides along.

### Catching

**Bait**:
A consumable bought at the **Store** in three tiers (0 blue, 1 green, 2 purple). Each costs more but waits less for a bite and pulls rarer **Prey**. Burning one at a **Pond** gives off coloured **Incense**.
_Avoid_: lure, bait type (when you mean the item itself)

**Incense**:
The coloured smoke a burning **Bait** gives off at the **Pond**. It is only the visible side of **Bait**.
_Avoid_: aroma, particles

**Bite**:
The moment, a random delay after the **Incense** burns out, when a **Prey** appears and the **Catch** starts.

**Catch**:
The timed minigame at a **Pond**: the **Player** keeps the **Needle** inside the moving **Area** on the **Bar** long enough before time runs out. Success spawns the **Prey**. Failure means **Death**.
_Avoid_: minigame, catching minigame, fishing

**Bar** / **Area** / **Needle**:
The **Catch** HUD. The **Bar** is the track. The **Area** is the zone the **Prey** moves around. The **Needle** is what the **Player** steers with A/D.

**Prey**:
A creature lured from a **Pond**. It is first shown approaching the camera during the **Catch**, then spawned as a physical object the **Player** can **Grab**. It has a **Price** and a **Weight**.
_Avoid_: item, interactable, fish

**Prey Profile**:
The settings for one kind of **Prey**: its prefab, how long its **Catch** lasts, its base **Area** speed, and which **Personalities** it can have.

**Personality**:
How the **Area** moves during a **Catch** (0 steady, 1 wandering, 2 stop-and-go, 3 jittery, 4 frantic, 5 erratic). It is rolled per **Catch** from the **Prey Profile**.
_Avoid_: behaviour, AI

### Handling and selling

**Grab**:
Picking up a **Prey** with the left mouse button while `Regular`. The **Prey** floats above the ground under the cursor and moves slower the more it weighs. Releasing the button drops it.
_Avoid_: hold, drag, pick up

**Weight**:
How heavy a **Prey** is. It slows it while **Grabbed**.

**Price**:
The **Money** a **Prey** is worth when **Sold**.

**Sell**:
Turning a **Prey** into **Money** and destroying it. It happens when the **Prey** enters the **Store** trigger or rises to the **Sell Height**.
_Avoid_: deliver, cash in

**Sell Height**:
The world height (y = 10) at which a **Prey** **Sells** itself automatically. The **Lift** reaches it.

### Escape

**Escape**:
The final run. Entering its trigger releases the **Ghost**, and reaching the **Exit** before it catches the **Player** wins the game.
_Avoid_: escape sequence, chase, ending

**Ghost**:
The pursuer that appears when the **Escape** starts and walks straight at the **Player**. Touching the **Player** causes **Death**.
_Avoid_: monster, enemy

**Exit**:
The end of the **Escape** (`successTrigger`). The camera widens as the **Player** gets closer to it.
_Avoid_: success trigger, finish

## Relationships

- The **Player** has exactly one **Ball**, tied by one **Chain**.
- The **Player** holds **Money** and a **Bait** count for each of the 3 tiers.
- A **Pond** has many **Prey Profiles**. The **Bait** tier sets the odds of each one (index i of `baitNProbabilities` is the odds of `preyProfiles[i]`).
- A **Catch** involves one **Prey Profile** and one **Personality**. Success spawns one **Prey**.
- A **Prey** has one **Price** and one **Weight**. It is **Sold** at most once.
- Buying one **Chisel Kit** affects three systems: **Ball** size, **Player** speed, and **Player State**.
- There is one **Escape**, with one **Ghost** and one **Exit**.

## Example dialogue

> **Dev:** When the **Catch** fails we put the player in `Catching` state, right?
> **Domain expert:** No. `Catching` exists in the enum but nothing sets it. During the whole **Catch** the **Player** is `Interacting` with the **Pond**, and a failure is a **Death**, not a state change. If we want a real `Catching` state, we decide that now.

> **Dev:** So the cube I'm dragging onto the **Lift**: is that an "item" or an "interactable"?
> **Domain expert:** It's a **Prey**. You **Grab** it and drop it on the **Lift Platform**. The lever sends it to the **Sell Height** and it turns into **Money**. Dropping it into the **Store** is the other way to **Sell** it.

## Flagged ambiguities

- **Prey vs Item vs InteractableObject vs heldItem.** The code calls the same thing `InteractableObject` (class), `item` (`Store`, `HoldManager.heldItem`), "interactable" (`LiftPlatform`) and `Prey` (prefab folder, `Pond.SpawnPrey`, `ClaudeTasks/LiftInteractableResizing.txt`). *Proposal:* **Prey** is the domain term, and `InteractableObject` is just the component that makes it grabbable.
- **"Interact" is overloaded.** The `Interact` input (E) and the `Interacting` state are about **Buildings**. `InteractableObject` and `interactableLayer` are about mouse-**Grabbed** **Prey**. And `Lift` uses `interactableLayer` to raycast its lever. *Proposal:* "interact" means **Buildings** only, and **Grab** means **Prey**.
- **`Shopping` state is overwritten immediately.** `Store.BeginInteraction` sets `Shopping`, but `PlayerMovement.TryBeginInteraction` then sets `Interacting`. So `Shopping` (which keeps the HUD visible) never lasts. *Question:* is `Shopping` a real state or just a flavour of `Interacting`?
- **`Catching` state is unused.** It is declared and has an exit check in `PlayerStateManager`, but nothing ever enters it (see the dialogue). *Question:* remove it, or have the **Pond** set it during a **Catch**?
- **Two Sell paths.** `Store.OnTriggerEnter` and `InteractableObject.CheckSellCondition` (y ≥ 10) both **Sell**. *Question:* is selling at the **Store** intended, or is the **Lift** meant to be the only way?
- **`liftHeight` means two things.** `Lift.liftHeight` is how far the platform travels (10). `HoldManager.liftHeight` is how high a **Grabbed** **Prey** floats (1). *Proposal:* rename the second to `hoverHeight`.
- **Chisel Kit vs upgrade.** `Store.OnChiselKitBought`, `BallFollower.TotalChiselUpgrades`, and `PlayerMovement`'s "Upgrades" header with `speedIncreases`. The serialized field is misspelt `chieselKitPrice`. *Proposal:* use **Chisel Kit** everywhere.
- **Pond is not a pond.** Per `ClaudeTasks/Darkness.txt`, a **Pond** is a door and a corridor that **Prey** comes out of, and there are two cameras (`pondCamera` for choosing **Bait**, `pondCamera2` for the **Catch**). The name is historical. *Question:* keep "Pond" or rename (e.g. "Den" or "Lair")?
- **Pond UI is wired to one Pond.** The Bait buttons and Catch HUD call `Pond1.SelectBait` directly, so a duplicated **Pond** needs its own wiring, or the UI has to target whichever Pond is active. *Question:* which approach, before more Ponds are added?
- **Bait tiers are bare ints.** `baitType` is 0/1/2 throughout, with colours, prices and odds spread across `Pond`, `Store` and `UIManager` (which hard-codes 3). *Question:* do the tiers have names?
- **Escape outcome.** `SuccessfulEscape` only logs "WIN!". No win screen or end state exists yet, and nothing in the code calls it (presumably it is wired to the **Exit** trigger in the scene).
- **Death respawn.** `Die()` moves the player to `Vector3.zero` and disables a `CharacterController`, but the **Player** moves with a `Rigidbody`. The **Ball** isn't moved. *Question:* where should the respawn point be?
