namespace TurnBase
{
    public interface IGameLogEventListener<TInitResponseModel, TMoveResponseModel>
    {
        void GameLogCurrentField(IField field);
        void GameLogPlayerInit(int playerNumber, TInitResponseModel initResponseModel);
        void GameLogPlayerTurn(int playerNumber, TMoveResponseModel moveResponseModel, MoveValidationStatus status);
    }
}