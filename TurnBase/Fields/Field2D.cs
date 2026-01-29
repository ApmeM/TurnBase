namespace TurnBase
{
    public enum SetStatus
    {
        OK,
        OUT_OF_BOUNDS,
        OCCUPIED,
        READ_ONLY,
    }

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

        public IFigure get(Point from)
        {
            if (!IsInBounds(from))
            {
                return null;
            }

            return realField[from.X, from.Y];
        }

        public SetStatus trySet(Point to, IFigure ship)
        {
            if (!IsInBounds(to))
            {
                return SetStatus.OUT_OF_BOUNDS;
            }

            if (ship != null && this.realField[to.X, to.Y] != null)
            {
                return SetStatus.OCCUPIED;
            }

            if (ship == null)
            {
                this.realField[to.X, to.Y] = null;
                return SetStatus.OK;
            }
            else
            {
                this.realField[to.X, to.Y] = ship;
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
                        result.realField[x, y] = figure.CopyForPlayer(PlayerId);
                    }
                }
            }
            return result;
        }

        public bool IsInBounds(Point point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }

        public Point RotatePointFor2Players(Point point, int playerNumber)
        {
            if (playerNumber > 1)
            {
                return default;
            }

            playerNumber = playerNumber % 2;

            int mainWidth = this.Width - 1;
            int mainHeight = this.Height - 1;

            int x = playerNumber * mainWidth - (playerNumber * 2 - 1) * point.X;
            int y = playerNumber * mainHeight - (playerNumber * 2 - 1) * point.Y;

            return new Point { X = x, Y = y };
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
                    var ship = this.realField[j, i];
                    result += $" {ship?.ToString() ?? "  "}";
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