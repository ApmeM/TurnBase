namespace TurnBase
{
    public interface IField2D
    {
        int Width { get; }
        int Height { get; }

        IFigure GetFigure(Point key);
        IFigure GetFigure(int x, int y);
        bool GetWall(Point key);
        bool GetWall(int x, int y);
        bool IsInBounds(Point point);
        void SetFigure(Point key, IFigure value);
        void SetFigure(int x, int y, IFigure value);
        void SetWall(Point key, bool value);
        void SetWall(int x, int y, bool value);
    }
}