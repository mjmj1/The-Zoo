using Unity.Netcode;

namespace GamePlay
{
    public class ObserverManager : NetworkBehaviour
    {
        public NetworkList<ulong> observerIds = new();

        [Rpc(SendTo.Authority)]
        public void AddRpc(ulong observerId)
        {
            observerIds.Add(observerId);
        }
    }
}