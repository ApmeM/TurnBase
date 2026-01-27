using System.Collections.Generic;
using TurnBase;

public interface ICommunicationModel
{

}

public class CommunicationModel
{
    public ICommunicationModel Data;
}

public class InitModel<TInitModel> : ICommunicationModel
{
    public int PlayerId;
    public TInitModel Request;
}

public class InitResponseModel<TInitResponseModel> : ICommunicationModel
{
    public string Name = string.Empty;
    public TInitResponseModel Response;
}

public class JoinGameResponseModel : ICommunicationModel
{
    public string PlayerId;
}

public class MakeTurnResponseModel<TMoveResponseModel> : ICommunicationModel
{
    public TMoveResponseModel Response;
}

public class MakeTurnModel<TMoveModel> : ICommunicationModel
{
    public int TryNumber;
    public TMoveModel Request;
}

public class GameStartedCommunicationModel : ICommunicationModel
{
}

public class GamePlayerDisconnectedCommunicationModel : ICommunicationModel
{
    public int playerNumber;
}

public class GamePlayerInitCommunicationModel : ICommunicationModel
{
    public int playerNumber;
    public string playerName;
}

public class GamePlayersInitializedCommunicationModel : ICommunicationModel
{
}

public class GameLogCurrentFieldCommunicationModel : ICommunicationModel
{
    public IField field;
}

public class GamePlayerTurnCommunicationModel<TMoveNotificationModel> : ICommunicationModel
{
    public int playerNumber;
    public TMoveNotificationModel notification;
}

public class GameTurnFinishedCommunicationModel : ICommunicationModel
{
}

public class GameFinishedCommunicationModel : ICommunicationModel
{
    public List<int> winners;
}
