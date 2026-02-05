using System;

namespace TurnBase.KaNoBu
{
    public class KaNoBuFieldMemorization
    {
        public Field2D Field;

        public void Clear()
        {
            Field = null;
        }

        public void SynchronizeField(Field2D model)
        {
            if (Field == null)
            {
                Field = (Field2D)model.copyForPlayer(-1);
            }
            else
            {
                for (var x = 0; x < model.Width; x++)
                {
                    for (var y = 0; y < model.Height; y++)
                    {
                        var requestShip = model[x, y] as KaNoBuFigure;
                        var memorizedShip = Field[x, y] as KaNoBuFigure;

                        if (requestShip != null && memorizedShip == null || memorizedShip != null && requestShip == null)
                        {
                            throw new Exception("Inconsistent field state");
                        }

                        if (requestShip == null && memorizedShip == null)
                        {
                            continue;
                        }

                        memorizedShip.PlayerId = requestShip.PlayerId;
                        memorizedShip.FigureType = requestShip.FigureType != KaNoBuFigure.FigureTypes.Unknown ? requestShip.FigureType : memorizedShip.FigureType;
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

            var movedUnit = this.Field[fromMapPos] as KaNoBuFigure;
            var defenderUnit = this.Field[toMapPos] as KaNoBuFigure;

            this.Field[fromMapPos] = null;
            this.Field[toMapPos] = null;

            if (notification.battle.HasValue)
            {
                switch (notification.battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = movedUnit.FigureType;
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = defenderUnit.FigureType;
                        this.Field[fromMapPos] = movedUnit;
                        this.Field[toMapPos] = defenderUnit;
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
                        this.Field[toMapPos] = movedUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        // Defender won
                        if (defenderUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }

                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Winner[movedUnit.FigureType];
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Looser[defenderUnit.FigureType];

                        this.Field[toMapPos] = defenderUnit;
                        break;
                }
            }
            else
            {
                // No battle - swim here.
                this.Field[toMapPos] = movedUnit;
            }
        }
    }
}