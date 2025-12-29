using System.Collections.Generic;

namespace TurnBase
{
  public interface IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
  {
    // Preparing functions.
    IField generateGameField();
    int getMaxPlayersCount();
    int getMinPlayersCount();

    // Player initialization functions.
    IPlayerRotator GetInitRotator();
    TInitModel GetInitModel(int playerNumber);
    bool TryApplyInitResponse(IField mainField, int playerNumber, TInitResponseModel playerResponse);

    // Game functions.
    IPlayerRotator GetMoveRotator();
    TMoveResponseModel AutoMove(IField mainField, int playerNumber);
    TMoveModel GetMoveModel(IField mainField, int playerNumber);
    MoveValidationStatus CheckMove(IField mainField, int playerNumber, TMoveResponseModel move);
    TMoveNotificationModel MakeMove(IField mainField, int playerNumber, TMoveResponseModel playerMove);
    void TurnCompleted(IField mainField);
    List<int> findWinners(IField mainField);
    void PlayerDisconnected(IField mainField, int playerNumber);
  }
}