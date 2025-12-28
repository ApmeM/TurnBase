namespace TurnBase.Core
{
    public class GameEventListenerConnector
    {
        public static void Connect<TMoveNotificationModel>(
            IGameEvents<TMoveNotificationModel> game,
            IGameEventListener<TMoveNotificationModel> listener)
        {
            game.GameStarted += listener.GameStarted;
            game.GamePlayerInitialized += listener.GamePlayerInitialized;
            game.GamePlayerWrongTurn += listener.GamePlayerWrongTurn;
            game.GameTurnFinished += listener.GameTurnFinished;
            game.GamePlayerDisconnected += listener.GamePlayerDisconnected;
            game.GameFinished += listener.GameFinished;
            game.GamePlayerTurn += listener.GamePlayerTurn;
        }

        public static void Connect<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel>(
            IGameLogEvents<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel> game,
            IGameLogEventListener<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel> listener)
        {
            game.GameLogStarted += listener.GameLogStarted;
            game.GameLogPlayerInitialized += listener.GameLogPlayerInitialized;
            game.GameLogPlayerWrongTurn += listener.GameLogPlayerWrongTurn;
            game.GameLogTurnFinished += listener.GameLogTurnFinished;
            game.GameLogFinished += listener.GameLogFinished;
            game.GameLogPlayerTurn += listener.GameLogPlayerTurn;
            game.GameLogPlayerDisconnected += listener.GameLogPlayerDisconnected;
        }
    }
}