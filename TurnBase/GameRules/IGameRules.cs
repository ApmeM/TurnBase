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
    IFieldCopier<TField>[] GetFieldCopiers();

    // Player initialization functions.
    IPlayerRotator GetInitRotator();
    TInitModel GetInitModel(int playerNumber);
    bool TryApplyInitResponse(TField mainField, int playerNumber, TInitResponseModel playerResponse);

    // Game functions.
    IPlayerRotator GetMoveRotator();
    TMoveResponseModel AutoMove(TField mainField, int playerNumber);
    TMoveModel GetMoveModel(TField mainField, int playerNumber);
    bool IsMoveValid(TField mainField, int playerNumber, TMoveResponseModel move);
    TMoveNotificationModel MakeMove(TField mainField, int playerNumber, TMoveResponseModel playerMove);
    TMoveNotificationModel GetMoveNotificationForPlayer(TMoveNotificationModel notification, int playerNumber);
    void TurnCompleted(TField mainField);
    List<int> findWinners(TField mainField);
    void PlayerDisconnected(TField mainField, int playerNumber);
  }
}