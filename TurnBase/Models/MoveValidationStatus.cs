namespace TurnBase
{
    public enum MoveValidationStatus
    {
        OK,
        ERROR_OUTSIDE_FIELD,
        ERROR_INVALID_FIGURE_MOVE,
        ERROR_FIELD_OCCUPIED,
        ERROR_MOVE_FROM_NOWHERE,
        ERROR_INVALID_PLAYER
    }
}