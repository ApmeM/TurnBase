namespace TurnBase
{
    public class FieldReadOnly : IField
    {
        private IField mainField;

        public FieldReadOnly(IField mainField)
        {
            this.mainField = mainField;
        }

        public IFigure get(Point from)
        {
            return this.mainField.get(from);
        }

        public SetStatus trySet(Point to, IFigure figure)
        {
            return SetStatus.READ_ONLY;
        }

        public int Width => this.mainField.Width;

        public int Height => this.mainField.Height;

        public IField copyField()
        {
            return this.mainField.copyField();
        }

        public bool IsInBounds(Point point)
        {
            return this.mainField.IsInBounds(point);
        }
    }
}