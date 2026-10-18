using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    /// <summary>
    /// Creates KaNoBu players while leaving UI, console, and transport implementations to the host.
    /// </summary>
    public static class KaNoBuPlayerFactory
    {
        public struct GameParameters
        {
            public bool IsClient;
            public string GameId;
            public string ServerAddress;
            public int FieldSize;
            public bool WithDocks;
            public bool EnemyVisible;
            public int MaxMovesPerTurn;
            public List<KaNoBuPlayerType> PlayerTypes;
        }

        public static IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> Create(
            KaNoBuPlayerType playerType,
            IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> humanPlayer,
            IServer server,
            string gameId,
            Func<int, Task> delayAction)
        {
            switch (playerType)
            {
                case KaNoBuPlayerType.None:
                    return new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>();
                case KaNoBuPlayerType.Human:
                    return humanPlayer ?? throw new ArgumentNullException(nameof(humanPlayer));
                case KaNoBuPlayerType.Easy:
                    return CreateDelayed(new KaNoBuPlayerEasy(), delayAction);
                case KaNoBuPlayerType.Medium:
                    return CreateDelayed(new KaNoBuPlayerMedium(), delayAction);
                case KaNoBuPlayerType.Remote:
                    return new TimeoutPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(
                        new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(server, gameId),
                        delayAction,
                        600000,
                        60000);
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerType));
            }
        }

        private static IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> CreateDelayed(
            IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> player,
            Func<int, Task> delayAction)
        {
            if (delayAction == null)
            {
                return player;
            }

            return new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(
                player, delayAction, 1, 300);
        }

        public static string NormalizeServerAddress(string serverAddress)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
            {
                return serverAddress;
            }

            var trimmedAddress = serverAddress.Trim();
            var uriText = trimmedAddress.IndexOf("://", StringComparison.Ordinal) >= 0 ? trimmedAddress : $"http://{trimmedAddress}";

            if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri))
            {
                return trimmedAddress;
            }

            var builder = new UriBuilder(uri)
            {
                Port = uri.IsDefaultPort ? RemoteProtocol.DefaultPort : uri.Port
            };

            return builder.Uri.ToString().TrimEnd('/');
        }

        public static IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> CreateGame(
            GameParameters parameters,
            IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> humanPlayer,
            IClient client,
            ILogger logger,
            IServer server,
            Func<int, Task> delayAction)
        {
            var humansCount = parameters.PlayerTypes.Count(a => a == KaNoBuPlayerType.Human);
            if (humansCount > 1)
            {
                logger.Log("Max one human player is allowed.");
                return null;
            }

            IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> game;

            if (parameters.IsClient)
            {
                if (string.IsNullOrWhiteSpace(parameters.ServerAddress))
                {
                    logger.Log("Server address is required in client mode.");
                    return null;
                }

                var normalizedAddress = NormalizeServerAddress(parameters.ServerAddress);
                game = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(
                    client,
                    normalizedAddress,
                    parameters.GameId);
            }
            else
            {
                var rules = new KaNoBuRules(parameters.FieldSize)
                {
                    WithDocks = parameters.WithDocks,
                    EnemyVisible = parameters.EnemyVisible,
                    MaxMovesPerTurn = parameters.MaxMovesPerTurn
                };

                game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(
                    rules,
                    parameters.GameId);
            }

            foreach (var playerType in parameters.PlayerTypes)
            {
                var player = Create(
                    playerType,
                    humanPlayer,
                    server,
                    parameters.GameId,
                    delayAction);

                var status = game.AddPlayer(player);
                if (status != AddPlayerStatus.OK)
                {
                    logger.Log($"Не удалось добавить игрока: {status}.");
                    return null;
                }
            }

            if (logger != null)
            {
                var readableLogger = new ReadableLogger<KaNoBuMoveNotificationModel, KaNoBuField>(logger);
                game.AddGameLogListener(readableLogger);
            }

            if (humansCount == 0)
            {
                game.AddGameLogListener(humanPlayer);
            }

            return game;
        }
    }
}