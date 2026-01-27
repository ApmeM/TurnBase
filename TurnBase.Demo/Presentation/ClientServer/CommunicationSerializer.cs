using Godot;
using Newtonsoft.Json;

public class CommunicationSerializer
{
    public static T DeserializeObject<T>(string value) where T : ICommunicationModel
    {
        GD.Print($"ORIGINAL: {value}");

        var result = (T)JsonConvert.DeserializeObject<CommunicationModel>(value, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        }).Data;

        GD.Print($"RESULT: {SerializeObject(result)}");

        return result;
    }
    public static string SerializeObject(ICommunicationModel value)
    {
        return JsonConvert.SerializeObject(new CommunicationModel { Data = value }, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}
