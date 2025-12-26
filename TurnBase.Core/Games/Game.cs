using Microsoft.VisualBasic;

namespace TurnBase.Core;

public class Game<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IGameEvents<TMoveNotificationModel>
{
    private class PlayerData
    {
        public bool IsInGame;
        public int PlayerNumber;
    }

    public Game(IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules)
    {
        this.rules = rules;
        this.mainField = this.rules.generateGameField();
    }

    private IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules;
    private Dictionary<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>, PlayerData> players = new Dictionary<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>, PlayerData>();

    public event Action? GameStarted;
    public event Action<int>? GamePlayerDisconnected;
    public event Action<int, string>? GamePlayerInitialized;
    public event Action? GameTurnFinished;
    public event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
    public event Action<int, TMoveNotificationModel>? GamePlayerTurn;
    public event Action<List<int>>? GameFinished;

    private bool GameIsRunning = false;
    private IField mainField;

    public void AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
    {
        if (this.players.Count >= this.rules.getMaxPlayersCount())
        {
            throw new Exception($"Too many players added to the game. Maximum value is {this.rules.getMaxPlayersCount()}.");
        }

        if (this.GameIsRunning)
        {
            throw new Exception("Cannot add players after the game has started.");
        }

        if (this.players.ContainsKey(player))
        {
            throw new Exception("Player already added to the game.");
        }

        this.players.Add(player, new PlayerData { IsInGame = true, PlayerNumber = this.players.Count });
    }

    public async Task Play()
    {
        if (this.players.Count < this.rules.getMinPlayersCount())
        {
            throw new Exception($"Too few players added to the game. Minimum value is {this.rules.getMinPlayersCount()}.");
        }

        this.GameIsRunning = true;

        await this.InitPlayers();
        await this.MovePlayers();
    }

    private Task GroupAction(List<IPlayer> nextPlayers, Func<IPlayer, Task<bool>> action)
    {
        async Task SingleAction(IPlayer p)
        {
            if (!await action(p))
            {
                var playerData = this.players[(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)p];
                playerData.IsInGame = false;
                GamePlayerDisconnected?.Invoke(playerData.PlayerNumber);
                this.rules.PlayerDisconnected(this.mainField, playerData.PlayerNumber);
            }
        }

        return Task.WhenAll(nextPlayers.Select(SingleAction).ToArray());
    }

    private async Task InitPlayers()
    {
        var rotator = this.rules.GetInitRotator();
        var nextPlayers = rotator.MoveNext(null, this.players.Where(a => a.Value.IsInGame).Select(a => a.Key).Cast<IPlayer>().ToList());
        Task<bool> action(IPlayer player) => this.InitPlayer((IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)player);

        do
        {
            await GroupAction(nextPlayers.PlayersInTurn, action);
            nextPlayers = rotator.MoveNext(nextPlayers.PlayersInTurn, this.players.Where(a => a.Value.IsInGame).Select(a => a.Key).Cast<IPlayer>().ToList());
        } while (nextPlayers.IsNewTurn == false);
    }

    private async Task<bool> InitPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
    {
        var playerNumber = this.players[player].PlayerNumber;

        var initModel = this.rules.GetInitModel(playerNumber);

        var initResponseModel = await player.Init(new InitModel<TInitModel>(playerNumber, initModel));

        if (!initResponseModel.IsSuccess || initResponseModel.Response == null)
        {
            return false;
        }

        if (!this.rules.TryApplyInitResponse(this.mainField, playerNumber, initResponseModel.Response))
        {
            return false;
        }

        this.GamePlayerInitialized?.Invoke(playerNumber, initResponseModel.Name);
        return true;
    }

    private async Task MovePlayers()
    {
        this.GameStarted?.Invoke();
        var rotator = this.rules.GetMoveRotator();
        var nextPlayers = rotator.MoveNext(null, this.players.Where(a => a.Value.IsInGame).Select(a => a.Key).Cast<IPlayer>().ToList());
        Task<bool> action(IPlayer player) => this.MovePlayer((IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)player);

        while (this.rules.findWinners(this.mainField) == null)
        {
            await GroupAction(nextPlayers.PlayersInTurn, action);

            nextPlayers = rotator.MoveNext(nextPlayers.PlayersInTurn, this.players.Where(a => a.Value.IsInGame).Select(a => a.Key).Cast<IPlayer>().ToList());
            if (nextPlayers.IsNewTurn)
            {
                this.rules.TurnCompleted(this.mainField);
                this.GameTurnFinished?.Invoke();
            }
        }

        this.GameFinished?.Invoke(this.rules.findWinners(this.mainField)!);
    }

    private async Task<bool> MovePlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
    {
        var playerNumber = this.players[player].PlayerNumber;

        var field = this.rules.GetMoveModel(mainField, playerNumber);
        var tryNumber = 0;

        var move = this.rules.AutoMove(mainField, playerNumber);
        while (move == null)
        {
            var makeTurnResponseModel = await player.MakeTurn(new MakeTurnModel<TMoveModel>(tryNumber, field));
            tryNumber++;

            if (!makeTurnResponseModel.IsSuccess || makeTurnResponseModel.Response == null)
            {
                return false;
            }

            var validTurnStatus = this.rules.CheckMove(mainField, playerNumber, makeTurnResponseModel.Response);

            if (validTurnStatus != MoveValidationStatus.OK)
            {
                this.GamePlayerWrongTurn?.Invoke(playerNumber, validTurnStatus);
                continue;
            }

            move = makeTurnResponseModel.Response;
        }

        var moveResult = this.rules.MakeMove(mainField, playerNumber, move);
        this.GamePlayerTurn?.Invoke(playerNumber, moveResult);
        return true;
    }
}