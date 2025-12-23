namespace TurnBase.Core;

public interface IGameEvents
{
    event Action<IField>? GameStarted;
    event Action<int, string>? GamePlayerInitialized;
    event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
    event Action<List<int>>? GameFinished;
    event Action<int, Move, MoveResult?>? GamePlayerTurn;
}