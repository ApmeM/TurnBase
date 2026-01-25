using System.Collections.Generic;
using TurnBase;

public class CommunicationModel
{
    public object Data;
}

public class GameStartedCommunicationModel
{
}

public class GamePlayerDisconnectedCommunicationModel
{
    public int playerNumber;
}

public class GamePlayerInitCommunicationModel
{
    public int playerNumber;
    public string playerName;
}

public class GamePlayersInitializedCommunicationModel
{
}

public class GameLogCurrentFieldCommunicationModel
{
    public IField field;
}

public class GamePlayerTurnCommunicationModel
{
    public int playerNumber;
    public object notification;
}

public class GameTurnFinishedCommunicationModel
{
}

public class GameFinishedCommunicationModel
{
    public List<int> winners;
}
