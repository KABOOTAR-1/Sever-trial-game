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
    }

    public void ConnectToSever()
    {
        tcp.Connect();
    }

    public class TCP
    {

        public TcpClient socket;

        private NetworkStream stream;
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
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
        }

        private void ReceiveCallBack(IAsyncResult _result)
        {
            try
            {
                int byelength = stream.EndRead(_result);
                if (byelength <= 0)
                {
                    return;
                }

                byte[] _data = new byte[byelength];
                Array.Copy(receiveBuffer, _data, byelength);
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receivng TCP data: {ex}");
            }
        }
    }

}
