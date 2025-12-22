namespace TurnBase.Core;

public interface IGameRules
{
      // Preparing functions.
    IField generateGameField();
    int getPlayersCount();
    IPlayerRotator getRotator();

    // Player initialization functions.
    PlayerInitialization getInitializationData(int playerNumber);
    bool checkPreparedField(int playerNumber, IField preparedField);
    void addPlayerToField(IField mainField, IField playerField, int playerNumber);

    // Game functions.
    IField getFieldForPlayer(IField mainField, int playerNumber);
    Move getMoveForPlayer(IField mainField, Move move, int playerNumberToNotify);
    AutoMove autoMove(IField mainField, int playerNumber);
    MoveValidationStatus checkMove(IField mainField, int playerNumber, Move move);
    BattleResult? makeMove(IField mainField, int playerNumber, Move playerMove);
    List<int>? findWinners(IField mainField);
}