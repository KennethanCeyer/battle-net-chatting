using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnet.BnetConnect
{
    public class BnetPacketStream
    {
        public uint getIndexFromBytes(byte[] data, uint pos = 0)
        {
            List<byte> current = new List<byte>();
            uint i;
            for (i = pos; i < data.Length; i++)
            {
                if (data[i] == 0x00)
                {
                    break;
                }
            }
            return i;
        }

        public int readWord(byte[] data, uint pos)
        {
            int o1, o2;
            o1 = data[pos];
            o2 = data[pos + 1];

            o1 = o1 & 0x000000FF;
            o2 = (o2 << 8) & 0x0000FF00;

            return o1 | o2;
        }

        public uint readDword(byte[] data, uint pos)
        {
            uint o1, o2, o3, o4;
            o1 = data[pos];
            o2 = data[pos + 1];
            o3 = data[pos + 2];
            o4 = data[pos + 3];

            o1 = o1 & 0x000000FF;
            o2 = (o2 << (8 * 1)) & 0x0000FF00;
            o3 = (o3 << (8 * 2)) & 0x00FF0000;
            o4 = (o4 << (8 * 3)) & 0xFF000000;

            return o1 | o2 | o3 | o4;
        }
    }
}
