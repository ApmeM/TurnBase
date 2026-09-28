using System.Collections.Generic;

namespace TurnBase
{
  public interface IGameRules<
    TInitModel,
    TInitResponseModel,
    TMoveModel,
    TMoveResponseModel,
    TMoveNotificationModel,
    TField>
  {
    // Preparing functions.
    TField generateGameField();
    int getMaxPlayersCount();
    int getMinPlayersCount();

    // Player initialization functions.
    IPlayerRotator GetInitRotator();
    TInitModel GetInitModelForPlayer(int playerNumber);
    bool TryApplyInitResponse(TField mainField, int playerNumber, TInitResponseModel playerResponse);

    // Game functions.
    IPlayerRotator GetMoveRotator();
    TMoveResponseModel AutoMove(TField mainField, int playerNumber);
    TMoveModel GetMoveModelForPlayer(TField mainField, int playerNumber);
    bool IsMoveValid(TField mainField, int playerNumber, TMoveResponseModel move);
    TMoveNotificationModel TryApplyMoveResponse(TField mainField, int playerNumber, TMoveResponseModel playerMove);

    // Notifications
    TMoveNotificationModel GetMoveNotificationForPlayer(TMoveNotificationModel notification, int playerNumber);
    TField GetFieldNotificationForPlayer(TField mainField, int playerNumber);
    void PlayerDisconnected(TField mainField, int playerNumber);
    
    // EndGame
    void TurnCompleted(TField mainField);
    List<int> FindWinners(TField mainField);
  }
}