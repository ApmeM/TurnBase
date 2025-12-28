namespace TurnBase.Core
{
    
    public interface IField
    {

        int Width { get; }
        int Height { get; }

        IFigure get(Point from);

        SetStatus trySet(Point to, IFigure figure);

        IField copyField();

        bool IsInBounds(Point point);
    }
}