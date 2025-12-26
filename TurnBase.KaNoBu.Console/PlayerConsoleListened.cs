using System.Text;
using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class PlayerConsoleListener : 
    IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    private Dictionary<int, string> players = new Dictionary<int, string>();

    private string showField(IField field)
    {
        string result = "";
        result += string.Format("   ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $"  {j}";
        }
        result += "\n";

        for (int i = 0; i < field.Height; i++)
        {
            result += $"  {i}";
            for (int j = 0; j < field.Width; j++)
            {
                var ship = field.get(new Point { X = j, Y = i });
                result += $" {getShipResource(ship)}";
            }
            result += "\n";
        }
        return result;
    }

    public void GameLogStarted(IField field)
    {
        this.showMessage("Welcome to the game.");
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }

    public void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
        this.showMessage($"Player {playerNumber} '{this.players[playerNumber]}' made incorrect turn {moveResponseModel} with status {status}.");
    }

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveNotificationModel battle, KaNoBuMoveResponseModel moveResponseModel, IField field)
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

    public void GameLogFinished(List<int> winners, IField field)
    {
        this.showMessage($"Player {this.players[winners[0]]} win.");
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.showMessage($"Player {playerNumber} disconnected.");
    }

    public void GameLogPlayerInitialized(int playerNumber, InitResponseModel<KaNoBuInitResponseModel> initResponse, IField field)
    {
        this.players[playerNumber] = initResponse.Name;
        this.showMessage($"Player {initResponse.Name} initialized with id {playerNumber}.");
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }

    public void GameLogTurnFinished(IField field)
    {
        this.showMessage("Turn finished.");
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }

    private void showMessage(string text)
    {
        Console.WriteLine(text);
    }

    private string getShipResource(IFigure? figure)
    {
        if (figure == null)
        {
            return "  ";
        }
        else if (figure is UnknownFigure)
        {
            return figure.PlayerId + "?";
        }

        var ship = (KaNoBuFigure)figure;

        if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipPaper)
        {
            return figure.PlayerId + "P";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipScissors)
        {
            return figure.PlayerId + "S";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipStone)
        {
            return figure.PlayerId + "R";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
        {
            return figure.PlayerId + "F";
        }

        throw new Exception("Unknown ship type: " + ship.FigureType);
    }

    public void GameLogPlayerDisconnected(int playerNumber, IField field)
    {
        this.showMessage($"Player {playerNumber} disconnected.");
        this.showMessage("Field:");
        this.showMessage(this.showField(field));
    }
}