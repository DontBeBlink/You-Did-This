# Game Design Document: You Did This

## Core Concept
A puzzle platformer where players create "clones" to solve spatial and timing puzzles. Each clone repeats the player’s previous actions, and is sometimes the key to progress or an obstacle to avoid.

## Main Mechanic
- The player can "hop" to a new character (clone) at any time, leaving the previous clone to continue their recorded actions.
- Once a clone reaches a goal, it becomes permanently stuck in place and cannot be returned to.
- The player can "retract" to the previous clone (erasing the current one), unless that clone is already stuck at a goal.

## Gameplay Flow
1. Start as a single character.
2. Navigate platforms, switches, and obstacles.
3. When needed, create a clone:
    - Old clone replays your recorded actions on a loop (or just continues to the goal).
    - You take control of the new clone to solve the next part of the puzzle.
4. Reach a goal with a clone:
    - That clone becomes stuck and acts as a static object (e.g., holding a switch down).
    - You cannot retract to clones that have reached their goal.
5. Use retraction to undo mistakes:
    - Erase your current clone and return to the previous one (unless it’s stuck).
    - Useful for experimenting and iterating on solutions.

## Puzzle Elements
- Pressure plates/switches that must be held down.
- Doors or platforms activated by stuck clones.
- Obstacles requiring precise timing between clones.
- Hazards that require coordination between active and looping clones.

## Win/Lose Conditions
- Win: Get all required clones to their goal locations.
- Lose: All clones are stuck or destroyed, and you cannot progress.

## Visual/Audio Style
- Minimalist, clear silhouettes for clones.
- Distinct colors or effects to indicate stuck/active/looping clones.
- Subtle audio cues when creating, retracting, or sticking a clone.

## Title
**You Did This**

---

### Optional Extensions
- Leaderboards for fewest clones used or fastest completion.
- Secret collectibles requiring clever clone use.
- Advanced puzzles with time-based mechanics.
