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
        public BnetProtocol()
        {

        }

        public void setBnetByte(String hexData)
        {
            uint intData = uint.Parse(hexData, System.Globalization.NumberStyles.AllowHexSpecifier);
            byte[] bData = BitConverter.GetBytes(intData);
            foreach(byte data in bData) {
                bnetData.Add(data);
            }
        }

        public byte[] getBnetPacket(byte[] data)
        {
            return null;
        }
    }
}
