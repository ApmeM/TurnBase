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

    public bool trySet(Point to, IFigure figure)
    {
        // It is readonly. Nothing will be changed on the mainField
        throw new Exception("Field is read-only");
    }

    public int getWidth()
    {
        return this.mainField.getWidth();
    }

    public int getHeight()
    {
        return this.mainField.getHeight();
    }

    public IField copyField()
    {
        return this.mainField.copyField();
    }
}
