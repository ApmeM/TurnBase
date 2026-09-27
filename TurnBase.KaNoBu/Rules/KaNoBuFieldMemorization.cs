using System;
using System.Linq;

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
                Field = (Field2D)new SimpleCopier().CopyForPlayer(model, -1);
            }
            else
            {
                for (var x = 0; x < model.Width; x++)
                {
                    for (var y = 0; y < model.Height; y++)
                    {
                        var requestShip = model.GetFigure(x, y) as KaNoBuFigure;
                        var memorizedShip = Field.GetFigure(x, y) as KaNoBuFigure;

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

                        Field.SetFigure(x, y, memorizedShip);
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

                var movedUnit = this.Field.GetFigure(fromMapPos) as KaNoBuFigure;
                var defenderUnit = this.Field.GetFigure(toMapPos) as KaNoBuFigure;

                if (!notification.Battle.HasValue)
                {
                    this.Field.SetFigure(fromMapPos, null);
                    this.Field.SetFigure(toMapPos, movedUnit);
                    continue;
                }

                var battle = notification.Battle.Value;
                if (battle.attackerFigureType != KaNoBuFigure.FigureTypes.Unknown)
                {
                    movedUnit = movedUnit.WithFigureType(battle.attackerFigureType);
                }
                if (battle.defenderFigureType != KaNoBuFigure.FigureTypes.Unknown)
                {
                    defenderUnit = defenderUnit.WithFigureType(battle.defenderFigureType);
                }

                switch (battle.battleResult)
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
                        this.Field.SetFigure(fromMapPos, movedUnit);
                        this.Field.SetFigure(toMapPos, defenderUnit);
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.BothDestroyed:
                        this.Field.SetFigure(fromMapPos, null);
                        this.Field.SetFigure(toMapPos, null);
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
                        this.Field.SetFigure(fromMapPos, null);
                        this.Field.SetFigure(toMapPos, movedUnit);
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

                        this.Field.SetFigure(fromMapPos, null);
                        this.Field.SetFigure(toMapPos, defenderUnit);
                        break;
                }
            }
        }

        private static readonly KaNoBuFigure.FigureTypes[] BattleShipTypes =
        {
            KaNoBuFigure.FigureTypes.ShipPaper,
            KaNoBuFigure.FigureTypes.ShipScissors,
            KaNoBuFigure.FigureTypes.ShipStone,
            KaNoBuFigure.FigureTypes.ShipScout,
        };

        private static KaNoBuFigure FindFigure(KaNoBuFigure template, Func<KaNoBuFigure, bool> predicate)
        {
            var applicableTypes = BattleShipTypes
                .Select(figureType => template.WithFigureType(figureType))
                .Where(a => predicate(a))
                .ToList();

            if(applicableTypes.Count == 1)
            {
                return applicableTypes[0];
            }

            return template;
        }
    }
}