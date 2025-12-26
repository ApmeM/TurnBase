namespace TurnBase.Core;
public interface IGameEvents<TMoveNotificationModel>
{
    event Action? GameStarted;
    event Action<int, string>? GamePlayerInitialized;
    event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
    event Action<List<int>>? GameFinished;
    event Action? GameTurnFinished;
    event Action<int>? GamePlayerDisconnected;
    event Action<int, TMoveNotificationModel>? GamePlayerTurn;
}