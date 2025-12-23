namespace TurnBase.Core;

public class TwoPlayerPointRotator : IPointRotator
{
    public Point RotatePoint(IField mainField, Point point, int playerNumber)
    {
        playerNumber = playerNumber % 2;

        int mainWidth = mainField.Width - 1;
        int mainHeight = mainField.Height - 1;

        int x = playerNumber * mainWidth - (playerNumber * 2 - 1) * point.X;
        int y = playerNumber * mainHeight - (playerNumber * 2 - 1) * point.Y;

        return new Point { X = x, Y = y };
    }
}
