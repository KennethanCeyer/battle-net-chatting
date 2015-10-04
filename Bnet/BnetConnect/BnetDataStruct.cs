using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bnet.BnetConnect;

namespace Bnet.BnetConnect
{
    public struct BnetFriends
    {
        public BnetUsername name;
        public byte status;
        public byte location;
        public uint product;
        public String locationName;
    }

    public struct BnetUsername
    {
        public String name;
        public String color;
    }
}
