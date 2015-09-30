using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Bnet
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class BnetClient
    {
        private static Dictionary<String, String> bnetConInfo;
        private Socket bnetSock;
        public BnetClient(String bnetConIP, String bnetConPort)
        {
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);
        }

        public Socket Connect()
        {

            return this.bnetSock;
        }
    }
}
