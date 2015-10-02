using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

namespace Bnet.BnetConnect
{
    public class BnetProtocol
    {
        private List<byte> bnetData = new List<byte>();
        private BnetHelper bnetHelper = BnetHelper.getInstance();
        public byte bnetCommand;
        public uint serverToken = 0;
        public uint clientToken = 0;
        public uint nlsRevision = 0;

        public BnetProtocol()
        {
        }

        public void setBnetByte(byte byteData)
        {
            bnetData.Add(byteData);
        }

        public void setBnetByte(byte[] byteData)
        {
            bnetData.AddRange(byteData);
        }

        public void setBnetByte(long longData, bool isVariable = false)
        {
            setBnetByte((int)longData);
        }

        public void setBnetByte(Int32 intData, bool isVariable = false)
        {
            byte[] bData = BitConverter.GetBytes(intData);
            bnetData.AddRange(bData);
        }

        public void setBnetByte(UInt32 intData, bool isVariable = false)
        {
            byte[] bData = BitConverter.GetBytes(intData);
            bnetData.AddRange(bData);
        }

        public void setBnetByte(String strData, bool isVariable = false)
        {
            if (isVariable)
            {
                byte[] bData = Encoding.UTF8.GetBytes(strData);
                this.setBnetByte(bData);
                this.setBnetByte((byte) 0x00);
            }
            else {
                String hexData = bnetHelper.Acsii2Hex(strData);
                Int32 intData = Int32.Parse(hexData, System.Globalization.NumberStyles.AllowHexSpecifier);
                this.setBnetByte(intData, isVariable);
            }
        }

        public void send(Socket bnetSock, BnetPacketModel bnetCommand = 0x00)
        {
            if(bnetData.Count > 0 && bnetData[0] != 0xFF)
            {
                bnetData = this.capsulize(bnetData, bnetCommand);
            }

            try {
                bnetSock.Send(bnetData.ToArray());
            }
            catch (SocketException e)
            {
                BnetClient.OnConnectError();
            }
            bnetData.Clear();
        }

        public List<Byte> capsulize(List<byte> data, BnetPacketModel bnetCommand)
        {
            List<Byte> capsule = new List<Byte>();
            capsule.Add(0xFF);
            capsule.Add((byte)bnetCommand);
            capsule.Add((byte)((data.Count + 4) & 0x00FF));
            capsule.Add((byte)(((data.Count + 4) & 0xFF00) >> 8));
            capsule.AddRange(data);
            return capsule;
        }

        public BnetPacketStruct decapsulize(byte[] data)
        {
            BnetPacketStruct bnetPacketSt = new BnetPacketStruct();
            bnetPacketSt.packet_id = (BnetPacketModel) data[1];
            bnetPacketSt.packet_len = (uint) ((data[2] & 0x000000FF) | (data[3] << 8 & 0x0000FF00));

            for (int i=4; i<bnetPacketSt.packet_len; i++)
            {
                bnetPacketSt.pack_data.Add(data[i]);
            }

            return bnetPacketSt;
        }

        public byte[] encriptDobuleHash(String str)
        {
            List<byte> data = new List<byte>(Encoding.ASCII.GetBytes(str.ToLower()));
            data.Add((byte)0x00);
            byte[] clientData = BitConverter.GetBytes(this.clientToken);
            byte[] serverData = BitConverter.GetBytes(this.serverToken);

            byte[] sha1data = BnetHelper.Hash(data.ToArray());

            List<byte> buff = new List<byte>();
            buff.AddRange(clientData);
            buff.AddRange(serverData);
            buff.AddRange(sha1data);

            sha1data = BnetHelper.Hash(buff.ToArray());
            return sha1data.ToArray();
        }

        public List<byte> getBnetPacket()
        {
            return bnetData;
        }
    }
}
