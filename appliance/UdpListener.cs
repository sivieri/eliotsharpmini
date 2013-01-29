using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;

namespace appliance
{
    class UdpListener
    {
        public static readonly int PORT = 4369;
        public static readonly int BUF = 1472;

        private Thread listenerThread;
        private Socket socket;
        private IMessageParser parser;

        public UdpListener(IMessageParser parser)
        {
            this.parser = parser;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            listenerThread = new Thread(new ThreadStart(ListenForMessages));
            listenerThread.Start();
        }

        private void ListenForMessages()
        {
            byte[] buf = new byte[BUF];
            byte[] answer = new byte[BUF];
            int answerLen = 0;
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            for (;;) {
                int len = socket.ReceiveFrom(buf, ref endPoint);
                parser.parseMessage(buf, len, ref answer, ref answerLen);
                if (answerLen > 0) {
                    socket.SendTo(answer, answerLen, SocketFlags.None, endPoint);
                }
            }
        }
    }
}
