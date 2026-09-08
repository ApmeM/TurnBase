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
                        if (requestShip.FigureType != KaNoBuFigure.FigureTypes.Unknown)
                        {
                            memorizedShip = memorizedShip.WithFigureType(requestShip.FigureType);
                        }

                        Field[x, y] = memorizedShip;
                    }
                }
            }
        }

        public void UpdateKnownShips(KaNoBuMoveNotificationModel moveNotification)
        {
            if (this.Field == null || moveNotification.MoveNotifications.Count == 0)
            {
                return;
            }

            foreach (var notification in moveNotification.MoveNotifications)
            {
                var fromMapPos = notification.From;
                var toMapPos = notification.To;

                var movedUnit = this.Field[fromMapPos] as KaNoBuFigure;
                var defenderUnit = this.Field[toMapPos] as KaNoBuFigure;

                if (!notification.Battle.HasValue)
                {
                    this.Field[fromMapPos] = null;
                    this.Field[toMapPos] = movedUnit;
                    continue;
                }

                switch (notification.Battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown)
                        {
                            defenderUnit = defenderUnit.WithFigureType(movedUnit.FigureType);
                        }
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown)
                        {
                            movedUnit = movedUnit.WithFigureType(defenderUnit.FigureType);
                        }
                        this.Field[fromMapPos] = movedUnit;
                        this.Field[toMapPos] = defenderUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.BothDestroyed:
                        this.Field[fromMapPos] = null;
                        this.Field[toMapPos] = null;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                        if (movedUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            movedUnit = movedUnit.WithFigureType(KaNoBuFigure.FigureTypes.Unknown);
                        }
                        if (movedUnit.FigureType == KaNoBuFigure.FigureTypes.Unknown && defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown)
                        {
                            movedUnit = FindFigure(movedUnit, candidate => candidate.ResolveBattle(defenderUnit).Outcome == KaNoBuMoveNotificationModel.BattleResult.AttackerWon);
                        }
                        this.Field[fromMapPos] = null;
                        this.Field[toMapPos] = movedUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        if (defenderUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            defenderUnit = defenderUnit.WithFigureType(KaNoBuFigure.FigureTypes.Unknown);
                        }

                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown && defenderUnit.FigureType == KaNoBuFigure.FigureTypes.Unknown)
                        {
                            defenderUnit = FindFigure(defenderUnit, candidate => movedUnit.ResolveBattle(candidate).Outcome == KaNoBuMoveNotificationModel.BattleResult.DefenderWon);
                        }

                        this.Field[fromMapPos] = null;
                        this.Field[toMapPos] = defenderUnit;
                        break;
                }
            }
        }

        private static readonly KaNoBuFigure.FigureTypes[] BattleShipTypes =
        {
            KaNoBuFigure.FigureTypes.ShipPaper,
            KaNoBuFigure.FigureTypes.ShipScissors,
            KaNoBuFigure.FigureTypes.ShipStone,
        };

        private static KaNoBuFigure FindFigure(KaNoBuFigure template, Func<KaNoBuFigure, bool> predicate)
        {
            foreach (var figureType in BattleShipTypes)
            {
                var candidate = template.WithFigureType(figureType);
                if (predicate(candidate))
                {
                    return candidate;
                }
            }

            return template;
        }
    }
}