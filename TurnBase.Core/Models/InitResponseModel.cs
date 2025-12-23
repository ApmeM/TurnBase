namespace TurnBase.Core;

public class InitResponseModel<TInitResponseModel> {
    public bool IsSuccess;
    public string Name = string.Empty;
    public TInitResponseModel? Response;
}
