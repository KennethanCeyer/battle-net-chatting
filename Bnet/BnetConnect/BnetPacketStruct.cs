using System.Collections.Generic;

namespace Bnet.BnetConnect
{
    public class BnetPacketStruct
    {
        public BnetPacketModel packet_id;
        public uint packet_len;
        public List<byte> pack_data = new List<byte>();
    }
}
