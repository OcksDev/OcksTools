using Unity.Collections;
using Unity.Netcode;

public class IDSync : NetworkBehaviour
{
    [ShowFixedBetter]
    public FixedString64Bytes MyID = "-";
    public void AssignMyID(FixedString64Bytes id)
    {
        MyID = id;
    }
}
