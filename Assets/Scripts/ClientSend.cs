using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    // Start is called before the first frame update
  private static void TCPSendData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    #region packet
    public static void WelcomeReceived()
    {
        using(Packet _packet=new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.userNameField.text);
            TCPSendData(_packet);
        }
    }
    #endregion


}
