namespace TurnBase.Core;

public class Game<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IGameEvents<TMoveNotificationModel>
{
    public Game(IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules)
    {
        this.rules = rules;
        this.mainField = this.rules.generateGameField();
        this.playerRotator = this.rules.getRotator();
    }

    private IGameRules<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> rules;
    private IField mainField;
    private IPlayerRotator playerRotator;
    private List<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>> players = new List<IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>>();

    public event Action? GameStarted;
    public event Action<int, string>? GamePlayerInitialized;
    public event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
    public event Action<int, TMoveNotificationModel>? GamePlayerTurn;
    public event Action<List<int>>? GameFinished;

    private bool GameIsRunning = false;

    public void AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player)
    {
        if (this.players.Count >= this.rules.getMaxPlayersCount())
        {
            throw new Exception("Too many players added to the game.");
        }

        if (this.GameIsRunning)
        {
            throw new Exception("Cannot add players after the game has started.");
        }

        if(this.players.Contains(player))
        {
            throw new Exception("Player already added to the game.");
        }

        this.players.Add(player);
    }

    public async Task Play(bool parallelInit)
    {
        this.GameIsRunning = true;
        var initRequests = new List<Task>();

        for (int i = 0; i < this.players.Count(); i++)
        {
            var initRequest = this.InitPlayer(i);
            if (parallelInit)
            {
                initRequests.Add(initRequest);
            }
            else
            {
                await initRequest;
            }
        }

        await Task.WhenAll(initRequests);

        await this.GameProcess();
    }

    private async Task InitPlayer(int playerNumber)
    {
        var player = this.players[playerNumber];

        var initModel = this.rules.GetInitModel(playerNumber);

        var initResponseModel = await player.Init(new InitModel<TInitModel>(playerNumber, initModel));

        if (!initResponseModel.IsSuccess || initResponseModel.Response == null)
        {
            throw new Exception("Player not initialized successfully.");
        }

        if (!this.rules.TryApplyInitResponse(this.mainField, playerNumber, initResponseModel.Response))
        {
            throw new Exception("Failed to apply initialization response.");
        }

        this.GamePlayerInitialized?.Invoke(playerNumber, initResponseModel.Name);
    }

    private async Task GameProcess()
    {
        this.GameStarted?.Invoke();

        while (true)
        {
            var playerNumber = this.playerRotator.GetCurrent();
            var player = this.players[playerNumber];

            var autoMove = this.rules.AutoMove(mainField, playerNumber);
            if (autoMove != null)
            {
                var moveResult = this.rules.MakeMove(mainField, playerNumber, autoMove);
                this.GamePlayerTurn?.Invoke(playerNumber, moveResult);
            }
            else
            {
                var field = this.rules.GetMoveModel(mainField, playerNumber);
                var tryNumber = 0;
                while (true)
                {
                    var makeTurnResponseModel = await player.MakeTurn(new MakeTurnModel<TMoveModel>(tryNumber, field));
                    tryNumber++;

                    var validTurnStatus =
                        !makeTurnResponseModel.isSuccess ? MoveValidationStatus.ERROR_COMMUNICATION :
                        makeTurnResponseModel.move == null ? MoveValidationStatus.ERROR_COMMUNICATION :
                        this.rules.CheckMove(mainField, playerNumber, makeTurnResponseModel.move);

                    if (validTurnStatus != MoveValidationStatus.OK)
                    {
                        this.GamePlayerWrongTurn?.Invoke(playerNumber, validTurnStatus);
                        continue;
                    }

                    var moveResult = this.rules.MakeMove(mainField, playerNumber, makeTurnResponseModel.move);
                    this.GamePlayerTurn?.Invoke(playerNumber, moveResult);
                    break;
                }
            }
            this.playerRotator.MoveNext();

            var winners = this.rules.findWinners(this.mainField);
            if (winners != null)
            {
                this.GameFinished?.Invoke(winners);
                break;
            }
        }
    }
}