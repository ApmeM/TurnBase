namespace TurnBase.Core;

public class FieldConcealer : IField {
    private IField mainField;
    private int playerNumber;

    public FieldConcealer(IField mainField, int playerNumber) {
        this.mainField = mainField;
        this.playerNumber = playerNumber;
    }

    public IFigure? get(Point from) {
        IFigure? figure = this.mainField.get(from);
        if (figure == null){
            return null;
        }

        if (figure.PlayerId != playerNumber){
            figure = new UnknownFigure(figure.PlayerId);
        }

        return figure;
    }

    public bool trySet(Point to, IFigure figure) {
        return this.mainField.trySet(to, figure);
    }

    public int getWidth() {
        return this.mainField.getWidth();
    }

    public int getHeight() {
        return this.mainField.getHeight();
    }

    public IField copyField() {
        return new FieldConcealer(this.mainField.copyField(), this.playerNumber);
    }
}