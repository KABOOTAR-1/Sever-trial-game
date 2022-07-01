using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;   
    public static int dataBufferSize=4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;

    public TCP tcp;
    public UDP udp;

    private bool isConnected=false;

    private delegate void packetHandler(Packet _packet);
    private static Dictionary<int, packetHandler> packetHandlers;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Debug.Log("Instance already exsists, Destroying Current Object");
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }



    public void ConnectToSever()
    {
        InitilizeClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {

        public TcpClient socket; 

        private NetworkStream stream;
        private Packet receievedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
               SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallBack, socket);
        }

        private void ConnectCallBack(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();
            receievedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch(Exception e)
            {
                Debug.Log($"Error sending the data{e}");
            }
        }
        private void ReceiveCallBack(IAsyncResult _result)
        {
            try
            {
                int byelength = stream.EndRead(_result);
                if (byelength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[byelength];
                Array.Copy(receiveBuffer, _data, byelength);

                receievedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
            }
            catch 
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int PacketLength = 0;
            receievedData.SetBytes(_data);

            if (receievedData.UnreadLength() >= 4)
            {
                PacketLength = receievedData.ReadInt();
                if (PacketLength <= 0)
                {
                    return true;
                }
            }
          
            while(PacketLength>0 && PacketLength <= receievedData.UnreadLength())
            {
                byte[] _packetBytes = receievedData.ReadBytes(PacketLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet _packet=new Packet(_packetBytes))
                    {
                        int _packetit= _packet.ReadInt();
                        packetHandlers[_packetit](_packet);

                    }
                });
                PacketLength = 0;

                if (receievedData.UnreadLength() >= 4)
                {
                    PacketLength = receievedData.ReadInt();
                    if (PacketLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (PacketLength <= 1)
                return true;


            return false;   
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receievedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallBack, null);

            using(Packet _packet= new Packet())
            {
                SendData(_packet);  
            }

        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log($" Exeception found{ex}");
            }
        }

        private void ReceiveCallBack(IAsyncResult _result)
        {
            try
            {
                byte[] data = socket.EndReceive(_result,ref endPoint);
                socket.BeginReceive(ReceiveCallBack, null);

                if (data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using(Packet _packet= new Packet(data))
            {
                int _packetlength = _packet.ReadInt();
                data= _packet.ReadBytes(_packetlength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using(Packet _packet= new Packet(data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            }
            );
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }

    private void InitilizeClientData()
    {
        packetHandlers = new Dictionary<int, packetHandler>() 
        {
            {(int)ServerPackets.welcome,ClientHandle.Welcome },
            {(int)ServerPackets.spawnPlayer,ClientHandle.SpawnPlayer },
            {(int)ServerPackets.playerPosition,ClientHandle.PlayerPosition },
            {(int)ServerPackets.playerRotation,ClientHandle.PlayerRotation },
        };

        Debug.Log("Initilized packets"); 
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected");
        }
    }
}
