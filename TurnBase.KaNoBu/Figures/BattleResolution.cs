namespace TurnBase.KaNoBu
{
    public sealed class BattleResolution
    {
        private BattleResolution(KaNoBuFigure winner, KaNoBuMoveNotificationModel.BattleResult outcome)
        {
            Winner = winner;
            Outcome = outcome;
        }

        public KaNoBuFigure Winner { get; }

        public KaNoBuMoveNotificationModel.BattleResult Outcome { get; }

        public static BattleResolution Draw()
        {
            return new BattleResolution(null, KaNoBuMoveNotificationModel.BattleResult.Draw);
        }

        public static BattleResolution Scouted()
        {
            return new BattleResolution(null, KaNoBuMoveNotificationModel.BattleResult.Scouted);
        }

        public static BattleResolution AttackerWon(KaNoBuFigure winner)
        {
            return new BattleResolution(winner, KaNoBuMoveNotificationModel.BattleResult.AttackerWon);
        }

        public static BattleResolution DefenderWon(KaNoBuFigure winner)
        {
            return new BattleResolution(winner, KaNoBuMoveNotificationModel.BattleResult.DefenderWon);
        }

        public static BattleResolution BothAreDestroyed()
        {
            return new BattleResolution(null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed);
        }
    }
}