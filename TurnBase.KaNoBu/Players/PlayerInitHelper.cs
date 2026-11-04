using System;

namespace TurnBase.KaNoBu
{
    public static class PlayerInitHelper
    {
        private static Random random = new Random();

        public static Field2D CreateRandomField(int playerId, KaNoBuInitModel model)
        {
            var preparedField = Field2D.Create(model.Width, model.Height);
            for (var x = 0; x < model.Width; x++)
            {
                for (var y = 0; y < model.Height; y++)
                {
                    var figureIndex = random.Next(model.AvailableFigures.Count);
                    var figureType = model.AvailableFigures[figureIndex];
                    model.AvailableFigures.RemoveAt(figureIndex);
                    preparedField.SetFigure(x, y, KaNoBuFigure.Create(playerId, figureType, 0));
                }
            }

            return preparedField;
        }

        public static Field2D CreateOrderedField(int playerId, KaNoBuInitModel model)
        {
            var preparedField = Field2D.Create(model.Width, model.Height);
            for (var i = 0; i < model.Width; i++)
            {
                for (var j = 0; j < model.Height; j++)
                {
                    var p = new Point { X = i, Y = j };
                    var ship = model.AvailableFigures[0];
                    preparedField.SetFigure(p, KaNoBuFigure.Create(playerId, ship, 0));
                    model.AvailableFigures.Remove(ship);
                }
            }

            return preparedField;
        }
    }
}