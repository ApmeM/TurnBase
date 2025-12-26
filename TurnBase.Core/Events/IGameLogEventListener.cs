namespace TurnBase.Core;

public interface IGameLogEventListener<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel>
{
    public void GameLogStarted(IField field);
    public void GameLogPlayerInitialized(int playerNumber, InitResponseModel<TInitResponseModel> initResponseModel, IField field);
    public void GameLogPlayerDisconnected(int playerNumber, IField field);
    public void GameLogTurnFinished(IField field);
    public void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, TMoveResponseModel moveResponseModel, IField field);
    public void GameLogPlayerTurn(int playerNumber, TMoveNotificationModel moveNotificationModel, TMoveResponseModel moveResponseModel, IField field);
    public void GameLogFinished(List<int> winners, IField field);
}

