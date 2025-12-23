using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class KaNoBuInitModel
{
    public KaNoBuInitModel(FieldReadOnly preparingField, List<IFigure> availableFigures)
    {
        PreparingField = preparingField;
        AvailableFigures = availableFigures;
    }

    public readonly FieldReadOnly PreparingField;
    public readonly List<IFigure> AvailableFigures;
}

public class KaNoBuInitResponseModel
{
    public KaNoBuInitResponseModel(IField preparedField)
    {
        PreparedField = preparedField;
    }

    public readonly IField PreparedField;
}

public class KaNoBuMoveModel
{
    public KaNoBuMoveModel(IField field)
    {
        Field = field;
    }

    public readonly IField Field;
}

public class KaNoBuMoveResponseModel
{
    public KaNoBuMoveResponseModel(MoveStatus status, Point from, Point to)
    {
        Status = status;
        From = from;
        To = to;
    }

    public KaNoBuMoveResponseModel(MoveStatus status)
    {
        Status = status;
        From = new Point();
        To = new Point();
    }

    public enum MoveStatus
    {
        MAKE_TURN,
        SKIP_TURN
    }
    
    public readonly MoveStatus Status;
    public readonly Point From;
    public readonly Point To;
}

public class KaNoBuMoveNotificationModel
{
    public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move, List<IFigure>? attackers, List<IFigure>? defenders, List<IFigure>? winners)
    {
        this.move = move;
        this.attackers = attackers;
        this.defenders = defenders;
        this.winners = winners;
    }
    public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move)
    {
        this.move = move;
        this.attackers = null;
        this.defenders = null;
        this.winners = null;
    }
    public readonly KaNoBuMoveResponseModel move;
    public readonly List<IFigure>? attackers;
    public readonly List<IFigure>? defenders;
    public readonly List<IFigure>? winners;
}
