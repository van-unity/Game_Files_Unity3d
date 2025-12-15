# Refactoring

1. In some places probably would be better to implement Zenject’s IInitializable interface, but I decided to not to to initialize those things whenever I actually need.
2. There is no singleton in current implementation. 
   As for the board size for example, I imagine that in a real implementation we will have some Level data and we will create board with the size of level(maybe with some additional data like gem types that can be used in particular level))
3. For matching the bomb(special piece) I'm a confused because the original task description says that
the special piece can match with same type of pieces or with 2 or more regular piecees with the same color.
I implemented the original requirement but changing it to any other logic is a matter of changing only the BombAbility(in current case) so the change is easy!
4. I didn't create factories for all entities that are being created by new because that doesn't add any value in this case

# Match-3 Game Implementation

I'm pleased to submit my work for this match-3 assignment. Thank you for the interesting test opportunity.

My primary focus throughout this refactoring process was **changeability and extensibility**. The resulting architecture supports rapid prototyping, enabling easy experimentation with gameplay mechanics and algorithms. Once the core design is validated, the structure is ready for further development, including:

* More complex dependency injection
* Comprehensive unit testing
* Feature expansion without modifying core logic

---

## Architectural Overview — Prioritizing Change (SOLID)

The most important principle guiding the refactoring was the **Open/Closed Principle (OCP)**.
The Strategy Pattern plays a key role in achieving that.

### Key Strategy Interfaces Used

| Interface                                           | Responsibility                                                   |
| --------------------------------------------------- | ---------------------------------------------------------------- |
| `IMatchCheckStrategy`                               | Decouples match detection behaviors (initialization vs gameplay) |
| `IBoardInitializeStrategy` / `IBoardRefillStrategy` | Separates initialization and refill logic                        |
| `IGemAbility`                                       | Encapsulates special gem abilities (e.g., Bomb)                  |

### Benefits

* Core logic remains unchanged when adding new behaviors
* Specialized behaviors live in isolated classes
* Feature variations = implementation-only changes

> Example: Adding a new special gem only requires a new `IGemAbility` implementation
> The `GameController` never changes.

---

## Dependency Resolution

Dependencies are resolved through the `GameInitializer`, acting as a lightweight initializer.

Key choices:

* Non-MonoBehaviour logic receives dependencies via constructors
* MonoBehaviours remain minimal, mostly visual
* Easy future migration to frameworks like **Zenject**

This architecture is strong enough for iteration, but prepared for scaling.

---

## Feature Implementation Summary

---

### 1. Input Management Refactoring

The initial design handled input in `SC_Gem`, mixing view, interaction, and logic — violating SRP.

#### Solution

Introduced a dedicated **InputManager**.

#### Benefits

1. `GemView` now handles only visuals
2. Removed collision-based input (`OnMouseDown`, etc.) → performance gain possible
3. Implements **Observer Pattern**

```
InputManager -> event Swiped -> GameController
```

Fully decoupled — easily testable and replaceable.

---

### 2. Modern Sequential Flow

#### Tasks instead of Coroutines

The gameplay sequence:

```
Swap
→ Animate swap
→ Check match
→ Destroy gems
→ Refill
→ Animate new positions
```

is implemented using:

* `async/await`
* `Task`

This provides:

✓ clarity
✓ maintainability
✓ precise sequencing

**Exception:** `GemDestroyEffect` still uses Coroutines — ideal for simple effect-timers.

---

### 3. Gem Pooling System (Task 2)

To reduce allocation spikes, I built a reusable Object Pool system.

#### Components

* `IObjectPoolBase`
* `MonoBehaviourObjectPool`
* `GemPool`

What is pooled?

* `GemView`
* `GemDestroyEffect`

#### Result

* Almost zero runtime instantiation
* No unnecessary garbage collection

The pattern is scalable into:

* automatic pool factories
* pool warm-up
* pre-loading per level

---

### 4. Special Piece — Bomb (Task 3)

Implemented using:

* `IGemAbility` (interface)
* `BombAbility` (concrete strategy)

#### Creation

Bomb is spawned when match length ≥ 4
Handled before gravity/drop begins.

#### Execution Behavior

Bomb destroys:

* its own tile,
* 3×3 area around it,
* plus cross-shaped neighbors.

All destruction sequencing is preserved visually.

#### Key advantage

Core match logic does not know anything about bombs.

---

### 5. Cascading Gem Drop Logic (Tasks 1 & 5)

Challenges addressed:

✔ avoid accidental new matches during refill
✔ ensure animated staggered fall timing
✔ correctly resolve cascade states

Implemented in `BoardRefillStrategy`.

#### Animation System

Using DG.Tweening:

* delays based on creation timestamp
* gems fall in a chain-like stagger

Example logic:

> Later-spawned gems should appear to fall later
> even if drop duration is identical

There is room for minor timing refinement,
but structurally the system is correct.

---

## Summary of Improvements

✔ Architecture modularized
✔ Core game loop readable & sequenced
✔ Input separated from visuals
✔ Object pooling implemented
✔ Special gem behavior isolated & extendable
✔ Cascading animation logic implemented
✔ Dependency management clean and future-ready


### Thank you again for the assignment.
