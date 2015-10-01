using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnet.BnetConnect
{
    public class BnetHandler
    {
        protected bool debugMode = false;
        private const String prefix = "BnetClient";

        public enum BnetCode
        {
            ConnectionWithServer,
            ConnectionFailed,
            ConnectionSuccess,
            AuthInfoSuccess,
            ServerBen,
            UnkownError,
            LOGONRESP_Success,
            LOGONRESP_FaildID,
            LOGONRESP_FaildPW,
            LOGONRESP_LockedID
        };

        private String[] BnetMsg =
        {
            "서버와의 연결을 시도하고 있습니다.",
            "서버와의 연결에 실패하였습니다.",
            "연결이 정상적으로 완료되었습니다.",
            "연결정보 송신이 정상적으로 완료되었습니다.",
            "사용자의 IP는 서버에서 벤 당하셨습니다.",
            "알 수 없는에러, 관리자에게 알려주시기 바랍니다.",
            "LOGONRESP2 프로세스: 연결완료.",
            "LOGONRESP2 프로세스: 연결실패_아이디문제.",
            "LOGONRESP2 프로세스: 연결실패_패스워드문제.",
            "LOGONRESP2 프로세스: 연결실패_아이디락."
        };

        public BnetHandler()
        {
        }

        public int getHandleMsg(BnetCode code)
        {
            if(this.debugMode == true)
            {
                Debug.WriteLine(prefix + " : " + this.BnetMsg[(int) code]);
            }
            return (int) code;
        }

        public String getHandleMsg(String msg)
        {
            if (this.debugMode == true)
            {
                Debug.WriteLine(prefix + " : " + msg);
            }
            return msg;
        }
    }
}
