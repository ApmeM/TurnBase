using System.Threading;
using System.Globalization;

namespace TurnBase.KaNoBu;

public class KaNoBuPlayerConsole :
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    #region IPlayer region

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> makeTurnModel, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var field = makeTurnModel.Request.Field;
        this.showMessage(field.ToString());
        this.showMessage("Select your move in format A0-A1 (rows may have multiple digits).");
        Point? from = null;
        Point? to = null;

        while (from == null || to == null)
        {
            token.ThrowIfCancellationRequested();
            (from, to) = await readMove(field.Width, field.Height);
        }

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
        {
            Response = new KaNoBuMoveResponseModel(
                new List<KaNoBuMoveResponseModel.MoveStep>
                {
                    new KaNoBuMoveResponseModel.MoveStep(from.Value, to.Value)
                })
        };
    }

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
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

    public void GameLogCurrentField(Field2D field)
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

    private async Task<Field2D> fillField(int playerId, KaNoBuInitModel model)
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
            Point? p = await readPoint(model.Width, model.Height);
            if (p == null)
            {
                while (true)
                {
                    var x = r.Next(model.Width);
                    var y = r.Next(model.Height);
                    var point = new Point(x, y);
                    if (preparedField.GetFigure(point) != null)
                    {
                        continue;
                    }
                    preparedField.SetFigure(point, KaNoBuFigure.Create(playerId, ship, 0));
                    break;
                }
                ships.RemoveAt(0);
            }
            else
            {
                if (preparedField.GetFigure(p.Value) != null)
                {
                    this.showMessage("Cant place on this field. It is already occupied.");
                    continue;
                }
                preparedField.SetFigure(p.Value, KaNoBuFigure.Create(playerId, ship, 0));
                ships.RemoveAt(0);
            }
        }

        return preparedField;
    }

    private async Task<Point?> readPoint(int width, int height)
    {
        while (true)
        {
            var input = (await Task.Run(() => Console.ReadLine()))?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!TryParsePoint(input, out var point) || point.X >= width || point.Y >= height)
            {
                this.showMessage($"Invalid point value: {input}");
                continue;
            }

            return point;
        }
    }

    private async Task<(Point, Point)> readMove(int width, int height)
    {
        while (true)
        {
            var input = (await Task.Run(() => Console.ReadLine()))?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var parts = input.ToUpperInvariant().Split('-', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 ||
                !TryParsePoint(parts[0], out var from) ||
                !TryParsePoint(parts[1], out var to) ||
                from.X >= width || from.Y >= height || to.X >= width || to.Y >= height)
            {
                this.showMessage($"Invalid point value: {input}");
                continue;
            }

            return (from, to);
        }
    }

    private static bool TryParsePoint(string input, out Point point)
    {
        point = default;
        input = input.Trim().ToUpperInvariant();
        if (input.Length < 2 || input[0] < 'A' || input[0] > 'T' ||
            !int.TryParse(input.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var y) || y < 0)
        {
            return false;
        }

        point = new Point(input[0] - 'A', y);
        return true;
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