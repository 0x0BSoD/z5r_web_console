using System;
using System.Runtime.InteropServices;
using System.Threading;
using ZGuard;

class NotifyTh
{
    private static bool m_fThreadActive;
    public static ManualResetEvent m_oEvent = null;
    private static Thread m_oThread = null;

    public static int CheckNotifyMsgs()
    {
        uint num = 0u;
        IntPtr zero = IntPtr.Zero;
        int num2;
        while ((num2 = ZGIntf.ZG_Ctr_GetNextMessage(Program.m_hCtr, ref num, ref zero)) == ZGIntf.S_OK)
        {
            uint num3 = num;
            if (num3 == 3)
            {
                ZG_N_KEY_TOP_INFO zG_N_KEY_TOP_INFO = (ZG_N_KEY_TOP_INFO)Marshal.PtrToStructure(zero, typeof(ZG_N_KEY_TOP_INFO));
                string text = "{ \"Bank\":" + zG_N_KEY_TOP_INFO.nBankN + "," +
                              "\"Action\": \"top key border changed\"," +
                              "\"Border\": [" + zG_N_KEY_TOP_INFO.nOldTopIdx + ", " + zG_N_KEY_TOP_INFO.nNewTopIdx + "] }";
                // Console.WriteLine(Helpers.StringGenerateAnswer(text, true));
            }
        }
        if (num2 == 262658)
        {
            num2 = ZGIntf.S_OK;
        }
        return num2;
    }

    public static void DoNotifyWork()
    {
        while (m_fThreadActive)
        {
            if (m_oEvent.WaitOne())
            {
                m_oEvent.Reset();
                if (Program.m_hCtr != IntPtr.Zero)
                {
                    CheckNotifyMsgs();
                }
            }
        }
    }

    public static void StartNotifyThread()
    {
        if (m_oThread == null)
        {
            m_fThreadActive = true;
            m_oThread = new Thread(DoNotifyWork);
            m_oThread.Start();
        }
    }

    public static void StopNotifyThread()
    {
        if (m_oThread != null)
        {
            m_fThreadActive = false;
            m_oEvent.Set();
            m_oThread.Join();
            m_oThread = null;
        }
    }
}
