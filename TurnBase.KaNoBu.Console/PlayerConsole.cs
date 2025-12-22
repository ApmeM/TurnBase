using System.Text;
using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class PlayerConsole : IPlayer
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    private string showField(IField field)
    {
        string result = "";
        result += string.Format("  ");
        for (int j = 0; j < field.getWidth(); j++)
        {
            result += $" {j}";
        }
        result += "\n";

        for (int i = 0; i < field.getHeight(); i++)
        {
            result += $" {i}";
            for (int j = 0; j < field.getWidth(); j++)
            {
                var ship = field.get(new Point { X = j, Y = i });
                result += $" {getShipResource(ship)}";
            }
            result += "\n";
        }
        return result;
    }

    private Point? readPoint()
    {
        var x = readCoordinate("X");
        var y = readCoordinate("Y");
        if (x == null || y == null)
        {
            return null;
        }
        else
        {
            return new Point { X = x.Value, Y = y.Value };
        }
    }

    private int? readCoordinate(string coordinate)
    {
        while (true)
        {
            Console.Write(coordinate + ": ");
            var xS = Console.ReadLine();
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

    public Task<MakeTurnResponseModel> makeTurn(MakeTurnModel makeTurnModel)
    {
        var field = makeTurnModel.field;
        this.showMessage(showField(field));
        this.showMessage("Select ship to move.");
        Point? from = null;
        while (from == null)
        {
            from = readPoint();
        }

        this.showMessage("Select destination to move.");
        Point? to = null;
        while (to == null)
        {
            to = readPoint();
        }

        return Task.FromResult(new MakeTurnResponseModel
        {
            isSuccess = true,
            move = new Move { From = from.Value, To = to.Value },
            moveStatus = MoveStatus.MAKE_TURN
        });
    }

    public Task<InitResponseModel> init(InitModel model)
    {
        var playerNumber = model.playerNumber;
        this.showMessage($"Your turn number: {playerNumber}");
        var name = this.getName();
        var ships = new List<IFigure>(model.availableFigures);
        var preparedField = model.preparingField.copyField();

        this.fillField(preparedField, ships);

        return Task.FromResult(new InitResponseModel
        {
            success = true,
            name = name,
            preparedField = preparedField
        });
    }

    private void fillField(IField preparedField, List<IFigure> ships)
    {
        this.showMessage($"Initializing field with {ships.Count} ships.");
        var width = preparedField.getWidth();
        var height = preparedField.getHeight();
        var r = new Random();

        while (ships.Count != 0)
        {
            this.showMessage(showField(preparedField));
            var ship = ships[0];
            this.showMessage("Select position for " + getShipResource(ship) + ", empty value = random.");
            Point? p = readPoint();
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
                if (!preparedField.trySet(p.Value, ship))
                {
                    this.showMessage("Cant set ship at this coordinate.");
                }
                else
                {
                    ships.RemoveAt(0);
                }
            }
        }
    }

    private string getName()
    {
        this.showMessage("Please enter your name (default - unnamed):");
        var name = Console.ReadLine();
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

    public void playerTurnMade(int playerNumber, MoveStatus status, Move? move, BattleResult? battle)
    {
        if(status == MoveStatus.SKIP_TURN)
        {
            this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' skip turn.");
            return;
        }

        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' move from {move!.Value.From} to {move!.Value.To}.");
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