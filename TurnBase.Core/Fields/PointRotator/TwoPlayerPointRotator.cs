namespace TurnBase.Core;

public class TwoPlayerPointRotator : IPointRotator
{
    public Point getRotatedPoint(IField mainField, Point point, int playerNumber)
    {
        playerNumber = playerNumber % 2;

        int mainWidth = mainField.getWidth() - 1;
        int mainHeight = mainField.getHeight() - 1;

        int x = playerNumber * mainWidth - (playerNumber * 2 - 1) * point.X;
        int y = playerNumber * mainHeight - (playerNumber * 2 - 1) * point.Y;

        return new Point { X = x, Y = y };
    }
}
