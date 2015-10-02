using System;
using System.Collections.Generic;
using System.Text;

namespace Bnet.BnetConnect
{
    public class BnetPacketStruct
    {
        private uint cursor = 0;
        public BnetPacketModel packet_id;
        public uint packet_len;
        public List<byte> pack_data = new List<byte>();
        public String getData(byte[] data, uint pos = 0)
        {
            if(pos == 0)
            {
                pos = cursor + 1;
            }
            BnetPacketStream bnetPacketStream = new BnetPacketStream();
            cursor = bnetPacketStream.getIndexFromBytes(data, pos);
            byte[] part = new byte[cursor - pos];

            for(uint i=pos, j=0; i < cursor; i++, j++)
            {
                part[j] = data[i];
            }
            return Encoding.UTF8.GetString(part);
        }
    }
}
