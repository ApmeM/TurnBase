using System;
using System.Collections.Generic;

namespace TurnBase.KaNoBu
{
    public class KaNoBuRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        public static readonly Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes> Winner = new Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes>
        {
            {KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors},
            {KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone},
            {KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper},
        };

        public static readonly Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes> Looser = new Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes>
        {
            {KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipStone},
            {KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipPaper},
            {KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipScissors},
        };

        private readonly int size;

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
            return Field2D.Create(this.size, this.size);
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
            for (int i = 1; i < fieldSize; i++)
            {
                var shipN = i % 3;
                if (shipN == 0) availableShips.Add(KaNoBuFigure.FigureTypes.ShipStone);
                if (shipN == 1) availableShips.Add(KaNoBuFigure.FigureTypes.ShipScissors);
                if (shipN == 2) availableShips.Add(KaNoBuFigure.FigureTypes.ShipPaper);
            }

            return new KaNoBuInitModel(initFieldWidth, initFieldHeight, availableShips);
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
                    var p = new Point(i, j);
                    var shipType = (preparedField.get(p) as KaNoBuFigure).FigureType;
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
                    var playerShip = (preparedField.get(p) as KaNoBuFigure).FigureType;
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

                    mainField.trySet(position, null);
                    mainField.trySet(position, new KaNoBuFigure(playerNumber, playerShip));
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
                    var p = new Point(i, j);
                    var playerShip = (KaNoBuFigure)mainField.get(p);
                    if (playerShip == null)
                    {
                        continue;
                    }

                    if (playerShip.PlayerId != playerNumber)
                    {
                        continue;
                    }

                    if (playerShip.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                    {
                        flagFound = true;
                        continue;
                    }

                    canMove = true;
                }
            }

            if (canMove && flagFound)
            {
                return null;
            }
            else
            {
                return new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN, default, default);
            }
        }

        public MoveValidationStatus CheckMove(IField field, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
            var mainField = (Field2D)field;

            if (playerMove.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
            {
                return MoveValidationStatus.OK;
            }

            if (!mainField.IsInBounds(playerMove.From) || !mainField.IsInBounds(playerMove.To))
            {
                return MoveValidationStatus.ERROR_OUTSIDE_FIELD;
            }

            var from = (KaNoBuFigure)mainField.get(playerMove.From);
            var to = (KaNoBuFigure)mainField.get(playerMove.To);

            if (from == null)
            {
                return MoveValidationStatus.ERROR_MOVE_FROM_NOWHERE;
            }

            if (from.PlayerId != playerNumber)
            {
                return MoveValidationStatus.ERROR_INVALID_PLAYER;
            }

            if (!from.IsMoveValid(playerMove))
            {
                return MoveValidationStatus.ERROR_INVALID_FIGURE_MOVE;
            }

            if (to != null && to.PlayerId == from.PlayerId)
            {
                return MoveValidationStatus.ERROR_FIELD_OCCUPIED;
            }

            return MoveValidationStatus.OK;
        }

        public KaNoBuMoveNotificationModel MakeMove(IField field, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
            var mainField = (Field2D)field;
            if (playerMove.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
            {
                return new KaNoBuMoveNotificationModel(playerMove);
            }

            var from = (KaNoBuFigure)mainField.get(playerMove.From);
            var to = (KaNoBuFigure)mainField.get(playerMove.To);

            if (to == null)
            {
                mainField.trySet(playerMove.To, from);
                mainField.trySet(playerMove.From, null);
                return new KaNoBuMoveNotificationModel(playerMove);
            }

            var winner = this.battle(from, to);

            if (winner != null)
            {
                mainField.trySet(playerMove.From, null);
                mainField.trySet(playerMove.To, null);
                mainField.trySet(playerMove.To, winner);
                winner.WinNumber++;
                if(winner.WinNumber % 3 == 0)
                {
                    winner.FigureType = KaNoBuFigure.FigureTypes.ShipUniversal;
                }
            }

            if (to.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
            {
                // Change all figures of this player to the player that captures the flag.
                for (int i = 0; i < mainField.Width; i++)
                {
                    for (int j = 0; j < mainField.Height; j++)
                    {
                        var p = new Point(i, j);
                        var playerShip = (KaNoBuFigure)mainField.get(p);
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

            return new KaNoBuMoveNotificationModel(playerMove, new KaNoBuMoveNotificationModel.Battle
            {
                battleResult =
                    winner == null ? KaNoBuMoveNotificationModel.BattleResult.Draw :
                    winner == from ? KaNoBuMoveNotificationModel.BattleResult.AttackerWon :
                    winner == to ? KaNoBuMoveNotificationModel.BattleResult.DefenderWon :
                    throw new Exception("Invalid battle calculation."),
                isDefenderFlag = to.FigureType == KaNoBuFigure.FigureTypes.ShipFlag
            }
            );
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

        private KaNoBuFigure battle(KaNoBuFigure attacker, KaNoBuFigure defender)
        {
            if (defender.FigureType == attacker.FigureType)
            {
                return null;
            }
            else if(defender.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
            {
                return attacker;
            }
            else if(defender.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
            {
                defender.FigureType = Winner[attacker.FigureType];
                return defender;
            }
            else if(attacker.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
            {
                attacker.FigureType = Winner[defender.FigureType];
                return attacker;
            }
            else if (attacker.FigureType == Winner[defender.FigureType])
            {
                return attacker;
            }
            else if (defender.FigureType == Winner[attacker.FigureType])
            {
                return defender;
            }
            else
            {
                return null;
            }
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