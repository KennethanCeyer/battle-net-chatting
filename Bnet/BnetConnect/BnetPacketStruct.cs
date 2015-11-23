using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                pos = (cursor == 0) ? 0 : cursor + 1;
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

        public BnetUsername getUsername(String username)
        {
            String decoding = username, color = null;
            char[] divider = { '|', 'C', 'F', 'F' };
            int step = 0;
            for(int i=0; i<decoding.Length; i++)
            {
                if (step < 4)
                {
                    if (decoding[i] == divider[step])
                    {
                        if(step < 3)
                        {
                            step++;
                        } else
                        {
                            step = i + 1;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                } else
                {
                    decoding = username.Substring(step + 6);
                    color = username.Substring(step, 6);
                }
            }
            BnetUsername bnetUsername = new BnetUsername();
            bnetUsername.name = decoding;
            bnetUsername.color = color;
            return bnetUsername;
        }

        public uint getSeek()
        {
            return cursor + 1;
        }
    }
}
