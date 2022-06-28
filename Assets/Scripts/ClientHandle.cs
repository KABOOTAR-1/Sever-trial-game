using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string msg = _packet.ReadString();
        int id = _packet.ReadInt();

        Debug.Log($"Message from Server {msg}");
        Client.instance.myId = id;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

   public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string UserName=packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion Rotation = packet.ReadQuaternions();

        GameManager.instance.SpwanPlayer(id, UserName, position, Rotation);
    }
}
