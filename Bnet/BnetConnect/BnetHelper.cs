using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnet.BnetConnect
{
    public class BnetHelper
    {
        private static BnetHelper instance;

        protected BnetHelper()
        {
        }

        public static BnetHelper getInstance()
        {
            if(instance == null)
            {
                instance = new BnetHelper();
            }
            return instance;
        }

        public String Acsii2Hex(String ascii)
        {
            String hex = "";
            foreach (char c in ascii)
            {
                hex += ((int) c).ToString("X");
            }
            return hex;
        }

        public byte [] Hex2Byte(String hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] bytes = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                bytes[i] = (byte)((Hex2Int(hex[i << 1]) << 4) + (Hex2Int(hex[(i << 1) + 1])));
            }

            return bytes;
        }

        public int Hex2Int(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }
    }
}
