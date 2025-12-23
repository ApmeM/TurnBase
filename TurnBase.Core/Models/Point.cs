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

public enum MoveValidationStatus
{
    OK,
    ERROR_COMMUNICATION,
    ERROR_OUTSIDE_FIELD,
    ERROR_INVALID_MOVE,
    ERROR_FIELD_OCCUPIED
}
