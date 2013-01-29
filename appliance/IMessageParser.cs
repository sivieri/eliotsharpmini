using System;
using System.Net;
using Microsoft.SPOT;

namespace appliance
{
    interface IMessageParser
    {
        void ParseMessage(IPAddress source, byte[] msg, int len, ref byte[] answer, ref int answerLen);
    }
}
