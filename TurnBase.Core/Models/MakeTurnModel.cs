namespace TurnBase.Core;

public class MakeTurnModel<TMoveModel> 
{
    public MakeTurnModel(int tryNumber, TMoveModel model)
    {
        this.tryNumber = tryNumber;
        this.Model = model;
    }

    public int tryNumber;
    public TMoveModel Model;
}
