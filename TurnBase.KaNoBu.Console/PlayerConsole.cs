using System.Text;
using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class PlayerConsole : 
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>,
    IGameEventListener<KaNoBuMoveNotificationModel>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> makeTurnModel)
    {
        var field = makeTurnModel.Request.Field;
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

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(
            new KaNoBuMoveResponseModel(
                KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN,
                from.Value,
                to.Value));
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
            var xS = await Task.Run(() => Console.ReadLine());
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

        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' move from {move.From} to {move.To}.");

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