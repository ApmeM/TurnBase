namespace TurnBase
{
    public class Field2D : IField
    {
        public IFigure[,] realField;
        public int Width => this.realField.GetLength(0);
        public int Height => this.realField.GetLength(1);

        public static Field2D Create(int width, int height)
        {
            return new Field2D(new IFigure[width, height]);
        }

        public Field2D(IFigure[,] realField)
        {
            this.realField = realField;
        }

        public IFigure get(int fromX, int fromY)
        {
            if (fromX < 0 || fromX >= Width || fromY < 0 || fromY >= Height)
            {
                return null;
            }

            return realField[fromX, fromY];
        }

        public SetStatus trySet(int toX, int toY, IFigure ship)
        {
            if (toX < 0 || toX >= Width || toY < 0 || toY >= Height)
            {
                return SetStatus.OUT_OF_BOUNDS;
            }

            if (ship != null && this.realField[toX, toY] != null)
            {
                return SetStatus.OCCUPIED;
            }

            if (ship == null)
            {
                this.realField[toX, toY] = null;
                return SetStatus.OK;
            }
            else
            {
                this.realField[toX, toY] = ship;
                return SetStatus.OK;
            }
        }

        public IField copyForPlayer(int PlayerId)
        {
            var result = Field2D.Create(this.Width, this.Height);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    var figure = this.realField[x, y];
                    if (figure != null)
                    {
                        result.trySet(x, y, figure.CopyForPlayer(PlayerId));
                    }
                }
            }
            return result;
        }

        public bool IsInBounds(Point point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }

        public override string ToString()
        {
            string result = "";
            result += string.Format("   ");
            for (int j = 0; j < this.Width; j++)
            {
                result += $"  {(char)('A' + j)}";
            }
            result += string.Format("   ");
            result += "\n";

            for (int i = 0; i < this.Height; i++)
            {
                result += $"  {i}";
                for (int j = 0; j < this.Width; j++)
                {
                    var ship = this.get(j, i);
                    result += $" {ship?.PrintableName() ?? "  "}";
                }

                result += $"   {i}\n";
            }

            result += string.Format("   ");
            for (int j = 0; j < this.Width; j++)
            {
                result += $"  {(char)('A' + j)}";
            }
            result += string.Format("   ");

            return result;
        }
    }
}