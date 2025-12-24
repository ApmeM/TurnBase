// namespace TurnBase.Core;

// public class FieldRotator : IField
// {
//     private IField mainField;
//     private int playerNumber;
//     private IPointRotator pointRotator;

//     public int Width => this.mainField.Width;
//     public int Height => this.mainField.Height;

//     public FieldRotator(IField mainField, int playerNumber, IPointRotator pointRotator)
//     {
//         this.mainField = mainField;
//         this.playerNumber = playerNumber;
//         this.pointRotator = pointRotator;
//     }

//     public IFigure? get(Point from)
//     {
//         Point point = pointRotator.RotatePoint(this.mainField, from, this.playerNumber);
//         return this.mainField.get(point);
//     }

//     public IField.SetStatus trySet(Point to, IFigure? figure)
//     {
//         Point point = pointRotator.RotatePoint(this.mainField, to, this.playerNumber);
//         return this.mainField.trySet(point, figure);
//     }

//     public IField copyField()
//     {
//         return new FieldRotator(this.mainField.copyField(), this.playerNumber, this.pointRotator);
//     }
// }
