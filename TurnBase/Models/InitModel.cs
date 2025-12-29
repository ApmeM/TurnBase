namespace TurnBase
{
    public class InitModel<TInitModel>
    {
        public InitModel(int playerId, TInitModel request)
        {
            PlayerId = playerId;
            Request = request;
        }

        public int PlayerId;
        public TInitModel Request;
    }
}