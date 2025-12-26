namespace TurnBase.Core;

public interface IGameLogEvents<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel>
{
    public event Action<IField>? GameLogStarted;
    public event Action<int, InitResponseModel<TInitResponseModel>, IField>? GameLogPlayerInitialized;
    public event Action<int, IField>? GameLogPlayerDisconnected;
    public event Action<IField>? GameLogTurnFinished;
    public event Action<int, MoveValidationStatus, TMoveResponseModel, IField>? GameLogPlayerWrongTurn;
    public event Action<int, TMoveNotificationModel, TMoveResponseModel, IField>? GameLogPlayerTurn;
    public event Action<List<int>, IField>? GameLogFinished;
}
