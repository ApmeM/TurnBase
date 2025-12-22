namespace TurnBase.Core;

public class Field2D : IField
{
    Dictionary<Point, IFigure> realField;
    private int width;
    private int height;

    public Field2D(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.realField = new Dictionary<Point, IFigure>();
    }

    public IFigure? get(Point from)
    {
        if (!realField.ContainsKey(from))
        {
            return null;
        }

        return realField[from];
    }

    public bool trySet(Point to, IFigure? ship)
    {
        if (to.X < 0 || to.X >= width || to.Y < 0 || to.Y >= height)
        {
            return false;
        }

        if (ship != null && this.realField.ContainsKey(to))
        {
            return false;
        }

        if (ship == null)
        {
            this.realField.Remove(to);
            return true;
        }
        else
        {
            this.realField[to] = ship;
            return true;
        }
    }

    public int getHeight()
    {
        return height;
    }

    public int getWidth()
    {
        return width;
    }

    public IField copyField()
    {
        var result = new Field2D(this.width, this.height);
        foreach (var point in this.realField.Keys)
        {
            result.trySet(point, this.realField[point]);
        }
        return result;
    }
}
