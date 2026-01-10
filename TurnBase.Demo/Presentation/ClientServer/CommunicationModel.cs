using System.Collections.Generic;

public class CommunicationModel
{
    public object Data;
}

public class GameStartedCommunicationModel
{
}

public class GamePlayerInitCommunicationModel
{
    public int playerNumber;
    public string playerName;
}

public class GamePlayerTurnCommunicationModel
{
    public int playerNumber;
    public object notification;
}

public class GameTurnFinishedCommunicationModel
{
}

public class GamePlayerDisconnectedCommunicationModel
{
    public int playerNumber;
}

public class GameFinishedCommunicationModel
{
    public List<int> winners;
}
