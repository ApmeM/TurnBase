namespace TurnBase
{
    public class InitResponseModel<TInitResponseModel>
    {
        public InitResponseModel(string name, TInitResponseModel response)
        {
            this.IsSuccess = true;
            this.Name = name;
            this.Response = response;
        }

        public InitResponseModel()
        {
            this.IsSuccess = false;
            this.Name = string.Empty;
            this.Response = default;
        }

        public bool IsSuccess;
        public string Name = string.Empty;
        public TInitResponseModel Response;
    }
}