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
        private BnetProtocol bnetProtocol = new BnetProtocol();
        private List<Byte> sockBuffer = new List<Byte>();
        public BnetClient(String bnetConIP, String bnetConPort)
        {
            this.debugMode = true;
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);
        }

        public void Connect()
        {
            IPAddress bnetServerIP = Dns.GetHostAddresses(bnetConInfo["ip"])[0];
            IPEndPoint bnetServerEP = new IPEndPoint(bnetServerIP, Int32.Parse(bnetConInfo["port"]));
            this.bnetSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                this.bnetSock.ReceiveBufferSize = Int32.MaxValue;
                this.bnetSock.ReceiveTimeout = 3000;
                this.getHandleMsg(BnetCode.ConnectionWithServer);
            }
            catch (Exception e)
            {
                this.getHandleMsg(BnetCode.ConnectionSuccess);
            }

            this.bnetSock.Connect(bnetServerEP);
            this.bnetSock.Send(BitConverter.GetBytes(0x01));
            this.getHandleMsg(BnetCode.ConnectionSuccess);

            bnetProtocol.setBnetByte("49583836"); // Platform IX86
            bnetProtocol.setBnetByte("44534852"); // Warcraft III
            bnetProtocol.setBnetByte("00000000"); // Version  0.0
        }
    }
}
