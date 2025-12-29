namespace TurnBase
{
    public class MakeTurnModel<TMoveModel>
    {
        public MakeTurnModel(int tryNumber, TMoveModel request)
        {
            this.TryNumber = tryNumber;
            this.Request = request;
        }

        public int TryNumber;
        public TMoveModel Request;
    }
}