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

        public void setBnetByte(String hexData, bool isVariable = false)
        {
            int intData = int.Parse(hexData, System.Globalization.NumberStyles.AllowHexSpecifier);
            this.setBnetByte(intData);
        }

        public void setBnetByte(int intData, bool isVariable = false)
        {
            byte[] bData = BitConverter.GetBytes(intData);
            foreach (byte data in bData)
            {
                bnetData.Add(data);
            }
        }

        public void setBnetString(String strData, bool isVariable = false)
        {
            String hexData = bnetHelper.Acsii2Hex(strData);
            this.setBnetByte(hexData, isVariable);
        }

        public byte[] getBnetPacket(byte[] data)
        {
            return null;
        }
    }
}
