namespace TurnBase
{
    public interface IField
    {
        int Width { get; }
        int Height { get; }

        IFigure get(int fromX, int fromY);

        SetStatus trySet(int toX, int toY, IFigure figure);

        IField copyForPlayer(int playerNumber = -1);

        bool IsInBounds(Point point);
    }
}