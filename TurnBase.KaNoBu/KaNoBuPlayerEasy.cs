using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class KaNoBuPlayerEasy : IPlayer
{
    private Random r = new Random();
    private string name = "Computer easy";
    private int myNumber;

    public async Task<InitResponseModel> Init(int playerNumber, InitModel model)
    {
        await Task.Delay(0);
        this.myNumber = playerNumber;
        var ships = new List<IFigure>(model.AvailableFigures);
        var preparedField = model.PreparingField.copyField();
        this.generateField(preparedField, ships);
        return new InitResponseModel
        {
            IsSuccess = true,
            Name = name,
            PreparedField = preparedField
        };
    }

    public async Task<MakeTurnResponseModel> MakeTurn(MakeTurnModel model)
    {
        await Task.Delay(0);
        List<Move> from = this.findAllMovement(model.field);

        if (from == null || from.Count == 0)
        {
            return new MakeTurnResponseModel
            {
                isSuccess = true,
                move = new Move
                {
                    Status = MoveStatus.SKIP_TURN
                },
            };
        }

        int movementNum = r.Next(from.Count);

        return new MakeTurnResponseModel
        {
            isSuccess = true,
            move = from[movementNum],
        };
    }

    private List<Move> findAllMovement(IField field)
    {
        var availableShips = new List<Move>();
        for (int x = 0; x < field.Width; x++)
        {
            for (int y = 0; y < field.Height; y++)
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
        if (x < 0 || y < 0 || x >= field.Width || y >= field.Height)
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
                To = to,
                Status = MoveStatus.MAKE_TURN
            });
        }
    }

    private void generateField(IField preparedField, List<IFigure> ships)
    {
        var width = preparedField.Width;
        var height = preparedField.Height;
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