using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Newtonsoft.Json;
using ZGuard;
using ZPort;


internal class Program
{
    public const Byte CtrAddr = 2;
    public static IntPtr m_hCtr = IntPtr.Zero;
    public static bool m_fProximity;
    public static int m_nFoundKeyIdx;
    public static Byte[] m_rFindNum;
    public const string KeysCacheFileName_D = "ctr{0}_keys.bin";
    public static int m_nSn;
    public static int m_nMaxBanks;
    public static int m_nMaxKeys;
    public static int m_nOptRead;
    public static int m_nOptWrite;
    public static int m_nCtrMaxEvents;
    public static int m_nAppReadEventIdx;

    private static bool ZgProcessCb(int nPos, int nMax, IntPtr pUserData)
    {
        Console.Write("\r{0,5} / {1}", nPos, nMax);
        return true;
    }

    public static bool GetCtrList(ref ZG_CTR_KEY[] aList)
    {
        aList = new ZG_CTR_KEY[Program.m_nMaxKeys * Program.m_nMaxBanks];
        string path = $"ctr{Program.m_nSn}_keys.bin";
        if (File.Exists(path))
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[Marshal.SizeOf((object)aList[0])];
            object obj = default(ZG_CTR_KEY);
            for (int i = 0; i < aList.Length; i++)
            {
                fileStream.Read(array, 0, array.Length);
                Helpers.ByteArrayToStructure(array, ref obj);
                aList[i] = (ZG_CTR_KEY)obj;
            }
            fileStream.Close();
        }
        else
        {
            FileStream fileStream2 = new FileStream(path, FileMode.Create, FileAccess.Write);
            int num = 0;
            for (int j = 0; j < Program.m_nMaxBanks; j++)
            {
                int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, j);
                if (num2 < 0)
                {
                    Helpers.StringGenerateAnswer("Error ZG_Ctr_GetKeyTopIndex", false);
                    return false;
                }
                int num3 = j * Program.m_nMaxKeys;
                if (num > 0)
                {
                    ZG_CTR_KEY[] array2 = new ZG_CTR_KEY[num];
                    num2 = ZGIntf.ZG_Ctr_ReadKeys(Program.m_hCtr, 0, array2, num, Program.ZgProcessCb, (IntPtr)0, j);
                    if (num2 < 0)
                    {
                        Helpers.StringGenerateAnswer("Error ZG_Ctr_ReadKeys", false);
                        return false;
                    }
                    array2.CopyTo(aList, num3);
                    for (int k = 0; k < num; k++)
                    {
                        byte[] array3 = Helpers.StructureToByteArray(array2[k]);
                        fileStream2.Write(array3, 0, array3.Length);
                    }
                }
                if (num < Program.m_nMaxKeys)
                {
                    ZG_CTR_KEY zG_CTR_KEY = default(ZG_CTR_KEY);
                    zG_CTR_KEY.fErased = true;
                    byte[] array3 = Helpers.StructureToByteArray(zG_CTR_KEY);
                    for (int l = num; l < Program.m_nMaxKeys; l++)
                    {
                        fileStream2.Write(array3, 0, array3.Length);
                        aList[num3 + l] = zG_CTR_KEY;
                    }
                }
            }
            fileStream2.Close();
        }
        return true;
    }

    public static bool GetNewList(string fileName, ref List<DoActions.MYKEY> oList)
    {
        if (fileName == "")
        {
            return false;
        }
        if (!File.Exists(fileName))
        {
            Console.WriteLine("File not Found");
            Helpers.StringGenerateAnswer(fileName + " not Found", false);
            return false;
        }
        string path_to_file = Path.GetFullPath(fileName);
        StreamReader streamReader = new StreamReader(path_to_file);
        string text2;
        while ((text2 = streamReader.ReadLine()) != null)
        {
            string[] array = text2.Split(';');
            if (array.Length != 0)
            {
                DoActions.MYKEY mYKEY = default(DoActions.MYKEY);
                mYKEY.m_Num = new byte[16];
                if (Helpers.ParseKeyNum(ref mYKEY.m_Num, array[1]))
                {
                    mYKEY.m_nType = ZG_CTR_KEY_TYPE.ZG_KEY_NORMAL;
                    mYKEY.m_nAccess = 255u;
                    if (array.Length >= 1)
                    {
                        array[1] = array[1].Trim().ToUpper();
                        if (array[1] == "B")
                        {
                            mYKEY.m_nType = ZG_CTR_KEY_TYPE.ZG_KEY_BLOCKING;
                        }
                        else if (array[1] == "M")
                        {
                            mYKEY.m_nType = ZG_CTR_KEY_TYPE.ZG_KEY_MASTER;
                        }
                        int num = default(int);
                        if (array.Length >= 2 && int.TryParse(array[2].Trim(), NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, out num))
                        {
                            mYKEY.m_nAccess = (byte)num;
                        }
                    }
                    oList.Add(mYKEY);
                }
            }
        }
        streamReader.Close();
        return true;
    }

    private static void ShowKeys()
    {
        int num = 0;
        ZG_CTR_KEY[] array = new ZG_CTR_KEY[6];
        for (int i = 0; i < Program.m_nMaxBanks; i++)
        {

            List<string> keys = new List<string>();

            int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, i);
            if (num2 < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Ctr_GetKeyTopIndex", false);
                return;
            }
            if (num == 0)
            {
                Helpers.StringGenerateAnswer("List Empty", true);
            }
            else
            {
                for (int j = 0; j < num; j++)
                {
                    if (j % array.Length == 0)
                    {
                        int num3 = num - j;
                        if (num3 > array.Length)
                        {
                            num3 = array.Length;
                        }
                        num2 = ZGIntf.ZG_Ctr_ReadKeys(Program.m_hCtr, j, array, num3, null, IntPtr.Zero, i);
                        if (num2 < 0)
                        {
                            Helpers.StringGenerateAnswer("Error ZG_Ctr_ReadKeys", false);
                            return;
                        }
                    }
                    ZG_CTR_KEY zG_CTR_KEY = array[j % array.Length];



                    if (!zG_CTR_KEY.fErased)
                    {
                        keys.Add(ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity));
                    }
                }
            }
            var result = new
            {
                Bank = i,
                keys = keys
            };
            Helpers.StringGenerateAnswer(result, true);
        }
    }

    private static void ShowKeyTopIndex()
    {
        int num = 0;
        List<Object> result = new List<object>();
        for (int i = 0; i < Program.m_nMaxBanks; i++)
        {
            int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, i);
            if (num2 < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Ctr_GetKeyTopIndex", false);
                break;
            }
            var bank_data = new
            {
                Bank = i,
                top_index = num
            };
            result.Add(bank_data);
        }
        Helpers.StringGenerateAnswer(result, true);


    }

    private static void ShowHelp()
    {
        Console.WriteLine("= Help ============================");

        Console.WriteLine("--show-keys       [address:port] - Show Keys");
        Console.WriteLine("--border-keys     [address:port] - Top border of the keys...");
        Console.WriteLine("--erase-keys-all   [address:port] - Erase all keys");
        Console.WriteLine("--events          [address:port] - Show Events");
        Console.WriteLine("--erase-key-cache - Earse key cache (delete tmp file)");

        Console.WriteLine("=============================");

        Console.WriteLine("--add-key       [address:port] {key_number}   - Setup key");
        Console.WriteLine("--check-key     [address:port] {key_number}   - Search key by number, -1 last used");
        Console.WriteLine("--add-keys-list [address:port] {path_to_list} - Upload new key list in csv format");
        Console.WriteLine("--save-keys     [address:port] {file_name}    - Save keys to file");
        Console.WriteLine("--erase-key     [address:port] {key_index}    - Erase key by index");
    }
    // ENTRY POINT ============================================================
    private static void Main(string[] args)
    {
        Console.WriteLine();
        string command = "--help";
        string pszName = null;
        // Check arguments ==
        if (args.Length < 1)
        {
            Console.WriteLine("Need more argumetns, <command> [address:port] [cmd parameters]");
        }
        else if (args.Length < 2)
        {
            command = args[0];
        }
        else
        {
            pszName = args[1];
            command = args[0];
        }

        // Check SDK version ==
        UInt32 nVersion = ZGIntf.ZG_GetVersion();
        if ((((nVersion & 0xFF)) != ZGIntf.ZG_SDK_VER_MAJOR) || (((nVersion >> 8) & 0xFF) != ZGIntf.ZG_SDK_VER_MINOR))
        {
            Helpers.StringGenerateAnswer("SDK Guard Version wrong", false);
            return;
        }
        else
        {
            IntPtr intPtr = new IntPtr(0);
            int num2 = ZGIntf.ZG_Initialize(1u);
            if (num2 < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Initialize.", false);
                return;
            }
            else
            {
                // All Ok - try connect to converter and controller ==
                try
                {
                    if (pszName != null)
                    {
                        ZG_CVT_INFO pInfo = new ZG_CVT_INFO();
                        ZG_CVT_OPEN_PARAMS zG_CVT_OPEN_PARAMS = default(ZG_CVT_OPEN_PARAMS);
                        zG_CVT_OPEN_PARAMS.nPortType = ZP_PORT_TYPE.ZP_PORT_IP;
                        zG_CVT_OPEN_PARAMS.pszName = pszName;
                        zG_CVT_OPEN_PARAMS.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;

                        // Converter ==
                        num2 = ZGIntf.ZG_Cvt_Open(ref intPtr, ref zG_CVT_OPEN_PARAMS, pInfo);
                        if (num2 < 0)
                        {
                            Helpers.StringGenerateAnswer("Error ZG_Cvt_Open.", false);
                            return;
                        }
                        else
                        {
                            // Controller ==
                            ZG_CTR_INFO zG_CTR_INFO = default(ZG_CTR_INFO);
                            num2 = ZGIntf.ZG_Ctr_Open(ref Program.m_hCtr, intPtr, 2, 0, ref zG_CTR_INFO, ZG_CTR_TYPE.ZG_CTR_UNDEF);
                            if (num2 < 0)
                            {
                                Helpers.StringGenerateAnswer("Error ZG_Ctr_Open.", false);
                                return;
                            }
                            else
                            {
                                // All OK - set controller vars ==
                                Program.m_nCtrMaxEvents = zG_CTR_INFO.nMaxEvents;
                                Program.m_nSn = zG_CTR_INFO.nSn;
                                Program.m_fProximity = ((zG_CTR_INFO.nFlags & 2) != 0);
                                Program.m_nMaxBanks = (((zG_CTR_INFO.nFlags & 1) == 0) ? 1 : 2);
                                Program.m_nMaxKeys = zG_CTR_INFO.nMaxKeys;
                                Program.m_nOptRead = zG_CTR_INFO.nOptReadItems;
                                Program.m_nOptWrite = zG_CTR_INFO.nOptWriteItems;
                                NotifyTh.m_oEvent = new ManualResetEvent(false);

                                // Show notify in real-time ==
                                ZG_CTR_NOTIFY_SETTINGS pSettings = new ZG_CTR_NOTIFY_SETTINGS(4u, NotifyTh.m_oEvent.SafeWaitHandle, IntPtr.Zero, 0u, 0, 3000u, 0u);
                                num2 = ZGIntf.ZG_Ctr_SetNotification(Program.m_hCtr, pSettings);
                                if (num2 < 0)
                                {
                                    Helpers.StringGenerateAnswer("Error ZG_Ctr_SetNotification.", false);
                                    return;
                                }
                                else
                                {
                                    NotifyTh.StartNotifyThread();
                                    if (command != "")
                                    {
                                        string key_num = "000,00000";
                                        switch (command)
                                        {
                                            case "--show-keys":
                                                {
                                                    Program.ShowKeys();
                                                    return;
                                                }
                                            case "--check-key":
                                                {
                                                    key_num = args[2];
                                                    DoActions.DoFindKeyByNumber(key_num);
                                                    break;
                                                }
                                            case "--border-keys":
                                                {
                                                    Program.ShowKeyTopIndex();
                                                    break;
                                                }
                                            case "--add-key":
                                                {
                                                    key_num = args[2];
                                                    DoActions.DoSetKey(key_num);
                                                    break;
                                                }
                                            case "--add-keys-list":
                                                {
                                                    string keysList = args[2];
                                                    DoActions.DoLoadKeysFromFile(keysList);
                                                    break;
                                                }
                                            case "--save-keys":
                                                {
                                                    DoActions.DoSaveKeysToFile();
                                                    break;
                                                }
                                            case "--erase-key":
                                                {
                                                    key_num = args[2];
                                                    DoActions.DoClearKey(key_num);
                                                    break;
                                                }
                                            case "--erase-keys-all":
                                                {
                                                    DoActions.DoClearAllKeys();
                                                    break;
                                                }
                                            case "--events":
                                                {
                                                    DoActions.DoShowNewEvents();
                                                    break;
                                                }
                                            default:
                                                Helpers.StringGenerateAnswer("Wrong command", false);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (command != "")
                        {
                            switch (command)
                            {
                                case "--erase-key-cache":
                                    {
                                        DoActions.DoClearKeyCache();
                                        break;
                                    }
                                case "--help":
                                    {
                                        ShowHelp();
                                        break;
                                    }
                                default:
                                    Helpers.StringGenerateAnswer("Wrong command, try --help ", false);
                                    break;
                            }
                        }
                        else
                        {
                            ShowHelp();
                        }
                    }
                  }

                    finally
                    {
                        NotifyTh.StopNotifyThread();
                        if (Program.m_hCtr != IntPtr.Zero)
                        {
                            ZGIntf.ZG_CloseHandle(Program.m_hCtr);
                        }
                        if (intPtr != IntPtr.Zero)
                        {
                            ZGIntf.ZG_CloseHandle(intPtr);
                        }
                        ZGIntf.ZG_Finalyze();
                    }
                }
            }
        }
}
