using System.Diagnostics;

namespace TurnBase.Core;

public class Game : IGameEvents
{
    public Game(IGameRules rules)
    {
        this.rules = rules;
        this.mainField = this.rules.generateGameField();
        this.readonlyField = new FieldReadOnly(this.mainField);
        this.playerRotator = this.rules.getRotator();
    }

    private IGameRules rules;
    private IField mainField;
    private IField readonlyField;
    private IPlayerRotator playerRotator;
    private List<IPlayer> players = new List<IPlayer>();

    public event Action<IField>? GameStarted;
    public event Action<int, string>? GamePlayerInitialized;
    public event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
    public event Action<int, Move, MoveResult?>? GamePlayerTurn;
    public event Action<List<int>>? GameFinished;

    private bool GameIsRunning = false;

    public void AddPlayer(IPlayer player)
    {
        Debug.Assert(this.players.Count <= this.rules.getMaxPlayersCount(), "Too many players added to the game");
        Debug.Assert(!this.GameIsRunning, "Cannot add players after the game has started.");
        this.players.Add(player);
    }

    public async Task Play(bool parallelInit)
    {
        this.GameIsRunning = true;
        var initRequests = new List<Task>();

        for (int i = 0; i < players.Count(); i++) {
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

        var initModel = this.rules.getInitializationData(playerNumber);

        var initResponseModel = await player.Init(playerNumber, initModel);

        if (!initResponseModel.IsSuccess)
        {
            throw new Exception("Player not initialized successfully.");
        }

        if (!this.rules.CheckInitResponse(playerNumber, initResponseModel))
        {
            throw new Exception("Player not initialized successfully.");
        }

        this.rules.addPlayerToField(this.mainField, initResponseModel.PreparedField, playerNumber);

        this.GamePlayerInitialized?.Invoke(playerNumber, initResponseModel.Name);
    }

    private async Task GameProcess()
    {
        this.GameStarted?.Invoke(this.readonlyField);
        List<int>? winners = null;

        while (winners == null)
        {
            int playerNumber = this.playerRotator.GetCurrent();
            IPlayer player = this.players[playerNumber];

            Move? autoMove = this.rules.autoMove(mainField, playerNumber);
            if (autoMove != null)
            {
                var moveResult = this.rules.makeMove(mainField, playerNumber, autoMove.Value);
                this.GamePlayerTurn?.Invoke(playerNumber, autoMove.Value, moveResult);
            }
            else
            {
                var field = this.rules.getFieldForPlayer(mainField, playerNumber);
                var tryNumber = 0;
                while (true)
                {
                    var makeTurnModel = new MakeTurnModel
                    {
                        field = field,
                        tryNumber = tryNumber
                    };

                    var makeTurnResponseModel = await player.MakeTurn(makeTurnModel);
                    tryNumber++;

                    var validTurnStatus =
                        !makeTurnResponseModel.isSuccess ? MoveValidationStatus.ERROR_COMMUNICATION :
                        !makeTurnResponseModel.move.HasValue ? MoveValidationStatus.ERROR_COMMUNICATION :
                        this.rules.checkMove(mainField, playerNumber, makeTurnResponseModel.move!.Value);

                    if (validTurnStatus != MoveValidationStatus.OK)
                    {
                        this.GamePlayerWrongTurn?.Invoke(playerNumber, validTurnStatus);
                        continue;
                    }

                    var moveResult = this.rules.makeMove(mainField, playerNumber, makeTurnResponseModel.move!.Value);
                    this.GamePlayerTurn?.Invoke(playerNumber, makeTurnResponseModel.move!.Value, moveResult);
                    break;
                }
            }
            this.playerRotator.MoveNext();

            winners = this.rules.findWinners(this.mainField);
        }

        this.GameFinished?.Invoke(winners);
    }
}