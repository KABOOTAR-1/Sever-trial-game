using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string msg = _packet.ReadString();
        int id = _packet.ReadInt();

        Debug.Log($"Message from Server {msg}");
        Client.instance.myId = id;
        ClientSend.WelcomeReceived();
    }
}
