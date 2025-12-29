namespace TurnBase
{
    public interface IPointRotator
    {
        Point RotatePoint(IField field, Point point, int playerNumber);
    }
}