namespace TurnBase.Core;

public interface IField
{
    IFigure? get(Point from);

    bool trySet(Point to, IFigure? figure);

    int getWidth();

    int getHeight();

    IField copyField();
}
