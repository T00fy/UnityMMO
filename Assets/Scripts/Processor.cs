using UnityEngine;
using MMOServer;

public abstract class Processor : MonoBehaviour
{

    abstract public void ProcessPacket(BasePacket receivedPacket);
    abstract public void DoAuthenticationChecks(BasePacket receivedPacket, SubPacket subPacket);
}