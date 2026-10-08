namespace TurnBase
{
    public struct ClientResponse
    {
        public int result;
        public int code;
        public string[] headers;
        public ICommunicationModel body;
    }
}