using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{

    public class ServerPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        private readonly IServer server;

        public string PlayerId { get; private set; }

        public ServerPlayer(IServer server, string gameId)
        {
            this.server = server;
            this.PlayerId = Guid.NewGuid().ToString();
            this.server.RegisterPlayer(this.PlayerId, gameId);
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model, CancellationToken token = default)
        {
            return await this.server.SendRequest<InitResponseModel<TInitResponseModel>>(PlayerId, model, token);
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default)
        {
            return await this.server.SendRequest<MakeTurnResponseModel<TMoveResponseModel>>(PlayerId, model, token);
        }

        public void GameStarted()
        {
            this.server.SendRequest(PlayerId, new GameStartedCommunicationModel());
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.server.SendRequest(PlayerId, new GamePlayerInitCommunicationModel
            {
                playerNumber = playerNumber,
                playerName = playerName
            });
        }

        public void GamePlayersInitialized()
        {
            this.server.SendRequest(PlayerId, new GamePlayersInitializedCommunicationModel());
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.server.SendRequest(PlayerId, new GamePlayerTurnCommunicationModel<TMoveNotificationModel>
            {
                playerNumber = playerNumber,
                notification = notification
            });
        }

        public void GameTurnFinished()
        {
            this.server.SendRequest(PlayerId, new GameTurnFinishedCommunicationModel());
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.server.SendRequest(PlayerId, new GamePlayerDisconnectedCommunicationModel
            {
                playerNumber = playerNumber,
            });
        }

        public void GameFinished(List<int> winners)
        {
            this.server.SendRequest(PlayerId, new GameFinishedCommunicationModel
            {
                winners = winners
            });
        }
    }
}
