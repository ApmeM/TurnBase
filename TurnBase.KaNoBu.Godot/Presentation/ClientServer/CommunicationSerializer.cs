using System;
using Godot;
using Newtonsoft.Json;

public class CommunicationSerializer
{
    public static T DeserializeObject<T>(string value) where T : ICommunicationModel
    {
        try
        {
            return (T)JsonConvert.DeserializeObject<CommunicationModel>(value, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            }).Data;
        }
        catch (Exception ex)
        {
            GD.Print($"Incorrect request: {ex}");
            return default;
        }

    }
    public static string SerializeObject(ICommunicationModel value)
    {
        return JsonConvert.SerializeObject(new CommunicationModel { Data = value }, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}
