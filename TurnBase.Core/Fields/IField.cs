namespace TurnBase.Core;

public interface IField
{
    public enum SetStatus
    {
        OK,
        OUT_OF_BOUNDS,
        OCCUPIED,
        READ_ONLY,
    }

    int Width { get; }
    int Height { get; }

    IFigure? get(Point from);

    SetStatus trySet(Point to, IFigure? figure);

    IField copyField();
    
    bool IsInBounds(Point point);
}
