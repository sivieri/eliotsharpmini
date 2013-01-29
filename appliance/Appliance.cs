using System;
using System.Text;
using Microsoft.SPOT;

namespace appliance
{
    class Appliance : IMessageParser
    {
        public void parseMessage(byte[] msg, int len, ref byte[] answer, ref int answerLen)
        {
            StringBuilder bytes = new StringBuilder();
            for (int i = 0; i < len; ++i)
            {
                bytes.Append((uint) msg[i] + "");
            }
            Debug.Print(bytes.ToString());
        }
    }
}
