using System;
using System.Collections;
using System.Text;
using Microsoft.SPOT;

namespace appliance
{
    /*
     * Parameter: [20|20|1|1]
     * - 20: name (padded with char zeros at the beginning)
     * - 20: type (padded with char zeros at the beginning)
     * - 1: value
     * - 1: fixedValue (0 or 1)
     */
    class Param
    {
        public static readonly int ENCLENGTH = 42;
        public static readonly char[] PADDING = new char[] { '0' };

        private String name;
        private String type;
        private int value;
        private bool fixedValue;

        public static Param[] DecodeParams(byte[] buf)
        {
            byte[] tmp = new byte[ENCLENGTH];
            if (buf.Length % ENCLENGTH != 0) throw new ArgumentException("Buffer does not contain encoded parameters");
            Param[] parameters = new Param[buf.Length / ENCLENGTH];
            for (int i = 0, j = 0; i < buf.Length; i += ENCLENGTH, ++j)
            {
                Array.Copy(buf, i, tmp, 0, ENCLENGTH);
                parameters[j] = Decode(tmp);
            }

            return parameters;
        }

        public static Param Decode(byte[] buf)
        {
            if (buf.Length != ENCLENGTH) throw new ArgumentException("Buffer does not contain an encoded parameter");
            string name = new string(UTF8Encoding.UTF8.GetChars(buf, 0, 20)).TrimStart(PADDING);
            string type = new string(UTF8Encoding.UTF8.GetChars(buf, 20, 20)).TrimStart(PADDING);
            int value = buf[40];
            int f = buf[41];

            return new Param(name, type, value, f == 0 ? false : true);
        }

        public Param(String name, String type, int value, bool fixedValue)
        {
            this.name = name;
            this.type = type;
            this.value = value;
            this.fixedValue = fixedValue;
        }

        public String Name
        {
            get { return name; }
        }

        public String Type
        {
            get { return type; }
        }

        public int Value
        {
            get { return value; }
        }

        public bool FixedValue
        {
            get { return fixedValue; }
        }

        public override string ToString()
        {
            return name + " (" + type + "): " + value + " (" + (fixedValue ? "fixed" : "modifiable") + ")";
        }
    }
}
