namespace TurnBase.Core
{
    public class FieldConcealer : IField
    {
        private IField mainField;
        private int playerNumber;

        public int Width => this.mainField.Width;
        public int Height => this.mainField.Height;

        public FieldConcealer(IField mainField, int playerNumber)
        {
            this.mainField = mainField;
            this.playerNumber = playerNumber;
        }

        public IFigure get(Point from)
        {
            var figure = this.mainField.get(from);
            if (figure == null)
            {
                return null;
            }

            if (figure.PlayerId != playerNumber)
            {
                figure = new UnknownFigure(figure.PlayerId);
            }

            return figure;
        }

        public SetStatus trySet(Point to, IFigure figure)
        {
            return this.mainField.trySet(to, figure);
        }

        public IField copyField()
        {
            return new FieldConcealer(this.mainField.copyField(), this.playerNumber);
        }

        public bool IsInBounds(Point point)
        {
            return this.mainField.IsInBounds(point);
        }
    }
}