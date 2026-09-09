using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnBase.KaNoBu
{

    public class KaNoBuMoveNotificationModel
    {
        public class MoveNotification
        {
            public MoveNotification(Point from, Point to, Battle? battle = null)
            {
                From = from;
                To = to;
                Battle = battle;
            }

            public readonly Point From;
            public readonly Point To;
            public readonly Battle? Battle;
        }

        public struct Battle
        {
            public BattleResult battleResult;
            public bool isDefenderFlag;
            public int attackerPlayerNumber;
            public int defenderPlayerNumber;
            public KaNoBuFigure.FigureTypes attackerFigureType;
            public KaNoBuFigure.FigureTypes defenderFigureType;
        }

        public enum BattleResult
        {
            Draw,
            AttackerWon,
            DefenderWon,
            BothDestroyed
        }

        public KaNoBuMoveNotificationModel(List<MoveNotification> moveNotifications)
        {
            this.MoveNotifications = moveNotifications ?? new List<MoveNotification>();
        }

        public readonly List<MoveNotification> MoveNotifications;

        public override string ToString()
        {
            if (this.MoveNotifications.Count == 0)
            {
                return "Player skip turn.";
            }
            StringBuilder result = new StringBuilder();

            foreach (var move in this.MoveNotifications)
            {
                result.AppendLine($"Player move from {move.From} to {move.To}.");
                if (move.Battle.HasValue)
                {
                    result.AppendLine($"Battle result: {move.Battle.Value.battleResult} (IsFlag = {move.Battle.Value.isDefenderFlag})");
                }
            }

            return result.ToString();
        }
    }
}