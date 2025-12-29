namespace TurnBase
{
    public enum MoveValidationStatus
    {
        OK,
        ERROR_COMMUNICATION,
        ERROR_OUTSIDE_FIELD,
        ERROR_INVALID_MOVE,
        ERROR_FIELD_OCCUPIED
    }
}