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
            bnetProtocol.clientToken = Math.Abs(new Random().Next());
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);
        }

        public void Connect()
        {
            IPAddress bnetServerIP = Dns.GetHostAddresses(bnetConInfo["ip"])[0];
            IPEndPoint bnetServerEP = new IPEndPoint(bnetServerIP, Int32.Parse(bnetConInfo["port"]));
            this.bnetSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] receiveBuffer = new byte[255];

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

            Int32 bnetClientTimeOffset = Convert.ToInt32(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMilliseconds / -60000);

            bnetProtocol.setBnetByte(0x00000000);
            bnetProtocol.setBnetByte(0x49583836); // Platform IX86
            bnetProtocol.setBnetByte(0x44534852); // Warcraft III
            bnetProtocol.setBnetByte(0x00000000); // Version  0.0
            bnetProtocol.setBnetByte("koKR");     // Language
            bnetProtocol.setBnetByte(0x00000000);
            bnetProtocol.setBnetByte(bnetClientTimeOffset);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte("KOR", true);
            bnetProtocol.setBnetByte("Korea", true);
            bnetProtocol.send(this.bnetSock, BnetPacketModel.SID_AUTH_INFO);

            try {
                while (true)
                {
                    if (bnetSock.Receive(receiveBuffer) > 0)
                    {
                        this.getHandleMsg(BnetCode.AuthInfoSuccess);
                        break;
                    }
                }
            }
            catch (SocketException e)
            {
                this.getHandleMsg(e.StackTrace);
            }
        }
    }
}
