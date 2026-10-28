using System.Collections.Generic;

namespace TurnBase.KaNoBu
{
    public class KaNoBuField
    {
        public HashSet<int> DisconnectedPlayers = new HashSet<int>();

        public HashSet<int> LostPlayers = new HashSet<int>();
        
        public Field2D Field;
    }
}