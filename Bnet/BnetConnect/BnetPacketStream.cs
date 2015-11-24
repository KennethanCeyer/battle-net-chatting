using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnet.BnetConnect
{
    public class BnetPacketStream
    {
        private uint userFlagSeek = 28;
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

        public List<BnetUsername> getUserFlags(BnetPacketStruct bnetPackSt, byte[] originPack)
        {
            uint totalLen = (uint)bnetPackSt.pack_data.Count + 4;
            BnetProtocol bnetProtocol = new BnetProtocol();
            BnetPacketStruct curPackSt = new BnetPacketStruct();
            List<string> bnetUserList = new List<string>();
            List<BnetUsername> bnetUsernameList = new List<BnetUsername>();

            for(uint i = totalLen; i < originPack.Length;)
            {
                BnetDataPack formatedData = this.readFlagHead(originPack, i);
                if(formatedData.data == null)
                {
                    break;
                } else
                {
                    bnetUserList.Add(Encoding.UTF8.GetString(formatedData.data));
                    i += formatedData.len;
                }
            }

            BnetPacketStruct bnetPacketSt = new BnetPacketStruct();
            for (int i = 0; i < bnetUserList.Count; i++)
            {
                BnetUsername bnetUsername = bnetPacketSt.getUsername(bnetUserList[i]);
                bnetUsernameList.Add(bnetUsername);
            }
            return bnetUsernameList;
        }

        public BnetDataPack readFlagHead(byte[] data, uint pos)
        {
            BnetDataPack bnetDataPack = new BnetDataPack();
            List<byte> lData = data.ToList().Skip((int)pos).ToList();
            byte[] lHead = lData.Take(4).ToArray();
            byte[] rHead = lData.Skip(4).Take(4).ToArray();

            if(lHead[0] == 0xFF && lHead[1] == (uint)BnetPacketModel.SID_CHATEVENT)
            {
                byte[] iHead = lData.Skip(12).Take(4).ToArray();
                uint end = getIndexFromBytes(lData.Skip((int)userFlagSeek + 1).ToArray());
                uint len = lHead[2];
                bnetDataPack.data = lData.Skip((int) userFlagSeek).Take((int) (end + 1)).ToArray();
                bnetDataPack.len = len;
            }
            return bnetDataPack;

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
