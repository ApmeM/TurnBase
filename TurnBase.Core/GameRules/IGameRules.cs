namespace TurnBase.Core;

public interface IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
{
      // Preparing functions.
    IField generateGameField();
    int getMaxPlayersCount();
    int getMinPlayersCount();
    IPlayerRotator getRotator();

    // Player initialization functions.
    TInitModel GetInitModel(int playerNumber);
    bool TryApplyInitResponse(IField mainField, int playerNumber, TInitResponseModel playerResponse);

    // Game functions.
    TMoveResponseModel? AutoMove(IField mainField, int playerNumber);
    TMoveModel GetMoveModel(IField mainField, int playerNumber);
    MoveValidationStatus CheckMove(IField mainField, int playerNumber, TMoveResponseModel move);
    TMoveNotificationModel MakeMove(IField mainField, int playerNumber, TMoveResponseModel playerMove);
    List<int>? findWinners(IField mainField);
}