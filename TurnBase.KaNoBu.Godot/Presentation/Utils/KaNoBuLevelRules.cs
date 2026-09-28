using System.Collections.Generic;
using TurnBase;
using TurnBase.KaNoBu;

public class KaNoBuLevelRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
{
    private readonly KaNoBuRules mainRules;

    public KaNoBuLevelRules(int size, bool visibleShips)
    {
        this.mainRules = new KaNoBuRules(size)
        {
            EnemyVisible = visibleShips
        };
    }

    public KaNoBuMoveResponseModel AutoMove(Field2D mainField, int playerNumber)
    {
        return this.mainRules.AutoMove(mainField, playerNumber);
    }

    public bool IsMoveValid(Field2D mainField, int playerNumber, KaNoBuMoveResponseModel move)
    {
        return this.mainRules.IsMoveValid(mainField, playerNumber, move);
    }

    public List<int> FindWinners(Field2D mainField)
    {
        return this.mainRules.FindWinners(mainField);
    }

    private Field2D field;

    public void SetInitialField(Field2D field)
    {
        this.field = field;
    }

    public Field2D generateGameField()
    {
        return this.field;
    }

    public KaNoBuInitModel GetInitModelForPlayer(int playerNumber)
    {
        return new KaNoBuInitModel(1, 1, new List<KaNoBuFigure.FigureTypes> { KaNoBuFigure.FigureTypes.ShipFlag }, this.mainRules.MaxMovesPerTurn);
    }

    public IPlayerRotator GetInitRotator()
    {
        return this.mainRules.GetInitRotator();
    }

    public int getMaxPlayersCount()
    {
        return this.mainRules.getMaxPlayersCount();
    }

    public int getMinPlayersCount()
    {
        return this.mainRules.getMinPlayersCount();
    }

    public Field2D GetFieldNotificationForPlayer(Field2D mainField, int playerNumber)
    {
        return this.mainRules.GetFieldNotificationForPlayer(mainField, playerNumber);
    }
    
    public KaNoBuMoveModel GetMoveModelForPlayer(Field2D mainField, int playerNumber)
    {
        return mainRules.GetMoveModelForPlayer(mainField, playerNumber);
    }

    public IPlayerRotator GetMoveRotator()
    {
        return mainRules.GetMoveRotator();
    }

    public KaNoBuMoveNotificationModel TryApplyMoveResponse(Field2D mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
    {
        return mainRules.TryApplyMoveResponse(mainField, playerNumber, playerMove);
    }

    public KaNoBuMoveNotificationModel GetMoveNotificationForPlayer(KaNoBuMoveNotificationModel notification, int playerNumber)
    {
        return mainRules.GetMoveNotificationForPlayer(notification, playerNumber);
    }

    public void PlayerDisconnected(Field2D mainField, int playerNumber)
    {
        mainRules.PlayerDisconnected(mainField, playerNumber);
    }

    public bool TryApplyInitResponse(Field2D mainField, int playerNumber, KaNoBuInitResponseModel playerResponse)
    {
        return true;
    }

    public void TurnCompleted(Field2D mainField)
    {
        this.mainRules.TurnCompleted(mainField);
    }
}
