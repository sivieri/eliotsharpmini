using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SPOT;
using Erlang.NET;
using HydraMF;

namespace appliance
{
    /*
     * This class takes care of interpreting the Erlang message (which has already
     * been stripped of the ELIoT driver header), which is comprised of two parts:
     * the Erlang message header and the Erlang message payload.
     * If the object has to send an answer back to the caller, the distribution
     * message (along with the Erlang message header) has to be computed correctly.
     */
    class Appliance : IMessageParser
    {
        public static readonly byte DIST_MAGIC_RECV_TAG = 131;
        public static readonly string CODE = "miniapp1";
        public static readonly string NAME = "sm";

        public enum MsgType : byte { SmartMeter = 77, Schedule = 83, ApplianceLocal = 76 }
        public enum HeaderType { Send = 2, RegSend = 6 }

        private IPAddress smartMeter;

        public void ParseMessage(IPAddress source, byte[] msg, int len, ref byte[] answer, ref int answerLen)
        {
            /* Step 1: get the header out */
            OtpInputStream inStream = new OtpInputStream(msg);
            OtpErlangTuple header = (OtpErlangTuple)inStream.read_any();
            OtpErlangLong htype = (OtpErlangLong)header.elementAt(0);
            if (htype.intValue() != (int)HeaderType.RegSend) return;
            OtpErlangAtom dest = (OtpErlangAtom)header.elementAt(1);
            if (!dest.atomValue().Equals(NAME)) return;
            inStream.setPos(inStream.getPos() + 1);
            OtpErlangObject payload = inStream.read_any();
            /* Step 2: extract the payload */
            byte[] tmp = ((OtpErlangBinary)payload).binaryValue();
            byte[] content = new byte[tmp.Length - 1];
            Array.Copy(tmp, 1, content, 0, tmp.Length - 1);
            /* Step 3: react to the content */
            switch (tmp[0])
            {
                case (byte)MsgType.ApplianceLocal:
                    /* I should not be receiving this... */
                    break;
                case (byte)MsgType.Schedule:
                    PrintSchedule(content);
                    break;
                case (byte)MsgType.SmartMeter:
                    if (this.smartMeter == null || !this.smartMeter.Equals(source))
                    {
                        this.smartMeter = source;
                        OtpErlangObject hres = PrepareHeader();
                        OtpErlangObject res = PrepareCode();
                        OtpOutputStream houtStream = new OtpOutputStream(hres, false);
                        OtpOutputStream outStream = new OtpOutputStream(res, false);
                        answerLen = houtStream.length() + outStream.length();
                        houtStream.Seek(0, SeekOrigin.Begin);
                        for (int i = 0; i < houtStream.length(); ++i )
                        {
                            answer[i] = (byte) houtStream.ReadByte();
                        }
                        outStream.Seek(0, SeekOrigin.Begin);
                        for (int i = houtStream.length(); i < answerLen; ++i)
                        {
                            answer[i] = (byte)outStream.ReadByte();
                        }
                    }
                    break;
                default:
                    Debug.Print("Unknown appliance message " + content[0]);
                    break;
            }
            /* Step 4: return the answer (if any) */

            return;
        }

        /*
         * Header: {6, 'sm'}
         */
        private OtpErlangObject PrepareHeader()
        {
            OtpErlangInt code = new OtpErlangInt((int)HeaderType.RegSend);
            OtpErlangAtom dest = new OtpErlangAtom("sm");
            OtpErlangObject[] elements = new OtpErlangObject[] { code, dest };
            OtpErlangTuple res = new OtpErlangTuple(elements);

            return res;
        }

        /*
         * Code: [L|20|20|x]
         * - L: message type
         * - 20: hash code (sha1) as characters
         * - 20: name (padded with char zeros at the beginning)
         * - x: the binary itself
         * Everything as a binary.
         */
        private OtpErlangObject PrepareCode()
        {
            byte[] codebytes = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.miniapp1);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] hashbytes = sha.ComputeHash(codebytes);
            byte[] bres = new byte[22 + CODE.Length + codebytes.Length];
            bres[0] = (byte)MsgType.ApplianceLocal;
            Array.Copy(hashbytes, 0, bres, 1, 20);
            bres[21] = (byte)CODE.Length;
            Array.Copy(UTF8Encoding.UTF8.GetBytes(CODE), 0, bres, 22, CODE.Length);
            Array.Copy(codebytes, 0, bres, 22 + CODE.Length, codebytes.Length);
            OtpErlangBinary res = new OtpErlangBinary(bres);

            return res;
        }

        private void PrintSchedule(byte[] buf)
        {
            Param[] parameters = Param.DecodeParams(buf);
            Debug.Print("Parameters:");
            foreach (Param p in parameters)
            {
                Debug.Print(p.ToString());
            }
            Debug.Print("");
        }
    }
}
