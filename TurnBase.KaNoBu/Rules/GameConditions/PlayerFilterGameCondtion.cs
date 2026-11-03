using TurnBase;
using TurnBase.KaNoBu;

public class PlayerFilterGameCondtion : IGameCondition
{
    private readonly int playerId;

    public PlayerFilterGameCondtion(int playerId)
    {
        this.playerId = playerId;
    }

    public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
    {
        return this.playerId == playerNumber;
    }
}
