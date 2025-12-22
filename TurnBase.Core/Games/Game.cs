using System.Diagnostics;

namespace TurnBase.Core;

public class Game : IGame, IGameEvents
{
  public Game(IGameRules rules)
  {
    this.rules = rules;
    this.mainField = this.rules.generateGameField();
    this.readonlyField = new FieldReadOnly(this.mainField);
    this.playerRotator = this.rules.getRotator();
    this.players = new List<IPlayer>();
  }

  private IGameRules rules;
  private IField mainField;
  private IField readonlyField;
  private IPlayerRotator playerRotator;
  private List<IPlayer> players;

  public event Action<IField>? GameStarted;
  public event Action<int, string>? GamePlayerInitialized;
  public event Action<int, MoveValidationStatus>? GamePlayerWrongTurn;
  public event Action<List<int>>? GameFinished;
  public event Action<int, MoveStatus, Move?, BattleResult?>? GamePlayerTurn;

  public void addPlayer(IPlayer player)
  {
    Debug.Assert(this.players.Count <= this.rules.getPlayersCount(), "Too many players added to the game");

    this.players.Add(player);
  }

  public IReadOnlyCollection<IPlayer> getPlayers()
  {
    return players;
  }

  public async Task initPlayer(int playerNumber)
  {
    var player = this.players[playerNumber];

    // Init player
    PlayerInitialization initData = this.rules.getInitializationData(playerNumber);

    InitModel initModel = new InitModel
    {
      playerNumber = playerNumber,
      preparingField = initData.preparingField,
      availableFigures = initData.availableFigures
    };

    InitResponseModel initResponseModel = await player.init(initModel);

    if (!initResponseModel.success)
    {
      throw new Exception("Player not initialized successfully.");
    }

    if (!this.rules.checkPreparedField(playerNumber, initResponseModel.preparedField))
    {
      throw new Exception("Player not initialized successfully.");
    }

    this.rules.addPlayerToField(this.mainField, initResponseModel.preparedField, playerNumber);

    this.GamePlayerInitialized?.Invoke(playerNumber, initResponseModel.name);
  }

  public async Task gameProcess()
  {
    this.GameStarted?.Invoke(this.readonlyField);
    List<int>? winners = null;

    while (winners == null)
    {
      int playerNumber = this.playerRotator.getCurrent();
      IPlayer player = this.players[playerNumber];

      AutoMove autoMove = this.rules.autoMove(mainField, playerNumber);
      switch (autoMove.status)
      {
        case AutoMove.AutoMoveStatus.SKIP_TURN:
          this.makeMove(playerNumber, MoveStatus.SKIP_TURN, null);
          break;
        case AutoMove.AutoMoveStatus.MAKE_TURN:
          this.makeMove(playerNumber, MoveStatus.MAKE_TURN, autoMove.move);
          break;
        case AutoMove.AutoMoveStatus.NONE:
          IField field = this.rules.getFieldForPlayer(mainField, playerNumber);
          int tryNumber = 0;
          MakeTurnResponseModel makeTurnResponseModel;
          MoveValidationStatus validTurnStatus;
          do
          {
            MakeTurnModel makeTurnModel = new MakeTurnModel
            {
              field = field,
              tryNumber = tryNumber
            };
            makeTurnResponseModel = await player.makeTurn(makeTurnModel);
            tryNumber++;
            validTurnStatus = checkMove(playerNumber, makeTurnResponseModel);
            if (validTurnStatus != MoveValidationStatus.OK)
            {
              this.GamePlayerWrongTurn?.Invoke(playerNumber, validTurnStatus);
            }

          } while (validTurnStatus != MoveValidationStatus.OK);

          this.makeMove(playerNumber, makeTurnResponseModel.moveStatus, makeTurnResponseModel.move);
          break;
      }

      this.playerRotator.moveNext();

      winners = this.rules.findWinners(this.mainField);
    }

    this.GameFinished?.Invoke(winners);
  }

  private void makeMove(int playerNumber, MoveStatus status, Move? move)
  {
    if (status == MoveStatus.MAKE_TURN)
    {
      var battle = this.rules.makeMove(mainField, playerNumber, move!.Value);
      this.GamePlayerTurn?.Invoke(playerNumber, status, move, battle);
    }
    else
    {
      this.GamePlayerTurn?.Invoke(playerNumber, status, move, null);
    }
  }

  private MoveValidationStatus checkMove(int playerNumber, MakeTurnResponseModel playerMove)
  {
    MoveStatus status = playerMove.moveStatus;
    if (status == MoveStatus.SKIP_TURN)
    {
      return MoveValidationStatus.OK;
    }

    return this.rules.checkMove(mainField, playerNumber, playerMove.move!.Value);
  }
}