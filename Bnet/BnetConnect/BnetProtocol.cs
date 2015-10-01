using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnet.BnetConnect
{
    public class BnetProtocol
    {
        private List<byte> bnetData = new List<byte>();
        private BnetHelper bnetHelper = BnetHelper.getInstance();
        public int serverToken = 0;
        public int clientToken = 0;

        public BnetProtocol()
        {

        }

        public void setBnetByte(byte[] byteData)
        {
            foreach (byte data in byteData)
            {
                bnetData.Add(data);
            }
        }

        public void setBnetByte(UInt32 intData, bool isVariable = false)
        {
            byte[] bData = BitConverter.GetBytes(intData);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bData);
            foreach (byte data in bData)
            {
                bnetData.Add(data);
            }
        }

        public void setBnetByte(String strData, bool isVariable = false)
        {
            String hexData = bnetHelper.Acsii2Hex(strData);
            if (isVariable)
            {
                byte[] bData = bnetHelper.Hex2Byte(hexData);
                this.setBnetByte(bData);
            }
            else {
                UInt32 intData = UInt32.Parse(hexData, System.Globalization.NumberStyles.AllowHexSpecifier);
                this.setBnetByte(intData, isVariable);
            }
        }

        public void send(System.Net.Sockets.Socket bnetSock)
        {
            bnetSock.Send(bnetData.ToArray());
            bnetData = null;
        }

        public List<byte> getBnetPacket()
        {
            return bnetData;
        }
    }
}
