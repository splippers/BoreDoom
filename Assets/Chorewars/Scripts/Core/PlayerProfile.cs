using System.Collections.Generic;

namespace Chorewars.Core
{
    public class PlayerProfile
    {
        public string playerId;
        public string displayName;
        public int lifetimePoints;
        public List<ChoreSession> history = new();
    }
}
