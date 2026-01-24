using System;

namespace TurnBase.KaNoBu
{
    public class KaNoBuFieldMemorization
    {
        public IField Field;

        public void Clear()
        {
            Field = null;
        }

        public void SynchronizeField(IField model)
        {
            if (Field == null)
            {
                Field = model.copyForPlayer(-1);
            }
            else
            {
                for (var x = 0; x < model.Width; x++)
                {
                    for (var y = 0; y < model.Height; y++)
                    {
                        var requestShip = model.get(x, y) as KaNoBuFigure;
                        var memorizedShip = Field.get(x, y) as KaNoBuFigure;

                        if (requestShip != null && memorizedShip == null || memorizedShip != null && requestShip == null)
                        {
                            throw new Exception("Inconsistent field state");
                        }

                        if (requestShip == null && memorizedShip == null)
                        {
                            continue;
                        }

                        memorizedShip.PlayerId = requestShip.PlayerId;
                        memorizedShip.FigureType = memorizedShip.FigureType == KaNoBuFigure.FigureTypes.Unknown ? requestShip.FigureType : memorizedShip.FigureType;
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

            var movedUnit = this.Field.get(fromMapPos.X, fromMapPos.Y) as KaNoBuFigure;
            var defenderUnit = this.Field.get(toMapPos.X, toMapPos.Y) as KaNoBuFigure;

            this.Field.trySet(fromMapPos.X, fromMapPos.Y, null);
            this.Field.trySet(toMapPos.X, toMapPos.Y, null);

            if (notification.battle.HasValue)
            {
                switch (notification.battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = movedUnit.FigureType;
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = defenderUnit.FigureType;
                        this.Field.trySet(fromMapPos.X, fromMapPos.Y, movedUnit);
                        this.Field.trySet(toMapPos.X, toMapPos.Y, defenderUnit);
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
                        this.Field.trySet(toMapPos.X, toMapPos.Y, movedUnit);
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        // Defender won
                        if (defenderUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }

                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Winner[movedUnit.FigureType];
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Looser[defenderUnit.FigureType];

                        this.Field.trySet(toMapPos.X, toMapPos.Y, defenderUnit);
                        break;
                }
            }
            else
            {
                // No battle - swim here.
                this.Field.trySet(toMapPos.X, toMapPos.Y, movedUnit);
            }
        }
    }
}