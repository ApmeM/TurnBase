using System;
using System.Collections.Generic;
using System.Linq;

namespace TurnBase.KaNoBu
{
    public class KaNoBuRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private readonly int size;

        public bool AllFiguresVisible;
        public bool WithDocks;
        public int MaxMovesPerTurn = int.MaxValue;

        public KaNoBuRules(int size)
        {
            if (size < 6)
            {
                throw new Exception("Min size is 6.");
            }

            this.size = size;
        }

        public IField generateGameField()
        {
            var field = Field2D.Create(this.size, this.size);
            if (this.WithDocks)
            {
                var blockSize = size / 3;
                for (var i = 0; i < blockSize; i++)
                {
                    for (var j = 0; j < blockSize; j++)
                    {
                        field.walls[i, j] = true;
                        field.walls[i, size - 1 - j] = true;
                        field.walls[size - 1 - i, j] = true;
                        field.walls[size - 1 - i, size - 1 - j] = true;
                    }
                }
            }
            return field;
        }

        public int getMaxPlayersCount()
        {
            return 4;
        }

        public int getMinPlayersCount()
        {
            return 2;
        }

        public IPlayerRotator GetInitRotator()
        {
            return new PlayerRotatorNormal();
            // return new PlayerRotatorAllAtOnce(); // ToDo: fix it.
        }

        public IPlayerRotator GetMoveRotator()
        {
            return new PlayerRotatorNormal();
        }

        public KaNoBuInitModel GetInitModel(int playerNumber)
        {
            var initFieldHeight = size / 3;
            var initFieldWidth = size - 2 * initFieldHeight;

            var availableShips = new List<KaNoBuFigure.FigureTypes>();
            var fieldSize = initFieldWidth * initFieldHeight;
            availableShips.Add(KaNoBuFigure.FigureTypes.ShipFlag);
            availableShips.Add(KaNoBuFigure.FigureTypes.ShipMine);
            for (int i = 2; i < fieldSize; i++)
            {
                var shipN = i % 3;
                if (shipN == 0) availableShips.Add(KaNoBuFigure.FigureTypes.ShipStone);
                if (shipN == 1) availableShips.Add(KaNoBuFigure.FigureTypes.ShipScissors);
                if (shipN == 2) availableShips.Add(KaNoBuFigure.FigureTypes.ShipPaper);
            }

            return new KaNoBuInitModel(initFieldWidth, initFieldHeight, availableShips, this.MaxMovesPerTurn);
        }

        public bool TryApplyInitResponse(IField field, int playerNumber, KaNoBuInitResponseModel initResponse)
        {
            var preparedField = (Field2D)initResponse.Field;
            var mainField = (Field2D)field;

            var ships = this.GetInitModel(playerNumber).AvailableFigures;
            var availableShips = new Dictionary<KaNoBuFigure.FigureTypes, int>();
            foreach (KaNoBuFigure.FigureTypes shipType in ships)
            {
                var count = 0;
                if (availableShips.ContainsKey(shipType))
                {
                    count = availableShips[shipType];
                }
                count++;

                availableShips[shipType] = count;
            }

            for (var i = 0; i < preparedField.Width; i++)
            {
                for (var j = 0; j < preparedField.Height; j++)
                {
                    var shipType = (preparedField[i, j] as KaNoBuFigure).FigureType;
                    if (!availableShips.ContainsKey(shipType))
                    {
                        return false;
                    }
                    var count = availableShips[shipType];
                    count--;
                    availableShips[shipType] = count;
                    if (count == 0)
                    {
                        availableShips.Remove(shipType);
                    }
                }
            }

            if (availableShips.Count != 0)
            {
                return false;
            }


            var initFieldHeight = size / 3;
            var initFieldWidth = size - 2 * initFieldHeight;

            var mainHeight = mainField.Height;
            var mainWidth = mainField.Width;

            var playerWidth = preparedField.Width;
            var playerHeight = preparedField.Height;

            if (initFieldWidth != playerWidth || initFieldHeight != playerHeight)
            {
                return false;
            }

            for (var i = 0; i < playerWidth; i++)
            {
                for (var j = 0; j < playerHeight; j++)
                {
                    var p = new Point(i, j);
                    var playerShip = (preparedField[i, j] as KaNoBuFigure).FigureType;
                    Point position;
                    if (playerNumber == 0)
                    {
                        position = new Point { X = i + initFieldHeight, Y = j };
                    }
                    else if (playerNumber == 1)
                    {
                        position = new Point { X = i + initFieldHeight, Y = playerNumber == 0 ? j : mainHeight - playerHeight + j };
                    }
                    else if (playerNumber == 2)
                    {
                        position = new Point { X = j, Y = i + initFieldHeight };
                    }
                    else if (playerNumber == 3)
                    {
                        position = new Point { X = playerNumber == 0 ? j : mainWidth - playerHeight + j, Y = i + initFieldHeight };
                    }
                    else
                    {
                        throw new Exception("Unsupported number of players.");
                    }

                    mainField[position] = null;
                    mainField[position] = KaNoBuFigure.Create(playerNumber, playerShip, this.AllFiguresVisible, 0);
                }
            }

            return true;
        }

        public KaNoBuMoveModel GetMoveModel(IField mainField, int playerNumber)
        {
            return new KaNoBuMoveModel(mainField.copyForPlayer(playerNumber));
        }

        public KaNoBuMoveResponseModel AutoMove(IField field, int playerNumber)
        {
            var mainField = (Field2D)field;

            var mainWidth = mainField.Width;
            var mainHeight = mainField.Height;
            var canMove = false;
            var flagFound = false;
            for (var i = 0; i < mainWidth; i++)
            {
                for (var j = 0; j < mainHeight; j++)
                {
                    var playerShip = (KaNoBuFigure)mainField[i, j];
                    canMove = canMove || (
                        playerShip != null && 
                        playerShip.PlayerId == playerNumber && 
                        playerShip.IsMoveable
                        );

                    flagFound = flagFound || (
                        playerShip != null && 
                        playerShip.PlayerId == playerNumber && 
                        playerShip.FigureType == KaNoBuFigure.FigureTypes.ShipFlag
                        );
                }
            }

            if (canMove && flagFound)
            {
                return null;
            }
            else
            {
                return new KaNoBuMoveResponseModel(null);
            }
        }

        public bool IsMoveValid(IField field, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
            if (playerMove.Moves.Count > this.MaxMovesPerTurn)
            {
                return false;
            }

            // All moves are valid now. If some ship can not move to its destination it will be skipped.
            return true;
        }

        private bool IsMoveValid(IField field, int playerNumber, KaNoBuMoveResponseModel.MoveStep playerMove)
        {
            var mainField = (Field2D)field;

            if (
                !mainField.IsInBounds(playerMove.From) || 
                !mainField.IsInBounds(playerMove.To) || 
                mainField.walls[playerMove.To.X, playerMove.To.Y])
            {
                return false;
            }

            var from = (KaNoBuFigure)mainField[playerMove.From];
            var to = (KaNoBuFigure)mainField[playerMove.To];

            return 
                from != null && 
                from.PlayerId == playerNumber && 
                from.IsMoveValid(playerMove) && 
                (to == null || to.PlayerId != from.PlayerId);
        }

        public KaNoBuMoveNotificationModel MakeMove(IField field, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
            var mainField = (Field2D)field;
            var notifications = new List<KaNoBuMoveNotificationModel.MoveNotification>();

            if (playerMove.Moves.Count == 0)
            {
                return new KaNoBuMoveNotificationModel(notifications);
            }

            foreach (var moveStep in playerMove.Moves)
            {
                if (!IsMoveValid(mainField, playerNumber, moveStep))
                {
                    continue;
                }
                var stepResult = this.MakeMoveStep(mainField, playerNumber, moveStep);
                notifications.Add(stepResult);
            }

            return new KaNoBuMoveNotificationModel(notifications);
        }

        public KaNoBuMoveNotificationModel GetMoveNotificationForPlayer(
            KaNoBuMoveNotificationModel notification,
            int playerNumber)
        {
            var notifications = notification.MoveNotifications.Select(move =>
            {
                if (!move.Battle.HasValue)
                {
                    return move;
                }

                var battle = move.Battle.Value;
                if (battle.attackerPlayerNumber != playerNumber && battle.defenderPlayerNumber != playerNumber)
                {
                    battle.attackerFigureType = KaNoBuFigure.FigureTypes.Unknown;
                    battle.defenderFigureType = KaNoBuFigure.FigureTypes.Unknown;
                }

                return new KaNoBuMoveNotificationModel.MoveNotification(move.From, move.To, battle);
            }).ToList();

            return new KaNoBuMoveNotificationModel(notifications);
        }

        private KaNoBuMoveNotificationModel.MoveNotification MakeMoveStep(IField field, int playerNumber, KaNoBuMoveResponseModel.MoveStep playerMove)
        {
            var mainField = (Field2D)field;
            var from = (KaNoBuFigure)mainField[playerMove.From];
            var to = (KaNoBuFigure)mainField[playerMove.To];

            if (to == null)
            {
                mainField[playerMove.To] = from;
                mainField[playerMove.From] = null;
                return new KaNoBuMoveNotificationModel.MoveNotification(playerMove.From, playerMove.To);
            }

            var resolution = this.battle(from, to);
            var winner = resolution.Winner;

            if (resolution.Outcome == KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)
            {
                mainField[playerMove.From] = null;
                mainField[playerMove.To] = null;
            }
            else if (winner != null)
            {
                mainField[playerMove.From] = null;
                mainField[playerMove.To] = winner;
                winner.WinNumber++;
                if (winner.WinNumber % 3 == 0)
                {
                    winner = winner.WithFigureType(KaNoBuFigure.FigureTypes.ShipUniversal);
                    mainField[playerMove.To] = winner;
                }

                if (to.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                {
                    // Change all figures of this player to the player that captures the flag.
                    for (int i = 0; i < mainField.Width; i++)
                    {
                        for (int j = 0; j < mainField.Height; j++)
                        {
                            var playerShip = (KaNoBuFigure)mainField[i, j];
                            if (playerShip == null)
                            {
                                continue;
                            }

                            if (playerShip.PlayerId == to.PlayerId)
                            {
                                playerShip.PlayerId = from.PlayerId;
                            }
                        }
                    }
                }
            }

            var battle = new KaNoBuMoveNotificationModel.Battle
            {
                battleResult = resolution.Outcome,
                isDefenderFlag = to.FigureType == KaNoBuFigure.FigureTypes.ShipFlag,
                attackerPlayerNumber = from.PlayerId,
                defenderPlayerNumber = to.PlayerId,
                attackerFigureType = from.FigureType,
                defenderFigureType = to.FigureType
            };

            return new KaNoBuMoveNotificationModel.MoveNotification(playerMove.From, playerMove.To, battle);
        }

        public List<int> findWinners(IField mainField)
        {
            var winners = new List<int>();
            for (var i = 0; i < getMaxPlayersCount(); i++)
            {
                var automove = this.AutoMove(mainField, i);
                if (automove == null)
                {
                    winners.Add(i);
                }
            }

            if (winners.Count > 1)
            {
                // 1+ winners means game not finished. 
                return null;
            }
            else
            {
                // 1 winner means someone won.
                // 0 winners means draw.
                return winners;
            }
        }

        private BattleResolution battle(KaNoBuFigure attacker, KaNoBuFigure defender)
        {
            return attacker.ResolveBattle(defender);
        }

        public void TurnCompleted(IField mainField)
        {
            // Nothing to do here.
        }

        public void PlayerDisconnected(IField field, int playerNumber)
        {
        }
    }
}