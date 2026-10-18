using System.Collections.Generic;
using TurnBase;
using TurnBase.KaNoBu;

public class KaNoBuLevelRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>
{
    private readonly KaNoBuRules mainRules;

    public KaNoBuLevelRules(int size, bool visibleShips)
    {
        this.mainRules = new KaNoBuRules(size)
        {
            EnemyVisible = visibleShips
        };
    }

    public KaNoBuMoveResponseModel AutoMove(KaNoBuField mainField, int playerNumber)
    {
        return this.mainRules.AutoMove(mainField, playerNumber);
    }

    public bool IsMoveValid(KaNoBuField mainField, int playerNumber, KaNoBuMoveResponseModel move)
    {
        return this.mainRules.IsMoveValid(mainField, playerNumber, move);
    }

    public List<int> FindWinners(KaNoBuField mainField)
    {
        return this.mainRules.FindWinners(mainField);
    }

    private Field2D field;

    public void SetInitialField(Field2D field)
    {
        this.field = field;
    }

    public KaNoBuField GenerateGameField()
    {
        return new KaNoBuField
        {
            Field = this.field,
            DisconnectedPlayers = new List<int>()
        };
    }

    public KaNoBuInitModel GetInitModelForPlayer(int playerNumber)
    {
        return new KaNoBuInitModel(1, 1, new List<KaNoBuFigure.FigureTypes> { KaNoBuFigure.FigureTypes.ShipFlag }, this.mainRules.MaxMovesPerTurn);
    }

    public IPlayerRotator GetInitRotator()
    {
        return this.mainRules.GetInitRotator();
    }

    public int GetMaxPlayersCount()
    {
        return this.mainRules.GetMaxPlayersCount();
    }

    public int GetMinPlayersCount()
    {
        return this.mainRules.GetMinPlayersCount();
    }

    public KaNoBuField GetFieldNotificationForPlayer(KaNoBuField mainField, int playerNumber)
    {
        return this.mainRules.GetFieldNotificationForPlayer(mainField, playerNumber);
    }

    public KaNoBuMoveModel GetMoveModelForPlayer(KaNoBuField mainField, int playerNumber)
    {
        return mainRules.GetMoveModelForPlayer(mainField, playerNumber);
    }

    public IPlayerRotator GetMoveRotator()
    {
        return mainRules.GetMoveRotator();
    }

    public KaNoBuMoveNotificationModel TryApplyMoveResponse(KaNoBuField mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
    {
        return mainRules.TryApplyMoveResponse(mainField, playerNumber, playerMove);
    }

    public KaNoBuMoveNotificationModel GetMoveNotificationForPlayer(KaNoBuMoveNotificationModel notification, int playerNumber)
    {
        return mainRules.GetMoveNotificationForPlayer(notification, playerNumber);
    }

    public void PlayerDisconnected(KaNoBuField mainField, int playerNumber)
    {
        mainRules.PlayerDisconnected(mainField, playerNumber);
    }

    public bool TryApplyInitResponse(KaNoBuField mainField, int playerNumber, KaNoBuInitResponseModel playerResponse)
    {
        return true;
    }

    public void TurnCompleted(KaNoBuField mainField)
    {
        this.mainRules.TurnCompleted(mainField);
    }
}
