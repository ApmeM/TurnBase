using System.Text;
using System.Threading.Tasks;
using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class PlayerConsole : IPlayer
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    private string showField(IField field)
    {
        string result = "";
        result += string.Format("  ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $" {j}";
        }
        result += "\n";

        for (int i = 0; i < field.Height; i++)
        {
            result += $" {i}";
            for (int j = 0; j < field.Width; j++)
            {
                var ship = field.get(new Point { X = j, Y = i });
                result += $" {getShipResource(ship)}";
            }
            result += "\n";
        }
        return result;
    }

    private async Task<Point?> readPoint()
    {
        var x = await readCoordinate("X");
        var y = await readCoordinate("Y");
        if (x == null || y == null)
        {
            return null;
        }
        else
        {
            return new Point { X = x.Value, Y = y.Value };
        }
    }

    private async Task<int?> readCoordinate(string coordinate)
    {
        while (true)
        {
            Console.Write(coordinate + ": ");
            var xS = await Task.Run(()=>Console.ReadLine());
            if (string.IsNullOrWhiteSpace(xS))
            {
                return null;
            }

            try
            {
                return int.Parse(xS);
            }
            catch
            {
                this.showMessage("Invalid " + coordinate + " value: " + xS);
            }
        }
    }

    public async Task<MakeTurnResponseModel> MakeTurn(MakeTurnModel makeTurnModel)
    {
        var field = makeTurnModel.field;
        this.showMessage(showField(field));
        this.showMessage("Select ship to move.");
        Point? from = null;
        while (from == null)
        {
            from = await readPoint();
        }

        this.showMessage("Select destination to move.");
        Point? to = null;
        while (to == null)
        {
            to = await readPoint();
        }

        return new MakeTurnResponseModel
        {
            isSuccess = true,
            move = new Move { 
                From = from.Value, 
                To = to.Value,
                Status = MoveStatus.MAKE_TURN
            },
        };
    }

    public async Task<InitResponseModel> Init(int playerNumber, InitModel model)
    {
        this.showMessage($"Your turn number: {playerNumber}");
        var name = await this.getName();
        var ships = new List<IFigure>(model.AvailableFigures);
        var preparedField = model.PreparingField.copyField();

        await this.fillField(preparedField, ships);

        return new InitResponseModel
        {
            IsSuccess = true,
            Name = name,
            PreparedField = preparedField
        };
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
                if (setStatus != IField.SetStatus.OK)
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

    private async Task<string> getName()
    {
        this.showMessage("Please enter your name (default - unnamed):");
        var name = await Task.Run(()=>Console.ReadLine());
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "unnamed";
        }
        return name!;
    }

    public void gameStarted(IField startField)
    {
        this.showMessage("Welcome to the game.");
        this.showMessage(showField(startField));
    }

    public void playerWrongTurnMade(int playerNumber, MoveValidationStatus status)
    {
        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' made incorrect turn {status}.");
    }

    public void playerTurnMade(int playerNumber, Move move, MoveResult? battle)
    {
        if(move.Status == MoveStatus.SKIP_TURN)
        {
            this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' skip turn.");
            return;
        }

        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' move from {move.From} to {move.To}.");
        if (battle == null)
        {
            return;
        }

        StringBuilder s = new StringBuilder();
        s.AppendLine("Battle:");
        s.AppendLine("  attackers:");
        s.AppendJoin("\n", battle.Value.attackers.Select(a => $"    {this.players[a.PlayerId]}.{getShipResource(a)}"));
        s.AppendLine("\n  defenders:");
        s.AppendJoin("\n", battle.Value.defenders.Select(a => $"    {this.players[a.PlayerId]}.{getShipResource(a)}"));
        var winners = battle.Value.winners;
        if (winners == null || winners.Count == 0)
        {
            s.AppendLine("\n  winner: None (draw)");
        }
        else
        {
            s.AppendLine("\n  winner:");
            s.AppendJoin("\n", battle.Value.winners.Select(a => $"    {this.players[a.PlayerId]}.{getShipResource(a)}"));
        }
        this.showMessage(s.ToString());
    }

    public void gameFinished(List<int> winners)
    {
        this.showMessage($"Player {this.players[winners[0]]} win.");
    }

    public void playerInitialized(int playerNumber, string playerName)
    {
        this.players[playerNumber] = playerName;
        this.showMessage($"Player {playerName} initialized.");
    }

    private void showMessage(string text)
    {
        Console.WriteLine(text);
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
            return "🗎";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipScissors)
        {
            return "✂️";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipStone)
        {
            return "🪨";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
        {
            return "🚩";
        }

        throw new Exception("Unknown ship type: " + ship.FigureType);
    }
}