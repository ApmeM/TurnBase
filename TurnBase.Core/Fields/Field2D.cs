using System.Collections.Generic;

namespace TurnBase.Core
{
    public class Field2D : IField
    {
        Dictionary<Point, IFigure> realField;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Field2D(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.realField = new Dictionary<Point, IFigure>();
        }

        public IFigure get(Point from)
        {
            if (!realField.ContainsKey(from))
            {
                return null;
            }

            return realField[from];
        }

        public SetStatus trySet(Point to, IFigure ship)
        {
            if (to.X < 0 || to.X >= Width || to.Y < 0 || to.Y >= Height)
            {
                return SetStatus.OUT_OF_BOUNDS;
            }

            if (ship != null && this.realField.ContainsKey(to))
            {
                return SetStatus.OCCUPIED;
            }

            if (ship == null)
            {
                this.realField.Remove(to);
                return SetStatus.OK;
            }
            else
            {
                this.realField[to] = ship;
                return SetStatus.OK;
            }
        }

        public IField copyField()
        {
            var result = new Field2D(this.Width, this.Height);
            foreach (var point in this.realField.Keys)
            {
                result.trySet(point, this.realField[point]);
            }
            return result;
        }

        public bool IsInBounds(Point point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }
    }
}