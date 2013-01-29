using System;
using Microsoft.SPOT;

namespace appliance
{
    interface IMessageParser
    {
        void parseMessage(byte[] msg, int len, ref byte[] answer, ref int answerLen);
    }
}
