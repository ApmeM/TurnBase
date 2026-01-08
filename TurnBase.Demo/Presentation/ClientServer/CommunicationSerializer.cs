using Newtonsoft.Json;

public class CommunicationSerializer
{
    public static T DeserializeObject<T>(string value)
    {
        return (T)JsonConvert.DeserializeObject<CommunicationModel>(value, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        }).Data;
    }
    public static string SerializeObject(object value)
    {
        return JsonConvert.SerializeObject(new CommunicationModel { Data = value }, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}
