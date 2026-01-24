namespace TurnBase.KaNoBu;

public class KaNoBuPlayerConsole :
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>,
    IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    #region IPlayer region

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> makeTurnModel)
    {
        var field = makeTurnModel.Request.Field;
        this.showMessage(showField(field));
        this.showMessage("Select your to move in format A0-A1.");
        Point? from = null;
        Point? to = null;

        while (from == null || to == null)
        {
            (from, to) = await readMove();
        }

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN, from.Value, to.Value));
    }

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        this.showMessage($"Your turn number: {model.PlayerId}");
        var name = await this.getName();

        var preparedField = await this.fillField(model.Request);

        return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
    }

    #endregion

    #region IGameEventListener region

    public void GameStarted()
    {
        this.showMessage("Welcome to the game.");
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        this.players[playerNumber] = playerName;
        this.showMessage($"Player {playerName} initialized.");
    }

    public void GameLogPlayerInit(int playerNumber, KaNoBuInitResponseModel initResponseModel)
    {
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel battle)
    {
        var move = battle.move;
        if (move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' skip turn.");
            return;
        }

        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' move {showPoint(move.From)}-{showPoint(move.To)}.");

        if (battle.battle != null)
        {
            this.showMessage($"Battle result: {battle.battle.Value.battleResult} (IsFlag = {battle.battle.Value.isDefenderFlag})");
        }
    }

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveResponseModel moveResponseModel, MoveValidationStatus status)
    {
        if (status != MoveValidationStatus.OK)
        {
            this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' made incorrect turn {showPoint(moveResponseModel.From)}-{showPoint(moveResponseModel.To)} with status {status}.");
        }
    }

    public void GameTurnFinished()
    {
        this.showMessage("Turn finished.");
    }

    public void GameFinished(List<int> winners)
    {
        this.showMessage($"Player {this.players[winners[0]]} win.");
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.showMessage($"Player {playerNumber} disconnected.");
    }

    public void GameLogCurrentField(IField field)
    {
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }

    #endregion

    private async Task<KaNoBuFigure.FigureTypes[,]> fillField(KaNoBuInitModel model)
    {
        var ships = model.AvailableFigures;
        var preparedField = new KaNoBuFigure.FigureTypes[model.Width, model.Height];
        var preparedFieldSet = new bool[model.Width, model.Height];
        this.showMessage($"Initializing field with {ships.Count} ships.");
        var r = new Random();

        while (ships.Count != 0)
        {
            this.showMessage(showField(preparedField));
            var ship = ships[0];
            this.showMessage($"Select position for {ship}, empty value = random.");
            Point? p = await readPoint();
            if (p == null)
            {
                while (true)
                {
                    var x = r.Next(model.Width);
                    var y = r.Next(model.Height);
                    if (preparedFieldSet[x, y])
                    {
                        continue;
                    }
                    preparedField[x, y] = ship;
                    preparedFieldSet[x, y] = true;
                    break;
                }
                ships.RemoveAt(0);
            }
            else
            {
                if (preparedFieldSet[p.Value.X, p.Value.Y])
                {
                    this.showMessage("Cant place on this field. It is already occupied.");
                    continue;
                }
                preparedField[p.Value.X, p.Value.Y] = ship;
                preparedFieldSet[p.Value.X, p.Value.Y] = true;
                // ToDo: check if field already occupied.
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

    private string showField(KaNoBuFigure.FigureTypes[,] field)
    {
        string result = "";
        result += string.Format("   ");
        for (int j = 0; j < field.GetLength(0); j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");
        result += "\n";

        for (int i = 0; i < field.GetLength(1); i++)
        {
            result += $"  {i}";
            for (int j = 0; j < field.GetLength(0); j++)
            {
                var ship = field[j, i];
                result += $"  {getShipResource(ship)}";
            }

            result += $"  {i}\n";
        }

        result += string.Format("   ");
        for (int j = 0; j < field.GetLength(0); j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");

        return result;
    }

    private string showField(KaNoBuMoveModel.FigureModel?[,] field)
    {
        string result = "";
        result += string.Format("   ");
        for (int j = 0; j < field.GetLength(0); j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");
        result += "\n";

        for (int i = 0; i < field.GetLength(1); i++)
        {
            result += $"  {i}";
            for (int j = 0; j < field.GetLength(0); j++)
            {
                var ship = field[j, i];
                result += $"  {getShipResource(ship?.FigureType)}";
            }

            result += $"  {i}\n";
        }

        result += string.Format("   ");
        for (int j = 0; j < field.GetLength(0); j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");

        return result;
    }

    private string showField(IField field)
    {
        string result = "";
        result += string.Format("   ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");
        result += "\n";

        for (int i = 0; i < field.Height; i++)
        {
            result += $"  {i}";
            for (int j = 0; j < field.Width; j++)
            {
                var ship = field.get(new Point { X = j, Y = i });
                result += $"  {getShipResource((ship == null) ? null : ((ship as KaNoBuFigure)?.FigureType ?? KaNoBuFigure.FigureTypes.Unknown))}";
            }

            result += $"   {i}\n";
        }

        result += string.Format("   ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");

        return result;
    }

    private string showPoint(Point point)
    {
        return $"({(char)('A' + point.X)}{point.Y})";
    }

    private string getShipResource(KaNoBuFigure.FigureTypes? figureType)
    {
        if (figureType == null)
        {
            return " ";
        }

        return figureType.Value.PrintableName();
    }
}