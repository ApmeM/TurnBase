namespace TurnBase.KaNoBu;

public class KaNoBuPlayerConsole :
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    #region IPlayer region

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> makeTurnModel)
    {
        var field = makeTurnModel.Request.Field;
        this.showMessage(field.ToString());
        this.showMessage("Select your move in format A0-A1.");
        Point? from = null;
        Point? to = null;

        while (from == null || to == null)
        {
            (from, to) = await readMove();
        }

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
        {
            Response = new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN, from.Value, to.Value)
        };
    }

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        this.showMessage($"Your turn number: {model.PlayerId}");
        var name = await this.getName();

        var preparedField = await this.fillField(model.PlayerId, model.Request);

        return new InitResponseModel<KaNoBuInitResponseModel>
        {
            Name = name,
            Response = new KaNoBuInitResponseModel(preparedField)
        };
    }

    #endregion

    #region IGameEventListener region

    public void GameStarted()
    {
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        this.players[playerNumber] = playerName;
    }

    public void PlayersInitialized()
    {
    }

    public void GameLogCurrentField(IField field)
    {
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel battle)
    {
    }

    public void GameTurnFinished()
    {
    }

    public void GameFinished(List<int> winners)
    {
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
    }

    #endregion

    private async Task<IField> fillField(int playerId, KaNoBuInitModel model)
    {
        var ships = model.AvailableFigures;
        var preparedField = Field2D.Create(model.Width, model.Height);
        this.showMessage($"Initializing field with {ships.Count} ships.");
        var r = new Random();

        while (ships.Count != 0)
        {
            this.showMessage(preparedField.ToString());
            var ship = ships[0];
            this.showMessage($"Select position for {ship}, empty value = random.");
            Point? p = await readPoint();
            if (p == null)
            {
                while (true)
                {
                    var x = r.Next(model.Width);
                    var y = r.Next(model.Height);
                    var point = new Point(x, y);
                    if (preparedField.get(point) != null)
                    {
                        continue;
                    }
                    preparedField.trySet(point, new KaNoBuFigure(playerId, ship, true, 0));
                    break;
                }
                ships.RemoveAt(0);
            }
            else
            {
                if (preparedField.get(p.Value) != null)
                {
                    this.showMessage("Cant place on this field. It is already occupied.");
                    continue;
                }
                preparedField.trySet(p.Value, new KaNoBuFigure(playerId, ship, true, 0));
                ships.RemoveAt(0);
            }
        }

        return preparedField;
    }

    private async Task<Point?> readPoint()
    {
        while (true)
        {
            var input = await Task.Run(() => Console.ReadLine().ToUpper());
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (input.Length != 2)
            {
                this.showMessage($"Invalid point value: {input}");
                continue;
            }

            var x = input[0] - 'A';
            var y = input[1] - '0';
            return new Point { X = x, Y = y };
        }
    }

    private async Task<(Point, Point)> readMove()
    {
        while (true)
        {
            var input = await Task.Run(() => Console.ReadLine().ToUpper());
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Length != 5)
            {
                this.showMessage($"Invalid point value: {input}");
                continue;
            }

            var x1 = input[0] - 'A';
            var y1 = input[1] - '0';
            var x2 = input[3] - 'A';
            var y2 = input[4] - '0';
            return (new Point { X = x1, Y = y1 }, new Point { X = x2, Y = y2 });
        }
    }

    private async Task<string> getName()
    {
        this.showMessage("Please enter your name (default - unnamed):");
        var name = await Task.Run(() => Console.ReadLine());
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "unnamed";
        }
        return name!;
    }

    private void showMessage(string text)
    {
        Console.WriteLine(text);
    }
}