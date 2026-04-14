# 🧩 Puzzle System Integration Guide

This project uses an event-driven `PuzzleManager` to handle Gear and Pulley puzzles. This allows you to trigger doors, sounds, or UI without modifying the puzzle logic itself.

## 1. Individual Puzzle Completion

If you need to trigger something when a **specific type** of puzzle is solved (e.g., a gear chain starts spinning a specific platform), use `OnPuzzleComplete`.

```csharp
void OnEnable() {
    PuzzleManager.OnPuzzleComplete += MyPuzzleHandler;
}

void OnDisable() {
    PuzzleManager.OnPuzzleComplete -= MyPuzzleHandler;
}

void MyPuzzleHandler(PuzzleType type) {
    if (type == PuzzleType.Gears) {
        // Trigger gear-specific logic
    }
}
```

## 2. Overall Level Completion

If you want to trigger the end of the level or a victory sequence, use `OnAllPuzzlesComplete`. This only fires once all required puzzles (set in the Inspector) are solved.

```csharp
void OnEnable() {
    PuzzleManager.OnAllPuzzlesComplete += OpenExitDoor;
}

void OpenExitDoor() {
    // Level is done!
}
```

## 3. Inspector Setup

- **Required Gears/Pulleys Solved:** Set these numbers to match your level design.
- **Motor Gear:** The driver gear (must have `Is Motor` checked).
- **Final Gear:** The goal gear (the manager will connect it automatically).
- **Atwood Manager:** The pulley system to monitor for balance.
