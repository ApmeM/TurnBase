using System.Text;
using TurnBase;

namespace TurnBase.KaNoBu;

public class KaNoBuPlayerConsole : 
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>,
    IGameEventListener<KaNoBuMoveNotificationModel>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

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

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(from.Value, to.Value));
    }

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        this.showMessage($"Your turn number: {model.PlayerId}");
        var name = await this.getName();
        var ships = new List<IFigure>(model.Request.AvailableFigures);
        var preparedField = new Field2D(model.Request.Width, model.Request.Height);

        await this.fillField(preparedField, ships);

        return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
    }

    private async Task fillField(IField preparedField, List<IFigure> ships)
    {
        this.showMessage($"Initializing field with {ships.Count} ships.");
        var width = preparedField.Width;
        var height = preparedField.Height;
        var r = new Random();

        while (ships.Count != 0)
        {
            this.showMessage(showField(preparedField));
            var ship = ships[0];
            this.showMessage("Select position for " + getShipResource(ship) + ", empty value = random.");
            Point? p = await readPoint();
            if (p == null)
            {
                while (true)
                {
                    var x = r.Next(width);
                    var y = r.Next(height);
                    p = new Point { X = x, Y = y };
                    if (preparedField.get(p.Value) != null)
                    {
                        continue;
                    }
                    preparedField.trySet(p.Value, ship);
                    break;
                }
                ships.RemoveAt(0);
            }
            else
            {
                var setStatus = preparedField.trySet(p.Value, ship);
                if (setStatus != SetStatus.OK)
                {
                    this.showMessage($"Cant set ship at this coordinate: {setStatus}");
                }
                else
                {
                    ships.RemoveAt(0);
                }
            }
        }
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

            if(input.Length != 2)
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

            if(input.Length != 5)
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
                result += $"  {getShipResource(ship)}";
            }

            result += $"  {i}\n";
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

    private string getShipResource(IFigure? figure)
    {
        if (figure == null)
        {
            return " ";
        }
        else if (figure is UnknownFigure)
        {
            return "?";
        }

        var ship = (KaNoBuFigure)figure;

        if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipPaper)
        {
            return "P";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipScissors)
        {
            return "S";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipStone)
        {
            return "R";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
        {
            return "F";
        }

        throw new Exception("Unknown ship type: " + ship.FigureType);
    }


#region IGameEventListener implementation

    public void GameStarted()
    {
        this.showMessage("Welcome to the game.");
    }

    public void GamePlayerWrongTurn(int playerNumber, MoveValidationStatus status)
    {
        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' made incorrect turn {status}.");
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
            var s = new StringBuilder();
            s.AppendLine("Battle:");
            s.AppendLine($"  attacker: {this.players[battle.battle.Value.Item1.PlayerId]}.{getShipResource(battle.battle.Value.Item1)}");
            s.AppendLine($"  defender: {this.players[battle.battle.Value.Item2.PlayerId]}.{getShipResource(battle.battle.Value.Item2)}");
            if (battle.battle.Value.Item3 != null)
                s.AppendLine($"  winner: {this.players[battle.battle.Value.Item3.PlayerId]}.{getShipResource(battle.battle.Value.Item3)}");
            else
                s.AppendLine("  winner: None (draw)");

            this.showMessage(s.ToString());
        }
    }

    public void GameFinished(List<int> winners)
    {
        this.showMessage($"Player {this.players[winners[0]]} win.");
    }

    public void GamePlayerInitialized(int playerNumber, string playerName)
    {
        this.players[playerNumber] = playerName;
        this.showMessage($"Player {playerName} initialized.");
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.showMessage($"Player {playerNumber} disconnected.");
    }

    public void GameTurnFinished()
    {
        this.showMessage("Turn finished.");
    }

#endregion

}