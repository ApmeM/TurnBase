namespace TurnBase.Core;

public interface IPointRotator
{
    Point getRotatedPoint(IField field, Point point, int playerNumber);
}
