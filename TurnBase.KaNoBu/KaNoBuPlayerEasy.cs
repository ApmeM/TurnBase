using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class KaNoBuPlayerEasy : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>
{
    private Random r = new Random();
    private string name = "Computer easy";
    private int myNumber;

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        await Task.Delay(200);
        this.myNumber = model.PlayerId;
        var ships = new List<IFigure>(model.Request.AvailableFigures);
        var preparedField = new Field2D(model.Request.Width, model.Request.Height);
        this.generateField(preparedField, ships);
        return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
    {
        await Task.Delay(200);
        var from = this.findAllMovement(model.Request.Field);

        if (from == null || from.Count == 0)
        {
            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN));
        }

        int movementNum = r.Next(from.Count);

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(from[movementNum]);
    }

    private List<KaNoBuMoveResponseModel> findAllMovement(IField field)
    {
        var availableShips = new List<KaNoBuMoveResponseModel>();
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

    private void tryAdd(List<KaNoBuMoveResponseModel> availableShips, IField field, Point from, int x, int y)
    {
        if (x < 0 || y < 0 || x >= field.Width || y >= field.Height)
        {
            return;
        }

        var to = new Point { X = x, Y = y };
        var shipTo = field.get(to);
        if (shipTo == null || shipTo.PlayerId != this.myNumber)
        {
            availableShips.Add(new KaNoBuMoveResponseModel(
                KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN,
                from,
                to
            ));
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