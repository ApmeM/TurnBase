using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class GameRulesKaNoBu : IGameRules
{
    private readonly int width;
    private readonly int height;

    private readonly Dictionary<int, IField> fieldsCache;
    private readonly IPointRotator pointRotator = new TwoPlayerPointRotator();

    public GameRulesKaNoBu(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.fieldsCache = new Dictionary<int, IField>();
    }

    public IField generateGameField()
    {
        return new Field2D(this.width, this.height);
    }

    public int getPlayersCount()
    {
        return 2;
    }

    public IPlayerRotator getRotator()
    {
        return new PlayerRotatorNormal
        {
            size = this.getPlayersCount()
        };
    }

    public PlayerInitialization getInitializationData(int playerNumber)
    {
        var newFieldWidth = width;
        var newFieldHeight = height / 3;
        var preparingField = new Field2D(newFieldWidth, newFieldHeight);

        var availableShips = new List<IFigure>();
        var fieldSize = newFieldWidth * newFieldHeight;
        availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipFlag));
        for (int i = 1; i < fieldSize; i++)
        {
            var shipN = i % 3;
            if (shipN == 0) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipStone));
            if (shipN == 1) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipScissors));
            if (shipN == 2) availableShips.Add(new KaNoBuFigure(playerNumber, KaNoBuFigure.FigureTypes.ShipPaper));
        }

        return new PlayerInitialization
        {
            preparingField = new FieldReadOnly(preparingField),
            availableFigures = availableShips
        };
    }

    public bool checkPreparedField(int playerNumber, IField preparedField)
    {
        var ships = this.getInitializationData(playerNumber).availableFigures;
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

        for (var i = 0; i < preparedField.getWidth(); i++)
        {
            for (var j = 0; j < preparedField.getHeight(); j++)
            {
                var ship = (KaNoBuFigure?)preparedField.get(new Point { X = i, Y = j });
                if (ship != null)
                {
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
        }

        return availableShips.Count() == 0;
    }

    public void addPlayerToField(IField mainField, IField playerField, int playerNumber)
    {
        var rotator = new FieldRotator(mainField, playerNumber, this.pointRotator);

        var mainHeight = mainField.getHeight();
        var mainWidth = mainField.getWidth();
        var playerWidth = playerField.getWidth();
        var playerHeight = playerField.getHeight();

        if (mainWidth != playerWidth)
        {
            throw new Exception("Player field width does not match main field width.");
        }

        for (var i = 0; i < playerWidth; i++)
        {
            for (var j = 0; j < playerHeight; j++)
            {
                var playerShip = playerField.get(new Point { X = i, Y = j });
                var position = new Point
                {
                    X = i,
                    Y = mainHeight - playerHeight + j
                };

                rotator.trySet(position, null);
                rotator.trySet(position, playerShip);
            }
        }
    }

    public IField getFieldForPlayer(IField mainField, int playerNumber)
    {
        if (!this.fieldsCache.ContainsKey(playerNumber))
        {
            var rotator = new FieldRotator(mainField, playerNumber, this.pointRotator);
            var concealer = new FieldConcealer(rotator, playerNumber);
            var readonlyField = new FieldReadOnly(concealer);

            this.fieldsCache[playerNumber] = readonlyField;
        }
        return this.fieldsCache[playerNumber];
    }

    public Move getMoveForPlayer(IField mainField, Move move, int playerNumberToNotify)
    {
        Point from = this.pointRotator.getRotatedPoint(mainField, move.From, playerNumberToNotify);
        Point to = this.pointRotator.getRotatedPoint(mainField, move.To, playerNumberToNotify);
        return new Move { From = from, To = to };
    }

    public AutoMove autoMove(IField mainField, int playerNumber)
    {
        var mainWidth = mainField.getWidth();
        var mainHeight = mainField.getHeight();
        var canMove = false;
        for (var i = 0; i < mainWidth; i++)
        {
            for (var j = 0; j < mainHeight; j++)
            {
                var playerShip = (KaNoBuFigure?)mainField.get(new Point { X = i, Y = j });
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
                    continue;
                }

                canMove = true;
            }
        }

        if (canMove)
        {
            return new AutoMove
            {
                move = null,
                status = AutoMove.AutoMoveStatus.NONE
            };
        }
        else
        {

            return new AutoMove
            {
                move = null,
                status = AutoMove.AutoMoveStatus.SKIP_TURN
            };
        }
    }

    public MoveValidationStatus checkMove(IField mainField, int playerNumber, Move playerMove)
    {
        var rotator = new FieldRotator(mainField, playerNumber, this.pointRotator);
        var from = (KaNoBuFigure?)rotator.get(playerMove.From);
        var to = (KaNoBuFigure?)rotator.get(playerMove.To);

        if (from == null)
        {
            return MoveValidationStatus.ERROR_INVALID_MOVE;
        }

        if (from.PlayerId != playerNumber)
        {
            return MoveValidationStatus.ERROR_INVALID_MOVE;
        }

        if (from.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
        {
            return MoveValidationStatus.ERROR_INVALID_MOVE;
        }

        var validMove =
            (playerMove.From.X == playerMove.To.X && playerMove.From.Y <= playerMove.To.Y + 1 && playerMove.From.Y >= playerMove.To.Y - 1) ||
            (playerMove.From.Y == playerMove.To.Y && playerMove.From.X <= playerMove.To.X + 1 && playerMove.From.X >= playerMove.To.X - 1);
        if (!validMove)
        {
            return MoveValidationStatus.ERROR_INVALID_MOVE;
        }

        if (to != null && to.PlayerId == from.PlayerId)
        {
            return MoveValidationStatus.ERROR_FIELD_OCCUPIED;
        }

        // ToDo:
        // if(rotator check for outside field)
        // {
        //     return MoveValidationStatus.ERROR_OUTSIDE_FIELD;
        // }

        return MoveValidationStatus.OK;
    }

    public BattleResult? makeMove(IField mainField, int playerNumber, Move playerMove)
    {
        var rotatedField = new FieldRotator(mainField, playerNumber, this.pointRotator);
        var from = (KaNoBuFigure?)rotatedField.get(playerMove.From);
        var to = (KaNoBuFigure?)rotatedField.get(playerMove.To);

        if (from == null)
        {
            throw new Exception("Move from empty field position");
        }

        if (to == null)
        {
            rotatedField.trySet(playerMove.To, from);
            rotatedField.trySet(playerMove.From, null);
            return null;
        }

        var winner = this.battle(from, to);

        if (winner == null)
        {
            return new BattleResult
            {
                attackers = new List<IFigure> { from },
                defenders = new List<IFigure> { to },
                winners = new List<IFigure>()
            };
        }

        rotatedField.trySet(playerMove.From, null);
        rotatedField.trySet(playerMove.To, null);
        rotatedField.trySet(playerMove.To, winner);

        return new BattleResult
        {
            attackers = new List<IFigure> { from },
            defenders = new List<IFigure> { to },
            winners = new List<IFigure> { winner }
        };
    }

    public List<int>? findWinners(IField mainField)
    {
        var winners = new List<int>();
        int mainWidth = mainField.getWidth();
        int mainHeight = mainField.getHeight();
        for (int i = 0; i < mainWidth; i++)
        {
            for (int j = 0; j < mainHeight; j++)
            {
                var playerShip = (KaNoBuFigure?)mainField.get(new Point { X = i, Y = j });
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

    private KaNoBuFigure? battle(KaNoBuFigure attacker, KaNoBuFigure defender)
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
}