using System;

namespace TurnBase.KaNoBu
{
    public class KaNoBuFieldMemorization
    {
        public KaNoBuMoveModel.FigureModel?[,] Field;

        public void SynchronizeField(MakeTurnModel<KaNoBuMoveModel> model)
        {
            if (Field == null)
            {
                Field = model.Request.Field;
            }
            else
            {
                for (var x = 0; x < model.Request.Field.GetLength(0); x++)
                {
                    for (var y = 0; y < model.Request.Field.GetLength(1); y++)
                    {
                        var ship = model.Request.Field[x, y];
                        if (ship != null)
                        {
                            if (Field[x, y] == null)
                            {
                                throw new Exception("Inconsistent field state");
                            }

                            Field[x, y] = new KaNoBuMoveModel.FigureModel
                            {
                                PlayerNumber = ship.Value.PlayerNumber,
                                FigureType = Field[x, y]?.FigureType == KaNoBuFigure.FigureTypes.Unknown ? ship.Value.FigureType : Field[x, y].Value.FigureType
                            };
                        }
                    }
                }
            }
        }

        public void UpdateKnownShips(KaNoBuMoveNotificationModel notification)
        {
            if (this.Field == null || notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
            {
                return;
            }

            var fromMapPos = notification.move.From;
            var toMapPos = notification.move.To;

            var movedUnit = this.Field[fromMapPos.X, fromMapPos.Y].Value;

            if (notification.battle.HasValue)
            {
                var defenderUnit = this.Field[toMapPos.X, toMapPos.Y].Value;

                switch (notification.battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = movedUnit.FigureType;
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = defenderUnit.FigureType;
                        this.Field[fromMapPos.X, fromMapPos.Y] = movedUnit;
                        this.Field[toMapPos.X, toMapPos.Y] = defenderUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                        // Attacker won

                        if (movedUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            movedUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }
                        if (notification.battle.Value.isDefenderFlag)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.ShipFlag;
                        }
                        else
                        {
                            if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Looser[movedUnit.FigureType];
                            if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Winner[defenderUnit.FigureType];
                        }
                        this.Field[fromMapPos.X, fromMapPos.Y] = null;
                        this.Field[toMapPos.X, toMapPos.Y] = movedUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        // Defender won
                        if (defenderUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }

                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Winner[movedUnit.FigureType];
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Looser[defenderUnit.FigureType];

                        this.Field[fromMapPos.X, fromMapPos.Y] = null;
                        this.Field[toMapPos.X, toMapPos.Y] = defenderUnit;
                        break;
                }
            }
            else
            {
                // No battle - swim here.
                this.Field[fromMapPos.X, fromMapPos.Y] = null;
                this.Field[toMapPos.X, toMapPos.Y] = movedUnit;
            }
        }
    }
}