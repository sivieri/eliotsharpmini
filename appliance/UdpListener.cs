using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Erlang.NET;
using HydraMF;

namespace appliance
{
    /*
     * UdpListener receives packets from the network and integrates the equivalent
     * behavior of the ELIoT UDP driver: provides the adequate feedback to the
     * message type (acks stuff), and adds the message header to the answer from
     * the application level.
     * It does not take care of the content of the message, and it provides
     * the message source to the upper level.
     */
    class UdpListener
    {
        public static readonly int PORT = 4369;
        public static readonly int BUF = 1472;
        public static readonly int MICROWAIT = 100;

        public enum MsgType : byte { Data = 0, DataAck = 1, Tick = 2, Ack = 3 }

        private Thread listenerThread;
        private Socket socket;
        private IMessageParser parser;
        private ushort counter;

        public UdpListener(string address, IMessageParser parser)
        {
            this.parser = parser;
            this.counter = 0;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(address), PORT));
            listenerThread = new Thread(new ThreadStart(ListenForMessages));
            listenerThread.Start();
        }

        private void ListenForMessages()
        {
            byte[] buf = new byte[BUF];
            byte[] input = new byte[BUF - 2];
            byte[] answer = new byte[BUF - 2];
            int answerLen = 0;
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            for (; ; )
            {
                Array.Clear(buf, 0, BUF);
                Array.Clear(answer, 0, BUF - 2);
                answerLen = 0;
                int len = socket.ReceiveFrom(buf, ref endPoint);
                if (len == 0) 
                {
                    Thread.Sleep(MICROWAIT);
                    continue;
                }
                /* Step 1: check the type, answer it if needed */
                ushort type = unchecked((ushort) (BitConverter.ToUInt16(buf, 0) >> 12));
                ushort n = unchecked((ushort) (BitConverter.ToUInt16(buf, 0) & 0x0FFF));
                switch (type)
                {
                    case (byte)MsgType.Ack:
                        /* Not implemented yet */
                        break;
                    case (byte)MsgType.Data:
                        /* Nothing to do */
                        break;
                    case (byte)MsgType.DataAck:
                        /* Send the ack back */
                        SendAck(endPoint, n);
                        break;
                    case (byte)MsgType.Tick:
                        /* Ignoring */
                        break;
                    default:
                        Debug.Print("Unknown UDP message " + type);
                        continue;
                }
                /* Step 2: pass the message to the application layer */
                Array.Copy(buf, 2, input, 0, len - 2);
                parser.ParseMessage(((IPEndPoint)endPoint).Address, input, len - 2, ref answer, ref answerLen);
                /* Step 3: if the application layer needs to answer, do it (appending the expected msg header) */
                if (answerLen > 0)
                {
                    Array.Clear(buf, 0, BUF);
                    type = (ushort)MsgType.Data;
                    ushort hdr = unchecked((ushort) (type << 12 | this.counter++));
                    Array.Copy(BitConverter.GetBytes(hdr), 0, buf, 0, 2);
                    Array.Copy(answer, 0, buf, 2, answerLen);
                    EndPoint endPoint2 = new IPEndPoint(((IPEndPoint)endPoint).Address, PORT);
                    socket.SendTo(buf, answerLen + 2, SocketFlags.None, endPoint2);
                }
            }
        }

        private void SendAck(EndPoint endPoint, ushort n)
        {
            byte[] msg = new byte[3];
            byte[] nbytes = BitConverter.GetBytes(n);
            msg[0] = (byte)MsgType.Ack;
            Array.Copy(nbytes, 0, msg, 1, 2);
            socket.SendTo(msg, endPoint);
        }
    }
}
