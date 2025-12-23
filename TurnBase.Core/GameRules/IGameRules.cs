namespace TurnBase.Core;

public interface IGameRules
{
      // Preparing functions.
    IField generateGameField();
    int getMaxPlayersCount();
    IPlayerRotator getRotator();

    // Player initialization functions.
    InitModel getInitializationData(int playerNumber);
    bool CheckInitResponse(int playerNumber, InitResponseModel preparedField);
    void addPlayerToField(IField mainField, IField playerField, int playerNumber);

    // Game functions.
    IField getFieldForPlayer(IField mainField, int playerNumber);
    Move getMoveForPlayer(IField mainField, Move move, int playerNumberToNotify);
    Move? autoMove(IField mainField, int playerNumber);
    MoveValidationStatus checkMove(IField mainField, int playerNumber, Move move);
    MoveResult? makeMove(IField mainField, int playerNumber, Move playerMove);
    List<int>? findWinners(IField mainField);
}