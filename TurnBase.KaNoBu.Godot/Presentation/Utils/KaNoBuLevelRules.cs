using System.Collections.Generic;
using TurnBase;
using TurnBase.KaNoBu;

public class KaNoBuLevelRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
{
    private readonly KaNoBuRules mainRules;

    public KaNoBuLevelRules(int size, bool visibleShips)
    {
        this.mainRules = new KaNoBuRules(size);
        if(!visibleShips)
        {
            this.mainRules.HideEnemyShips();
        }
    }

    public KaNoBuMoveResponseModel AutoMove(Field2D mainField, int playerNumber)
    {
        return this.mainRules.AutoMove(mainField, playerNumber);
    }

    public bool IsMoveValid(Field2D mainField, int playerNumber, KaNoBuMoveResponseModel move)
    {
        return this.mainRules.IsMoveValid(mainField, playerNumber, move);
    }

    public List<int> findWinners(Field2D mainField)
    {
        return this.mainRules.findWinners(mainField);
    }

    private Field2D field;

    public void SetInitialField(Field2D field)
    {
        this.field = field;
    }

    public Field2D generateGameField()
    {
        return new SimpleCopier().CopyForPlayer(this.field, -1);
    }

    public KaNoBuInitModel GetInitModel(int playerNumber)
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
    
    public IFieldCopier<Field2D>[] GetFieldCopiers()
    {
        return new IFieldCopier<Field2D>[] { new HideEnemyCopier() };
    }

    public KaNoBuMoveModel GetMoveModel(Field2D mainField, int playerNumber)
    {
        return mainRules.GetMoveModel(mainField, playerNumber);
    }

    public IPlayerRotator GetMoveRotator()
    {
        return mainRules.GetMoveRotator();
    }

    public KaNoBuMoveNotificationModel MakeMove(Field2D mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
    {
        return mainRules.MakeMove(mainField, playerNumber, playerMove);
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
