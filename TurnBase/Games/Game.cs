using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TurnBase
{
    public class Game<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        private class PlayerData
        {
            public bool IsInGame;
            public int PlayerNumber;
        }

        public Game(IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules, string gameId)
        {
            this.GameId = gameId;
            this.rules = rules;
            this.mainField = this.rules.generateGameField();
        }

        public string GameId { get; private set; }

        private IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules;
        private Dictionary<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>, PlayerData> players = new Dictionary<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>, PlayerData>();
        private List<IGameLogEventListener<TInitResponseModel, TMoveResponseModel>> gameLogListeners = new List<IGameLogEventListener<TInitResponseModel, TMoveResponseModel>>();
        private List<IGameEventListener<TMoveNotificationModel>> gameListeners = new List<IGameEventListener<TMoveNotificationModel>>();
        private bool GameIsRunning = false;
        private IField mainField;

        public AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
        {
            if (this.players.Count >= this.rules.getMaxPlayersCount())
            {
                return AddPlayerStatus.MAX_PLAYERS_REACHED;
            }

            if (this.GameIsRunning)
            {
                return AddPlayerStatus.GAME_ALREADY_STARTED;
            }

            if (this.players.ContainsKey(player))
            {
                return AddPlayerStatus.PLAYER_ALREADY_ADDED;
            }

            this.players.Add(player, new PlayerData { IsInGame = true, PlayerNumber = this.players.Count });
            return AddPlayerStatus.OK;
        }

        public void AddGameListener(IGameEventListener<TMoveNotificationModel> gameListener)
        {
            this.gameListeners.Add(gameListener);
        }

        public void AddGameLogListener(IGameLogEventListener<TInitResponseModel, TMoveResponseModel> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public async Task Play()
        {
            for (var i = this.players.Count; i < this.rules.getMinPlayersCount(); i++)
            {
                // Add enough players, but all of them will loose.
                this.AddPlayer(new PlayerLoose<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>());
            }

            this.GameIsRunning = true;

            this.gameListeners.ForEach(a => a.GameStarted());

            await this.InitPlayers();
            await this.MovePlayers();
        }

        private Task GroupAction(List<IPlayer> nextPlayers, Func<IPlayer, Task<bool>> action)
        {
            async Task SingleAction(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> p)
            {
                if (await action(p))
                {
                    return;
                }

                var playerData = this.players[p];
                playerData.IsInGame = false;
                this.rules.PlayerDisconnected(this.mainField, playerData.PlayerNumber);
                this.gameListeners.ForEach(a => a.GamePlayerDisconnected(playerData.PlayerNumber));
            }

            return Task.WhenAll(
                nextPlayers
                    .Select(a => (IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)a)
                    .Where(a => this.players[a].IsInGame)
                    .Select(SingleAction)
                    .ToArray());
        }

        private async Task InitPlayers()
        {
            var rotator = this.rules.GetInitRotator();
            var allPlayers = this.players.Keys.Cast<IPlayer>().ToList();

            var nextPlayers = rotator.MoveNext(null, allPlayers);
            Task<bool> action(IPlayer player) => this.InitPlayer((IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)player);

            do
            {
                await GroupAction(nextPlayers.PlayersInTurn, action);
                nextPlayers = rotator.MoveNext(nextPlayers.PlayersInTurn, allPlayers);
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

            this.gameListeners.ForEach(a => a.GamePlayerInit(playerNumber, initResponseModel.Name));
            this.gameLogListeners.ForEach(a => a.GameLogPlayerInit(playerNumber, initResponseModel.Response));
            this.gameLogListeners.ForEach(a => a.GameLogCurrentField(this.mainField));
            return true;
        }

        private async Task MovePlayers()
        {
            this.gameLogListeners.ForEach(a => a.GameLogCurrentField(this.mainField));

            var rotator = this.rules.GetMoveRotator();
            var allPlayers = this.players.Keys.Cast<IPlayer>().ToList();

            var nextPlayers = rotator.MoveNext(null, allPlayers);
            Task<bool> action(IPlayer player) => this.MovePlayer((IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>)player);

            while (this.rules.findWinners(this.mainField) == null)
            {
                await GroupAction(nextPlayers.PlayersInTurn, action);

                nextPlayers = rotator.MoveNext(nextPlayers.PlayersInTurn, allPlayers);
                if (nextPlayers.IsNewTurn)
                {
                    this.rules.TurnCompleted(this.mainField);
                    this.gameListeners.ForEach(a => a.GameTurnFinished());
                    this.gameLogListeners.ForEach(a => a.GameLogCurrentField(this.mainField));
                }
            }

            this.gameListeners.ForEach(a => a.GameFinished(this.rules.findWinners(this.mainField)));
            this.gameLogListeners.ForEach(a => a.GameLogCurrentField(this.mainField));
        }

        private async Task<bool> MovePlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
        {
            var playerNumber = this.players[player].PlayerNumber;

            var field = this.rules.GetMoveModel(this.mainField, playerNumber);
            var tryNumber = 0;

            var move = this.rules.AutoMove(this.mainField, playerNumber);
            while (move == null)
            {
                var makeTurnResponseModel = await player.MakeTurn(new MakeTurnModel<TMoveModel>(tryNumber, field));
                tryNumber++;

                if (!makeTurnResponseModel.IsSuccess || makeTurnResponseModel.Response == null)
                {
                    return false;
                }

                var validTurnStatus = this.rules.CheckMove(this.mainField, playerNumber, makeTurnResponseModel.Response);

                if (validTurnStatus != MoveValidationStatus.OK)
                {
                    this.gameLogListeners.ForEach(a => a.GameLogPlayerTurn(playerNumber, makeTurnResponseModel.Response, validTurnStatus));
                    continue;
                }

                move = makeTurnResponseModel.Response;
            }

            var moveResult = this.rules.MakeMove(this.mainField, playerNumber, move);
            this.gameListeners.ForEach(a => a.GamePlayerTurn(playerNumber, moveResult));
            this.gameLogListeners.ForEach(a => a.GameLogPlayerTurn(playerNumber, move, MoveValidationStatus.OK));
            this.gameLogListeners.ForEach(a => a.GameLogCurrentField(this.mainField));
            return true;
        }
    }
}