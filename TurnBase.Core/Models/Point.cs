namespace TurnBase.Core;

public struct Point
{
    public int X;
    public int Y;

    override public string ToString()
    {
        return $"{X}x{Y}";
    }
}

public enum MoveStatus {
    MAKE_TURN,
    SKIP_TURN
}

public enum MoveValidationStatus {
    OK,
    ERROR_OUTSIDE_FIELD,
    ERROR_INVALID_MOVE,
    ERROR_FIELD_OCCUPIED
}

public struct Move
{
    public Point From;
    public Point To;
}

public struct AutoMove {
    public enum AutoMoveStatus {
        NONE,
        MAKE_TURN,
        SKIP_TURN
    }
    public Move? move;
    public AutoMoveStatus status;
}

public struct BattleResult {
    public List<IFigure> attackers;
    public List<IFigure> defenders;
    public List<IFigure> winners;
}

public struct PlayerInitialization {
    public IField preparingField;
    public List<IFigure> availableFigures;
}