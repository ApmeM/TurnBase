namespace TurnBase.Core
{
    public class MakeTurnResponseModel<TMoveResponseModel>
    {
        public MakeTurnResponseModel(TMoveResponseModel move)
        {
            this.IsSuccess = true;
            this.Response = move;
        }

        public MakeTurnResponseModel()
        {
            this.IsSuccess = false;
            this.Response = default;
        }

        public TMoveResponseModel Response;
        public bool IsSuccess;
    }
}