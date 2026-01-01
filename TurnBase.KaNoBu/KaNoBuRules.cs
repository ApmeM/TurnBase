using System;
using System.Collections.Generic;
using TurnBase;

namespace TurnBase.KaNoBu
{
    public class KaNoBuRules : IGameRules<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private readonly Dictionary<int, IField> fieldsCache;
        private readonly int size;

        public KaNoBuRules(int size)
        {
            if (size < 6)
            {
                throw new Exception("Min size is 6.");
            }

            this.fieldsCache = new Dictionary<int, IField>();
            this.size = size;
        }

        public IField generateGameField()
        {
            return new Field2D(this.size, this.size);
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

            var availableShips = new List<IFigure>();
            var fieldSize = initFieldWidth * initFieldHeight;
            availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipFlag));
            for (int i = 1; i < fieldSize; i++)
            {
                var shipN = i % 3;
                if (shipN == 0) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipStone));
                if (shipN == 1) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipScissors));
                if (shipN == 2) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipPaper));
            }

            return new KaNoBuInitModel(initFieldWidth, initFieldHeight, availableShips);
        }

        public bool TryApplyInitResponse(IField mainField, int playerNumber, KaNoBuInitResponseModel initResponse)
        {
            var preparedField = initResponse.PreparedField;

            var ships = this.GetInitModel(playerNumber).AvailableFigures;
            var availableShips = new Dictionary<KaNoBuFigure.FigureTypes, int>();
            foreach (KaNoBuFigure s in ships)
            {
                var count = 0;
                var shipType = s.FigureType;
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
                    var ship = (KaNoBuFigure)preparedField.get(new Point { X = i, Y = j });
                    if (ship == null)
                    {
                        continue;
                    }

                    var shipType = ship.FigureType;
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

            var playerWidth = initResponse.PreparedField.Width;
            var playerHeight = initResponse.PreparedField.Height;

            if (initFieldWidth != playerWidth || initFieldHeight != playerHeight)
            {
                return false;
            }

            for (var i = 0; i < playerWidth; i++)
            {
                for (var j = 0; j < playerHeight; j++)
                {
                    var playerShip = initResponse.PreparedField.get(new Point { X = i, Y = j });
                    Point position;
                    if (playerNumber == 0)
                    {
                        position = new Point { X = i + initFieldHeight, Y = j };
                    }
                    else if(playerNumber == 1)
                    {
                        position = new Point { X = i + initFieldHeight, Y = playerNumber == 0 ? j : mainHeight - playerHeight + j };
                    }
                    else if(playerNumber == 2)
                    {
                        position = new Point { X = j, Y = i + initFieldHeight };
                    }
                    else if(playerNumber == 3)
                    {
                        position = new Point { X = playerNumber == 0 ? j : mainWidth - playerHeight + j, Y = i + initFieldHeight };
                    }
                    else
                    {
                        throw new Exception("Unsupported number of players.");
                    }

                    mainField.trySet(position, null);
                    mainField.trySet(position, playerShip);
                }
            }

            return true;
        }

        public KaNoBuMoveModel GetMoveModel(IField mainField, int playerNumber)
        {
            if (!this.fieldsCache.ContainsKey(playerNumber))
            {
                var concealer = new FieldConcealer(mainField, playerNumber);
                var readonlyField = new FieldReadOnly(concealer);

                this.fieldsCache[playerNumber] = readonlyField;
            }

            return new KaNoBuMoveModel(this.fieldsCache[playerNumber]);
        }

        public KaNoBuMoveResponseModel AutoMove(IField mainField, int playerNumber)
        {
            var mainWidth = mainField.Width;
            var mainHeight = mainField.Height;
            var canMove = false;
            var flagFound = false;
            for (var i = 0; i < mainWidth; i++)
            {
                for (var j = 0; j < mainHeight; j++)
                {
                    var playerShip = (KaNoBuFigure)mainField.get(new Point { X = i, Y = j });
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
                return new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN);
            }
        }

        public MoveValidationStatus CheckMove(IField mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
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
                return MoveValidationStatus.ERROR_INVALID_MOVE;
            }

            if (from.PlayerId != playerNumber)
            {
                return MoveValidationStatus.ERROR_INVALID_MOVE;
            }

            if (!from.IsMoveValid(playerMove))
            {
                return MoveValidationStatus.ERROR_INVALID_MOVE;
            }

            if (to != null && to.PlayerId == from.PlayerId)
            {
                return MoveValidationStatus.ERROR_FIELD_OCCUPIED;
            }

            return MoveValidationStatus.OK;
        }

        public KaNoBuMoveNotificationModel MakeMove(IField mainField, int playerNumber, KaNoBuMoveResponseModel playerMove)
        {
            var from = (KaNoBuFigure)mainField.get(playerMove.From);
            var to = (KaNoBuFigure)mainField.get(playerMove.To);

            if (from == null)
            {
                throw new Exception("Move from empty field position");
            }

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
            }

            return new KaNoBuMoveNotificationModel(playerMove, from, to, winner);
        }

        public List<int> findWinners(IField mainField)
        {
            var winners = new List<int>();
            int mainWidth = mainField.Width;
            int mainHeight = mainField.Height;
            for (int i = 0; i < mainWidth; i++)
            {
                for (int j = 0; j < mainHeight; j++)
                {
                    var playerShip = (KaNoBuFigure)mainField.get(new Point { X = i, Y = j });
                    if (playerShip == null)
                    {
                        continue;
                    }

                    if (playerShip.FigureType != KaNoBuFigure.FigureTypes.ShipFlag)
                    {
                        continue;
                    }

                    winners.Add(playerShip.PlayerId);
                }
            }

            if (winners.Count == 1)
            {
                return winners;
            }
            else
            {
                return null;
            }
        }

        private KaNoBuFigure battle(KaNoBuFigure attacker, KaNoBuFigure defender)
        {
            if (defender.FigureType == attacker.FigureType)
            {
                return null;
            }
            else if ((defender.FigureType == KaNoBuFigure.FigureTypes.ShipStone && attacker.FigureType == KaNoBuFigure.FigureTypes.ShipScissors) ||
                    (defender.FigureType == KaNoBuFigure.FigureTypes.ShipScissors && attacker.FigureType == KaNoBuFigure.FigureTypes.ShipPaper) ||
                    (defender.FigureType == KaNoBuFigure.FigureTypes.ShipPaper && attacker.FigureType == KaNoBuFigure.FigureTypes.ShipStone))
            {
                return defender;
            }
            else
            {
                return attacker;
            }
        }

        public void TurnCompleted(IField mainField)
        {
            // Nothing to do here.
        }

        public void PlayerDisconnected(IField mainField, int playerNumber)
        {
            int mainWidth = mainField.Width;
            int mainHeight = mainField.Height;
            for (int i = 0; i < mainWidth; i++)
            {
                for (int j = 0; j < mainHeight; j++)
                {
                    var point = new Point { X = i, Y = j };
                    var playerShip = (KaNoBuFigure)mainField.get(point);
                    if (playerShip == null)
                    {
                        continue;
                    }

                    // If the player is disconnected, remove their flag from the field => they lose.
                    if (playerShip.FigureType == KaNoBuFigure.FigureTypes.ShipFlag && playerShip.PlayerId == playerNumber)
                    {
                        mainField.trySet(point, null);
                    }
                }
            }
        }
    }
}