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
        private String bnetUsrId, bnetUserPw;
        public BnetClient(String bnetConIP, String bnetConPort)
        {
            this.debugMode = true;
            bnetProtocol.clientToken = (uint) Math.Abs(new Random().Next());
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);
        }

        public void Connect(String userId, String userPw)
        {
            bnetUsrId = userId; bnetUserPw = userPw;
            IPAddress bnetServerIP = Dns.GetHostAddresses(bnetConInfo["ip"])[0];
            IPEndPoint bnetServerEP = new IPEndPoint(bnetServerIP, Int32.Parse(bnetConInfo["port"]));
            this.bnetSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                this.bnetSock.ReceiveBufferSize = Int32.MaxValue;
                this.bnetSock.ReceiveTimeout = 3000;
                this.getHandleMsg(BnetCode.ConnectionWithServer);
            }
            catch (Exception e)
            {
                this.getHandleMsg(BnetCode.ConnectionFailed);
                this.getHandleMsg(e.StackTrace);
            }

            this.bnetSock.Connect(bnetServerEP);
            try
            {
                byte[] triedConnection = { 0x01 };
                this.bnetSock.Send(triedConnection);
            }
            catch (Exception e)
            {
                this.getHandleMsg(BnetCode.ConnectionFailed);
                this.getHandleMsg(e.StackTrace);
            }
            this.getHandleMsg(BnetCode.ConnectionSuccess);

            Int32 bnetClientTimeOffset = Convert.ToInt32(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMilliseconds / -60000);

            bnetProtocol.setBnetByte(0x00000000);
            bnetProtocol.setBnetByte(0x49583836); // Platform IX86
            bnetProtocol.setBnetByte(0x44534852);
            //bnetProtocol.setBnetByte(0x4452544c); // DRTL
            bnetProtocol.setBnetByte(0x00000000); // Version  0.0
            bnetProtocol.setBnetByte("koKR");     // Language
            bnetProtocol.setBnetByte(0x00000000);
            bnetProtocol.setBnetByte(bnetClientTimeOffset);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte("KOR", true);
            bnetProtocol.setBnetByte("Korea", true);
            bnetProtocol.send(this.bnetSock, BnetPacketModel.SID_AUTH_INFO);

            bool bnetConKeep = true;
            BnetPacketStream bnetPacketStream = new BnetPacketStream();
            while (bnetConKeep)
            {
                byte[] receiveBuffer = new byte[1024];
                try {
                    int receiveLen = this.bnetSock.Receive(receiveBuffer);
                    if (receiveLen > 0)
                    {
                        if(receiveBuffer[0] == 0xFF)
                        {
                            int bnetResult;
                            BnetPacketStruct bnetPackSt = bnetProtocol.decapsulize(receiveBuffer);
                            switch(bnetPackSt.packet_id)
                            {
                                case BnetPacketModel.SID_OPTIONALWORK:
                                case BnetPacketModel.SID_EXTRAWORK:
                                case BnetPacketModel.SID_REQUIREDWORK:
                                    break;
                                case BnetPacketModel.SID_NULL:
                                    bnetProtocol.send(bnetSock, BnetPacketModel.SID_NULL);
                                    break;
                                case BnetPacketModel.SID_PING:
                                    bnetProtocol.setBnetByte(bnetPackSt.pack_data.ToArray());
                                    bnetProtocol.send(bnetSock, BnetPacketModel.SID_PING);
                                    this.getHandleMsg(BnetCode.ConnectionPING);
                                    break;
                                case BnetPacketModel.SID_AUTH_INFO:
                                    bnetProtocol.nlsRevision = bnetPacketStream.readDword(bnetPackSt.pack_data.ToArray(), 0);
                                    bnetProtocol.serverToken = bnetPacketStream.readDword(bnetPackSt.pack_data.ToArray(), 4);

                                    bnetProtocol.setBnetByte(bnetProtocol.clientToken);
                                    bnetProtocol.setBnetByte(0x00000000); // EXE Version
                                    bnetProtocol.setBnetByte(0x00000000); // EXE Hash
                                    bnetProtocol.setBnetByte(0x00000001); // Number of CD-Key
                                    bnetProtocol.setBnetByte(0x00000000); // Spawn CD-Key
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte(0x00000000);
                                    bnetProtocol.setBnetByte("war3.exe 03/18/11 20:03:55 471040", true);
                                    bnetProtocol.setBnetByte("Chat", true);
                                    bnetProtocol.send(bnetSock, BnetPacketModel.SID_AUTH_CHECK);
                                    this.getHandleMsg(BnetCode.ConnectionAUTH_INFO);
                                    break;
                                case BnetPacketModel.SID_AUTH_CHECK:
                                    bnetResult = BitConverter.ToInt32(bnetPackSt.pack_data.ToArray(), 0);
                                    if (bnetResult != 0) {
                                        bnetConKeep = false;
                                        switch (bnetResult)
                                        {
                                            case 0x201:
                                                this.getHandleMsg(BnetCode.ServerBen);
                                                break;
                                            default:
                                                this.getHandleMsg(BnetCode.UnkownError);
                                                break;
                                        }
                                    } else
                                    {
                                        byte[] bnetPwHash = bnetProtocol.encriptDobuleHash(bnetUserPw);

                                        bnetProtocol.setBnetByte(bnetProtocol.clientToken);
                                        bnetProtocol.setBnetByte(bnetProtocol.serverToken);
                                        bnetProtocol.setBnetByte(BitConverter.ToUInt32(bnetPwHash, 0));
                                        bnetProtocol.setBnetByte(BitConverter.ToUInt32(bnetPwHash, 4));
                                        bnetProtocol.setBnetByte(BitConverter.ToUInt32(bnetPwHash, 8));
                                        bnetProtocol.setBnetByte(BitConverter.ToUInt32(bnetPwHash, 12));
                                        bnetProtocol.setBnetByte(BitConverter.ToUInt32(bnetPwHash, 16));
                                        bnetProtocol.setBnetByte(bnetUsrId, true);
                                        bnetProtocol.send(bnetSock, BnetPacketModel.SID_LOGONRESPONSE2);
                                    }
                                    break;
                                case BnetPacketModel.SID_LOGONRESPONSE2:
                                    bnetConKeep = false;
                                    bnetResult = BitConverter.ToInt32(bnetPackSt.pack_data.ToArray(), 0);
                                    switch(bnetResult)
                                    {
                                        case 0x00:
                                            this.getHandleMsg(BnetCode.LOGONRESP_Success);
                                            break;
                                        case 0x01:
                                            this.getHandleMsg(BnetCode.LOGONRESP_FaildID);
                                            break;
                                        case 0x02:
                                            this.getHandleMsg(BnetCode.LOGONRESP_FaildPW);
                                            break;
                                        case 0x06:
                                            this.getHandleMsg(BnetCode.LOGONRESP_LockedID);
                                            break;
                                        default:
                                            this.getHandleMsg(BnetCode.UnkownError);
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (SocketException e)
                {
                    this.getHandleMsg(e.StackTrace);
                    break;
                }
            }
        }
    }
}
