# TurnBaseCore Technical Architecture

This document describes the core components of the library kernel.

## 1. Data Structures

### `Position` (struct)
A simple structure for storing coordinates. Used for passing positions during pathfinding and attacks.
*   **`int X`**: Horizontal position.
*   **`int Y`**: Vertical position.

### `Cell` (struct)
Represents a single cell of the game board.
*   **`int TerrainType`**: Terrain type (e.g., 0 — grass, 1 — wall, 2 — water). Using `int` allows for easy extension of types without changing the core.
*   **`IFigure? Occupant`**: Reference to the figure standing in this cell. Can be `null` if the cell is free.

### `Board2D` (struct)
Represents current field state.
*   **State**:
    *   **`int PlayerId`**: ID of the player who owns this figure.
    *   **`int Width`**: Width of the field.
    *   **`int Height`**: Height of the field.
    *   **`int NextPlayerIdTurn`**: ID of the player who should do the next turn on the board.
*   **Methods**:
    *   **`bool InBounds(Position p)`**: Returns is position within board
    *   **`Cell GetCell(Position p)`**: Returns the cell at the position or throw an exception if not within bounds.
    *   **`int GetTerrainType(Position p)`**: Returns the terrain type of the cell at the specified position.
    *   **`IFigure? GetOccupant(Position p)`**: Returns the figure at the specified position, or null if empty.
    *   **`void SetOccupant(Position p, IFigure? figure)`**: Places a figure at the specified position or throw an exception if not within bounds.
    *   **`void SetTerrainType(Position p, int terrainType)`**: Sets the terrain type for the cell at the specified position or throw an exception if not within bounds.

## 2. Interfaces

### `IFigure`
Base interface for all game objects (for example pawns, heroes, monsters).
*   **State**:
    *   **`int PlayerId`**: ID of the player who owns this figure.
*   **Methods**:
    *   **`bool CheckPossibleMove(Board2D board, Position from, Position to)`**: Checks if the specified move is possible.

## 3. Core Logic

### `TurnBaseGame`
The main entry point for external code (UI, controllers). Manages the game state and turn order.
*   **Initialization (constructor)**:
    * Generates board with specific width and height
    * Fills it with default terrain type
    * Specifies players count
*   **State**:
    *   **`int PlayersCount`**: Numer of players in the game. Player Ids should be within the range [0, PlayersCount-1].
*   **Methods**:
    *   **`bool TryMakeMove(Position from, Position to)`**: Attempts to make a move. If successful and valid for the current player, updates the grid state and switches the turn.
    *   **`void NextTurn()`**: Manually switches the turn to the next player.