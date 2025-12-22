using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class PlayerAIEasy : IPlayer
{
    private Random r = new Random();
    private string name = "Computer easy";
    private int myNumber;

    public async Task<InitResponseModel> init(InitModel model)
    {
        await Task.Delay(0);
        this.myNumber = model.playerNumber;
        var ships = new List<IFigure>(model.availableFigures);
        var preparedField = model.preparingField.copyField();
        this.generateField(preparedField, ships);
        return new InitResponseModel
        {
            success = true,
            name = name,
            preparedField = preparedField
        };
    }

    public async Task<MakeTurnResponseModel> makeTurn(MakeTurnModel model)
    {
        await Task.Delay(0);
        List<Move> from = this.findAllMovement(model.field);

        if (from == null || from.Count == 0)
        {
            return new MakeTurnResponseModel
            {
                isSuccess = true,
                move = null,
                moveStatus = MoveStatus.SKIP_TURN
            };
        }

        int movementNum = r.Next(from.Count);

        return new MakeTurnResponseModel
        {
            isSuccess = true,
            move = from[movementNum],
            moveStatus = MoveStatus.MAKE_TURN
        };
    }

    private List<Move> findAllMovement(IField field)
    {
        var availableShips = new List<Move>();
        for (int x = 0; x < field.getWidth(); x++)
        {
            for (int y = 0; y < field.getHeight(); y++)
            {
                var from = new Point { X = x, Y = y };
                var shipFrom = field.get(from) as KaNoBuFigure;
                if (shipFrom == null)
                {
                    continue;
                }

                if (shipFrom.PlayerId != this.myNumber)
                {
                    continue;
                }

                if (shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                {
                    continue;
                }

                this.tryAdd(availableShips, field, from, x - 1, y);
                this.tryAdd(availableShips, field, from, x + 1, y);
                this.tryAdd(availableShips, field, from, x, y - 1);
                this.tryAdd(availableShips, field, from, x, y + 1);

            }
        }
        return availableShips;
    }

    private void tryAdd(List<Move> availableShips, IField field, Point from, int x, int y)
    {
        if (x < 0 || y < 0 || x >= field.getWidth() || y >= field.getHeight())
        {
            return;
        }

        var to = new Point { X = x, Y = y };
        var shipTo = field.get(to);
        if (shipTo == null || shipTo.PlayerId != this.myNumber)
        {
            availableShips.Add(new Move
            {
                From = from,
                To = to
            });
        }
    }

    private void generateField(IField preparedField, List<IFigure> ships)
    {
        var width = preparedField.getWidth();
        var height = preparedField.getHeight();
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var ship = ships[r.Next(ships.Count)];
                preparedField.trySet(new Point { X = i, Y = j }, ship);
                ships.Remove(ship);
            }
        }
    }
}