using System.Collections.Generic;
using TurnBase;
using TurnBase.KaNoBu;

public class KaNoBuLevelRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    private readonly KaNoBuRules mainRules;

    public KaNoBuLevelRules(int size, bool visibleShips)
    {
        this.mainRules = new KaNoBuRules(size);
        this.mainRules.AllFiguresVisible = visibleShips;
    }

    public KaNoBuMoveResponseModel AutoMove(IField mainField, int playerNumber)
    {
        return this.mainRules.AutoMove(mainField, playerNumber);
    }

    public MoveValidationStatus CheckMove(IField mainField, int playerNumber, KaNoBuMoveResponseModel move)
    {
        return this.mainRules.CheckMove(mainField, playerNumber, move);
    }

    public List<int> findWinners(IField mainField)
    {
        return this.mainRules.findWinners(mainField);
    }

    private Field2D field;

    public void SetInitialField(Field2D field)
    {
        this.field = field;
    }

    public IField generateGameField()
    {
        return this.field.copyForPlayer(-1);
    }

    public KaNoBuInitModel GetInitModel(int playerNumber)
    {
        return new KaNoBuInitModel(1, 1, new List<KaNoBuFigure.FigureTypes> { KaNoBuFigure.FigureTypes.ShipFlag });
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

    public KaNoBuMoveModel GetMoveModel(IField mainField, int playerNumber)
    {
        return mainRules.GetMoveModel(mainField, playerNumber);
    }

    public IPlayerRotator GetMoveRotator()
    {
        return mainRules.GetMoveRotator();
    }

    public KaNoBuMoveNotificationModel MakeMove(IField mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
    {
        return mainRules.MakeMove(mainField, playerNumber, playerMove);
    }

    public void PlayerDisconnected(IField mainField, int playerNumber)
    {
        mainRules.PlayerDisconnected(mainField, playerNumber);
    }

    public bool TryApplyInitResponse(IField mainField, int playerNumber, KaNoBuInitResponseModel playerResponse)
    {
        return true;
    }

    public void TurnCompleted(IField mainField)
    {
        this.mainRules.TurnCompleted(mainField);
    }
}
