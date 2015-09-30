using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Bnet.BnetConnect;

namespace Bnet
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class BnetClient : BnetHandler
    {
        private static Dictionary<String, String> bnetConInfo;
        private Socket bnetSock;
        public BnetClient(String bnetConIP, String bnetConPort)
        {
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);

            IPAddress bnetServerIP = Dns.GetHostAddresses(bnetConInfo["ip"])[0];
            IPEndPoint bnetServerEP = new IPEndPoint(bnetServerIP, Int32.Parse(bnetConInfo["port"]));
            Socket bClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                bClient.ReceiveBufferSize = Int32.MaxValue;
                bClient.ReceiveTimeout = 3000;
                this.getHandleMsg(BnetCode.ConnectionWithServer);
            }
            catch (Exception e)
            {
                this.getHandleMsg(BnetCode.ConnectionSuccess);
            }

            bClient.Connect(bnetServerEP);

        }

        public Socket Connect()
        {

            return this.bnetSock;
        }
    }
}
