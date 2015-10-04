using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M16Chat_Windows.BnetChatting
{
    public enum BnetChattingColor
    {
        Info = 0x00,
        Error = 0x01,
        Plain = 0x02,
        Whisper = 0x03,
        Tip = 0x04,
        Me = 0x05
    }

    public enum BnetChattingStatus
    {
        Default = 0x00,
        Join = 0x01,
        Leave = 0x02
    }

    public struct BnetChattingRGB
    {
        public byte r;
        public byte g;
        public byte b;

        BnetChattingRGB(byte _r, byte _g, byte _b)
        {
            r = _r;
            g = _g;
            b = _b;
        }
    }
}
