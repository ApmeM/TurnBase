using System.Linq;
using TurnBase;
using TurnBase.KaNoBu;

public class PlayerLooseGameCondtion : IGameCondition
{
    public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
    {
        return mainField.LostPlayers.Contains(playerNumber);
    }
}
