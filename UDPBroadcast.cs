using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace Network
{
    public class UDPBroadcast
    {
        const string DEFAULT_MULTICAST_GROUP = "229.193.56.222"; // 224.0.1.0 to 239.255.255.255

        private UdpClient udp;
        IAsyncResult ar_ = null;

        public readonly int Port;
        public readonly string MulticastGroup;
        public event Action<IPEndPoint, string> ReceiveData;

        public UDPBroadcast(int port)
            : this (port, DEFAULT_MULTICAST_GROUP)
        { }

        public UDPBroadcast(int port, string multicastGroup)
        {
            Port = port;
            MulticastGroup = multicastGroup;
        }

        public static UdpClient CreateMultiCastUdpClient(int port, string multicastGroup)
        {
            UdpClient client = new UdpClient();
            //client.ExclusiveAddressUse = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, port);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);

            IPAddress multicastaddress = IPAddress.Parse(multicastGroup);
            client.JoinMulticastGroup(multicastaddress);

            return client;
        }

        public void Start()
        {
            Stop();
            udp = CreateMultiCastUdpClient(Port, MulticastGroup);

            startListening();
        }
        public void Stop()
        {
            udp?.Close();
            udp = null;
        }

        private void startListening()
        {
            ar_ = udp.BeginReceive(receive, new object());
        }
        private void receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            ReceiveData?.Invoke(ip, message);
            startListening();
        }
        public void Send(string message)
        {
            Send(Port, message);
        }

        public static void Send(int port, string message)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }
    }
}