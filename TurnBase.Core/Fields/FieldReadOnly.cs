namespace TurnBase.Core;

public class FieldReadOnly : IField
{
    private IField mainField;

    public FieldReadOnly(IField mainField)
    {
        this.mainField = mainField;
    }

    public IFigure? get(Point from)
    {
        return this.mainField.get(from);
    }

    public IField.SetStatus trySet(Point to, IFigure? figure)
    {
        return IField.SetStatus.READ_ONLY;
    }

    public int Width => this.mainField.Width;

    public int Height => this.mainField.Height;

    public IField copyField()
    {
        return this.mainField.copyField();
    }
}
