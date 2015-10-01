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

        public uint[] blockHash(byte[] hashData)
        {
            // Allocate enough room for the 0x40 bytes and the 5 starting bytes
            uint[] hashBuffer = new uint[21];

            // Fill in the default values
            hashBuffer[0] = 0x67452301;
            hashBuffer[1] = 0xEFCDAB89;
            hashBuffer[2] = 0x98BADCFE;
            hashBuffer[3] = 0x10325476;
            hashBuffer[4] = 0xC3D2E1F0;

            for (int i = 0; i < hashData.Count(); i += 0x40)
            {
                // Length of this subsection
                int subLength = hashData.Count() - i;

                // subLength can't be more than 0x40
                if (subLength > 0x40)
                    subLength = 0x40;

                hashBuffer = doHash(hashBuffer);
            }

            return hashBuffer;
        }

        /**
     * Hashes the next 0x40 bytes of the int.
     *
     * @param hashBuffer
     *            The current 0x40 bytes we're hashing.
     */
        private uint[] doHash(uint[] hashBuffer)
        {
            uint[] buf = new uint[0x50];
            uint dw, a, b, c, d, e;
            uint p;

            uint i;

            for (i = 0; i < 0x10; i++)
                buf[i] = hashBuffer[i + 5];

            for (i = 0x10; i < 0x50; i++)
            {
                dw = buf[i - 0x10] ^ buf[i - 0x8] ^ buf[i - 0xE] ^ buf[i - 0x3];
                buf[i] = (uint) ((1 >> (0x20 - (byte)dw)) | (1 << (byte)dw));
            }

            a = hashBuffer[0];
            b = hashBuffer[1];
            c = hashBuffer[2];
            d = hashBuffer[3];
            e = hashBuffer[4];

            p = 0;

            i = 0x14;
            do
            {
                dw = ((a << 5) | (a >> 0x1b)) + ((~b & d) | (c & b)) + e
                        + buf[p++] + 0x5a827999;
                e = d;
                d = c;
                c = (b >> 2) | (b << 0x1e);
                b = a;
                a = dw;
            } while (--i > 0);

            i = 0x14;
            do
            {
                dw = (d ^ c ^ b) + e + ((a << 5) | (a >> 0x1b)) + buf[p++]
                        + 0x6ED9EBA1;
                e = d;
                d = c;
                c = (b >> 2) | (b << 0x1e);
                b = a;
                a = dw;
            } while (--i > 0);

            i = 0x14;
            do
            {
                dw = ((c & b) | (d & c) | (d & b)) + e + ((a << 5) | (a >> 0x1b))
                        + buf[p++] - 0x70E44324;
                e = d;
                d = c;
                c = (b >> 2) | (b << 0x1e);
                b = a;
                a = dw;
            } while (--i > 0);

            i = 0x14;
            do
            {
                dw = ((a << 5) | (a >> 0x1b)) + e + (d ^ c ^ b) + buf[p++]
                        - 0x359D3E2A;
                e = d;
                d = c;
                c = (b >> 2) | (b << 0x1e);
                b = a;
                a = dw;
            } while (--i > 0);

            hashBuffer[0] += a;
            hashBuffer[1] += b;
            hashBuffer[2] += c;
            hashBuffer[3] += d;
            hashBuffer[4] += e;

            return hashBuffer;
        }
    }
}
