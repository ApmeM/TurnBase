namespace TurnBase.Core;

public class MakeTurnResponseModel<TMoveResponseModel> {
    public MakeTurnResponseModel(bool isSuccess, TMoveResponseModel move )
    {
        this.isSuccess = isSuccess;
        this.move = move;
    }
    public TMoveResponseModel move;
    public bool isSuccess;
}