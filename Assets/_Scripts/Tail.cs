using Mirror;
using UnityEngine;

public class Tail : NetworkBehaviour
{
    [SyncVar] public NetworkIdentity ownerIdentity;
}
