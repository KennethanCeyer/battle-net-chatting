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
    /**
     *
     * [ Battlet Client v 1.0 ]
     *
     * @ Author         PIGNOSE
     * @ Last Updated   2015. 10. 02
     * @ Version        0.0.2
     * @ BuildTool      Visual Studio 2015 - .Net Framework 4.5
     * @ GitHub         https://github.com/KennethanCeyer/M16-Chatting-For-Windows
     *
     **/
    public class BnetClient : BnetHandler
    {
        public ManualResetEvent connectDone = new ManualResetEvent(false);
        public ManualResetEvent receiveDone = new ManualResetEvent(false);

        public delegate void OnChatLoginedDelegate(BnetUsername user);
        public delegate void OnChatUserDelegate(BnetUsername user, String message);
        public delegate void OnChatErrorDelegate(BnetUsername user, String message);
        public delegate void OnChatInfoDelegate(BnetUsername user, String message);
        public delegate void OnChatJoinDelegate(BnetUsername user);
        public delegate void OnChatLeaveDelegate(BnetUsername user);
        public delegate void OnChatWhisperDelegate(BnetUsername user, String message);
        public delegate void OnChatSockErrorDelegate();
        public delegate void OnChatFriendsUpdateDelegate(BnetFriends[] bnetFriends);
        public delegate void OnChatUserChannelMoveDelegate(BnetUsername user, String channel);

        public static event OnChatLoginedDelegate OnChatLogined;
        public static event OnChatUserDelegate OnChatUser;
        public static event OnChatErrorDelegate OnChatError;
        public static event OnChatInfoDelegate OnChatInfo;
        public static event OnChatJoinDelegate OnChatJoin;
        public static event OnChatLeaveDelegate OnChatLeave;
        public static event OnChatWhisperDelegate OnChatWhisper;
        public static event OnChatSockErrorDelegate OnChatSockError;
        public static event OnChatFriendsUpdateDelegate OnChatFriendsUpdate;
        public static event OnChatUserChannelMoveDelegate OnChatUserChannelMove;

        private const string firstJoinChannel = "ib";
        private static Dictionary<String, String> bnetConInfo;
        private Socket bnetSock;
        private BnetProtocol bnetProtocol = new BnetProtocol();
        private byte[] sockBuffer;
        private String bnetUsrId, bnetUserPw;
        public BnetUsername bnetUserUid;
        public BnetHelper bnetHelper = BnetHelper.getInstance();
        public BnetPacketStream bnetPacketStream = new BnetPacketStream();
        public BnetClient(String bnetConIP, String bnetConPort)
        {
            this.debugMode = true;
            bnetProtocol.clientToken = (uint) Math.Abs(new Random().Next());
            bnetConInfo = new Dictionary<String, String>();
            bnetConInfo.Add("ip", bnetConIP);
            bnetConInfo.Add("port", bnetConPort);
        }

        public void commandFriendsUpdate(Socket bnetSock)
        {
            this.getHandleMsg(BnetCode.Request_FriendList);
            bnetProtocol.send(bnetSock, BnetPacketModel.SID_FRIENDSLIST);
        }

        public BnetUsername getUsername()
        {
            return this.bnetUserUid;
        }

        public void Connect(String userId, String userPw)
        {
            bnetUsrId = userId;
            bnetUserPw = userPw;
            IPAddress bnetServerIP = Dns.GetHostAddresses(bnetConInfo["ip"])[0];
            IPEndPoint bnetServerEP = new IPEndPoint(bnetServerIP, Int32.Parse(bnetConInfo["port"]));
            this.bnetSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.bnetSock.ReceiveTimeout = 3000;
            this.bnetSock.SendTimeout = 3000;
            this.getHandleMsg(BnetCode.ConnectionWithServer);
            this.sockBuffer = new byte[this.bnetSock.ReceiveBufferSize];
            try {
                connectDone.Reset();
                this.bnetSock.BeginConnect(bnetServerEP, new AsyncCallback(OnConnectCallback), this.bnetSock);
                connectDone.WaitOne();
            } catch(Exception e)
            {
                this.getHandleMsg(BnetCode.ConnectionFailed);
                this.getHandleMsg(e.StackTrace);

                throw e;
            }
        }

        public void MusicBot()
        {
            string[] Song =
            {
                "There’s evil that waits inside it’s me",
                "Though we tried our best you seem to have beginners luck",
                "We came so close but we just couldn’t make you one of us",
                "Congratulations are deserved it’s 6 AM you win",
                "We’ll see you here tomorrow night and do it all again",
                "The masks that we wear",
                "pretend they aren’t there",
                "but you can only hide for so long, for so long",
                "Why don’t you",
                "Spend the night then you’ll find",
                "there’s evil that waits inside(4x)",
                "it’s me",
                "There’s evil that waits inside",
                "it’s me"
            };
            uint i = 0;
            while (true)
            {
                bnetProtocol.setBnetByte(Song[i], true);
                bnetProtocol.send(this.bnetSock, BnetPacketModel.SID_CHATCOMMAND);
                Thread.Sleep(5000);
                i = (uint) ((i + 1) % Song.Length);
            }
        }

        public void setChatMessage(String message)
        {
            receiveDone.WaitOne();
            bnetProtocol.setBnetByte(message, true);
            bnetProtocol.send(this.bnetSock, BnetPacketModel.SID_CHATCOMMAND);
        }

        public static void OnConnectError()
        {
            OnChatSockError();
        }

        public void OnConnectCallback(IAsyncResult IAR)
        {
            connectDone.Set();
            receiveDone.Reset();

            Socket tmpSock = (Socket) IAR.AsyncState;
            tmpSock.EndConnect(IAR);
            this.BindREceiveHandler(tmpSock);
            this.bnetSock = tmpSock;

            try
            {
                byte[] triedConnection = { 0x01 };
                tmpSock.Send(triedConnection);
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
            //bnetProtocol.setBnetByte(0x44534852);
            bnetProtocol.setBnetByte(0x4452544c); // DRTL
            bnetProtocol.setBnetByte(0x00000000); // Version  0.0
            bnetProtocol.setBnetByte("koKR");     // Language
            bnetProtocol.setBnetByte(0x00000000);
            bnetProtocol.setBnetByte(bnetClientTimeOffset);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte(0x412);
            bnetProtocol.setBnetByte("KOR", true);
            bnetProtocol.setBnetByte("Korea", true);
            bnetProtocol.send(tmpSock, BnetPacketModel.SID_AUTH_INFO);
        }

        public void BindREceiveHandler(Socket cSock)
        {
            cSock.BeginReceive(sockBuffer, 0, cSock.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(OnReceiveCallback), cSock);
        }

        public void OnReceiveCallback(IAsyncResult IAR)
        {
            try
            {
                receiveDone.Reset();
                Socket recvSock = (Socket)IAR.AsyncState;
                int receiveLen = recvSock.EndReceive(IAR);
                byte[] receiveBuffer = this.sockBuffer;
                receiveDone.Set();

                if (receiveLen > 0)
                {
                    if (receiveBuffer[0] == 0xFF)
                    {
                        int bnetResult;
                        BnetPacketStruct bnetPackSt = bnetProtocol.decapsulize(receiveBuffer);
                        switch (bnetPackSt.packet_id)
                        {
                            case BnetPacketModel.SID_OPTIONALWORK:
                            case BnetPacketModel.SID_EXTRAWORK:
                            case BnetPacketModel.SID_REQUIREDWORK:
                                this.getHandleMsg("미사용 패킷: " + bnetPackSt.packet_id.ToString("X"));
                                break;
                            case BnetPacketModel.SID_NULL:
                                bnetProtocol.send(recvSock, BnetPacketModel.SID_NULL);
                                break;
                            case BnetPacketModel.SID_PING:
                                bnetProtocol.setBnetByte(bnetPackSt.pack_data.ToArray());
                                bnetProtocol.send(recvSock, BnetPacketModel.SID_PING);
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
                                bnetProtocol.send(recvSock, BnetPacketModel.SID_AUTH_CHECK);
                                this.getHandleMsg(BnetCode.ConnectionAUTH_INFO);
                                break;
                            case BnetPacketModel.SID_AUTH_CHECK:
                                bnetResult = BitConverter.ToInt32(bnetPackSt.pack_data.ToArray(), 0);
                                if (bnetResult != 0)
                                {
                                    switch (bnetResult)
                                    {
                                        case 0x201:
                                            this.getHandleMsg(BnetCode.ServerBen);
                                            break;
                                        default:
                                            this.getHandleMsg(BnetCode.UnkownError);
                                            break;
                                    }
                                }
                                else
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
                                    bnetProtocol.send(recvSock, BnetPacketModel.SID_LOGONRESPONSE2);
                                    this.getHandleMsg(BnetCode.ConnectionAUTH_CHECK);
                                }
                                break;
                            case BnetPacketModel.SID_LOGONRESPONSE2:
                                bnetResult = BitConverter.ToInt32(bnetPackSt.pack_data.ToArray(), 0);
                                switch (bnetResult)
                                {
                                    case 0x00:
                                        this.getHandleMsg(BnetCode.LOGONRESP_Success);
                                        bnetProtocol.setBnetByte(0x00);
                                        bnetProtocol.setBnetByte(0x00);
                                        bnetProtocol.send(recvSock, BnetPacketModel.SID_ENTERCHAT);
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
                            case BnetPacketModel.SID_ENTERCHAT:
                                this.getHandleMsg(BnetCode.ENTERCHAT);
                                this.getHandleMsg(Encoding.UTF8.GetString(bnetPackSt.pack_data.ToArray()));
                                this.bnetUserUid = bnetPackSt.getUsername(bnetPackSt.getData(bnetPackSt.pack_data.ToArray()));
                                OnChatLogined(this.bnetUserUid);
                                bnetProtocol.setBnetByte(0x01);
                                bnetProtocol.setBnetByte(firstJoinChannel, true);
                                bnetProtocol.send(recvSock, BnetPacketModel.SID_JOINCHANNEL);
                                this.commandFriendsUpdate(this.bnetSock);
                                //Thread musicBotThread = new Thread(new ThreadStart(MusicBot));
                                //musicBotThread.Start();
                                break;
                            case BnetPacketModel.SID_CHATEVENT:
                                BnetPacketEvent bnetPacketEvent = (BnetPacketEvent)BitConverter.ToUInt32(bnetPackSt.pack_data.ToArray(), 0);
                                uint flags = BitConverter.ToUInt32(bnetPackSt.pack_data.ToArray(), 4);
                                uint ping = BitConverter.ToUInt32(bnetPackSt.pack_data.ToArray(), 8);
                                String message;
                                BnetUsername user = bnetPackSt.getUsername(bnetPackSt.getData(bnetPackSt.pack_data.ToArray(), 24));

                                switch (bnetPacketEvent)
                                {
                                    case BnetPacketEvent.EID_CHANNEL:
                                        String channel = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg("유저 확인:" + user.name);
                                        OnChatInfo(this.bnetUserUid, "님이 " + channel + " 채널에 입장.");
                                        OnChatUserChannelMove(this.bnetUserUid, channel);
                                        break;
                                    case BnetPacketEvent.EID_USERFLAGS:
                                        this.getHandleMsg("유저 확인:" + user.name);
                                        bnetPacketStream.getUserFlags(bnetPackSt);
                                        break;
                                    case BnetPacketEvent.EID_SHOWUSER:
                                        this.getHandleMsg("유저 확인:" + user.name);
                                        break;
                                    case BnetPacketEvent.EID_ERROR:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg(message);
                                        OnChatError(user, message);
                                        break;
                                    case BnetPacketEvent.EID_INFO:
                                    case BnetPacketEvent.EID_BROADCAST:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg(message);
                                        OnChatInfo(user, message);
                                        break;
                                    case BnetPacketEvent.EID_WHISPER:
                                    case BnetPacketEvent.EID_WHISPERSENT:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg(message);
                                        OnChatWhisper(user, message);
                                        break;
                                    case BnetPacketEvent.EID_TALK:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg(message);
                                        OnChatUser(user, message);
                                        break;
                                    case BnetPacketEvent.EID_JOIN:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg("Join: " + user + " : " + message);
                                        OnChatJoin(user);
                                        break;
                                    case BnetPacketEvent.EID_LEAVE:
                                        message = bnetPackSt.getData(bnetPackSt.pack_data.ToArray());
                                        this.getHandleMsg("Leave: " + user + " : " + message);
                                        OnChatLeave(user);
                                        break;
                                    default:
                                        this.getHandleMsg("별도 타입 패킷 [EID]: " + bnetPacketEvent.ToString("X"));
                                        break;
                                }
                                break;
                            case BnetPacketModel.SID_FRIENDSLIST:
                                uint seek = 0;
                                byte cnt = bnetPackSt.pack_data[(int)seek++];
                                BnetFriends[] bnetFriends = new BnetFriends[(int)cnt];
                                this.getHandleMsg(BnetCode.Search_FriendList);
                                this.getHandleMsg("탐색 된 프랜즈 " + cnt.ToString() + " 명");
                                int player;
                                for (player = 0; player < cnt; player++)
                                {
                                    bnetFriends[player].name = bnetPackSt.getUsername(bnetPackSt.getData(bnetPackSt.pack_data.ToArray()));
                                    seek = bnetPackSt.getSeek();
                                    bnetFriends[player].status = bnetPackSt.pack_data[(int)seek++];
                                    bnetFriends[player].location = bnetPackSt.pack_data[(int)seek++];
                                    bnetFriends[player].product = BitConverter.ToUInt32(bnetPackSt.pack_data.ToArray(), (int)seek);
                                    bnetFriends[player].locationName = bnetPackSt.getData(bnetPackSt.pack_data.ToArray(), seek + 4);
                                }

                                try
                                {
                                    OnChatFriendsUpdate(bnetFriends);
                                }
                                catch (NullReferenceException e)
                                {
                                    this.getHandleMsg(e.StackTrace);
                                }
                                break;
                            default:
                                this.getHandleMsg("별도 타입 패킷: " + bnetPackSt.packet_id.ToString("X"));
                                break;
                        }
                    }
                    this.BindREceiveHandler(recvSock);
                }
                else
                {
                    recvSock.Close();
                }
            }
            catch (SocketException e)
            {
                this.getHandleMsg(e.StackTrace);
            }
        }
    }
}
