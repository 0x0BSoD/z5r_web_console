using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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
    // public static UInt32 m_nCtrFlags;
    // public static bool m_fCtrNotifyEnabled;
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
            Console.WriteLine("Reading keys from controller...");
            FileStream fileStream2 = new FileStream(path, FileMode.Create, FileAccess.Write);
            int num = 0;
            for (int j = 0; j < Program.m_nMaxBanks; j++)
            {
                int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, j);
                if (num2 < 0)
                {
                    Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex (bank num {0}) ({1}).", j, num2);
                    return false;
                }
                int num3 = j * Program.m_nMaxKeys;
                if (num > 0)
                {
                    ZG_CTR_KEY[] array2 = new ZG_CTR_KEY[num];
                    num2 = ZGIntf.ZG_Ctr_ReadKeys(Program.m_hCtr, 0, array2, num, Program.ZgProcessCb, (IntPtr)0, j);
                    if (num2 < 0)
                    {
                        Console.WriteLine("Error ZG_Ctr_ReadKeys (bank num {0}) ({1}).", j, num2);
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
            Console.WriteLine(" done.");
        }
        return true;
    }

    public static bool GetNewList(string fileName, ref List<DoActions.MYKEY> oList)
    {
        if (fileName == "")
        {
            Console.WriteLine("Canceled.");
            return false;
        }
        if (!File.Exists(fileName))
        {
            Console.WriteLine("File not Found");
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
            Console.WriteLine("------------");
            Console.WriteLine("Bank num {0}:", i);
            int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, i);
            if (num2 < 0)
            {
                Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex (Bank number {0}) ({1}).", i, num2);
                return;
            }
            if (num == 0)
            {
                Console.WriteLine("List Empty.");
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
                            Console.WriteLine("Error ZG_Ctr_ReadKeys (Bank number {0}) ({1}).", i, num2);
                            return;
                        }
                    }
                    ZG_CTR_KEY zG_CTR_KEY = array[j % array.Length];
                    if (!zG_CTR_KEY.fErased)
                    {
                        Console.WriteLine("{0} {1}, {2}, access: {3:X2}h.", j, ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity), Event_strs.KeyTypeStrs[(int)zG_CTR_KEY.nType], zG_CTR_KEY.nAccess);
                    }
                }
            }
        }
        Console.WriteLine("Success.");
    }
    
    private static void ShowKeyTopIndex()
    {
        Console.WriteLine("Getting top border of the keys...");
        int num = 0;
        for (int i = 0; i < Program.m_nMaxBanks; i++)
        {
            int num2 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num, i);
            if (num2 < 0)
            {
                Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex ({0}).", num2);
                break;
            }
            Console.WriteLine("Bank {0}: {1}", i, num);
        }
        Console.WriteLine("Done.");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("--show-keys - Show Keys");
        Console.WriteLine("--check-key - Search key by number, -1 last used");
        Console.WriteLine("--border-keys - Top border of the keys...");
        Console.WriteLine("--add-key {bank},{nIdx | -1},{key_num | -1},{key_type | 1 | 2 | 3 },{access_level | FF} - Setup key...");
        Console.WriteLine("--add-key-lsit {path_to_list} - Upload new key list in csv format");
        Console.WriteLine("--save-keys {file_name} - Save keys to file");
        Console.WriteLine("--erase-key {bank},{key_num} - Erase key");
        Console.WriteLine("--erase-key-all - Erase all keys");
        Console.WriteLine("--erase-key-cache - Earse key cache (delete tmp file)");
        Console.WriteLine("--events - Show Events");
    }
    // ENTRY POINT ============================================================
    private static void Main(string[] args)
    {
        string command = "10";
        string pszName = null;

        // Check arguments == 
        if (args.Length < 1)
        {
            Console.WriteLine("Need more argumetns, <command> [address:port] [cmd parameters]");
        }
        else
        {
            command = args[0];
            pszName = args[1];
        }

        // Check SDK version ==
        UInt32 nVersion = ZGIntf.ZG_GetVersion();
        if ((((nVersion & 0xFF)) != ZGIntf.ZG_SDK_VER_MAJOR) || (((nVersion >> 8) & 0xFF) != ZGIntf.ZG_SDK_VER_MINOR))
        {
            Console.WriteLine(Helpers.StringGenerateAnswer("SDK Guard Version wrong", false));
            return;
        }
        else
        {
            IntPtr intPtr = new IntPtr(0);
            int num2 = ZGIntf.ZG_Initialize(1u);
            if (num2 < 0)
            {
                Console.WriteLine(Helpers.StringGenerateAnswer("Error ZG_Initialize.", false));
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
                            Console.WriteLine(Helpers.StringGenerateAnswer("Error ZG_Cvt_Open.", false));
                            return;
                        }
                        else
                        {
                            // Controller ==
                            ZG_CTR_INFO zG_CTR_INFO = default(ZG_CTR_INFO);
                            num2 = ZGIntf.ZG_Ctr_Open(ref Program.m_hCtr, intPtr, 2, 0, ref zG_CTR_INFO, ZG_CTR_TYPE.ZG_CTR_UNDEF);
                            if (num2 < 0)
                            {
                                Console.WriteLine(Helpers.StringGenerateAnswer("Error ZG_Ctr_Open.", false));
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
                                    Console.WriteLine(Helpers.StringGenerateAnswer("Error ZG_Ctr_SetNotification.", false));
                                    return;
                                }
                                else
                                {
                                    NotifyTh.StartNotifyThread();
                                    if (command != "")
                                    {
                                        Console.WriteLine("--show-keys - Show Keys");
                                        Console.WriteLine("--check-key {bank} {key_num | -1} - Search key by number, -1 last used");
                                        Console.WriteLine("--border-keys - Top border of the keys...");
                                        Console.WriteLine("--add-key {bank} {nIdx | -1} {key_num | -1} {key_type | 1 | 2 | 3 } {access_level | FF} - Setup key...");
                                        Console.WriteLine("--add-key-lsit {path_to_list} - Upload new key list in csv format");
                                        Console.WriteLine("--save-keys {file_name} - Save keys to file");
                                        Console.WriteLine("--erase-key {bank} {key_num} - Erase key");
                                        Console.WriteLine("--erase-key-all - Erase all keys");
                                        Console.WriteLine("--erase-key-cache - Earse key cache (delete tmp file)");
                                        Console.WriteLine("--events - Show Events");

                                        string bank = "0";
                                        string key_num = "000,00000";
                                        string nIdsx = "-1";
                                        string key_type = "1";
                                        string access_level = "FF";

                                        switch (command)
                                        {
                                            case "--show-keys":
                                                {
                                                    Program.ShowKeys();
                                                    return;
                                                }
                                            case "--check-key":
                                                {
                                                    bank = args[3];
                                                    key_num = args[4];

                                                    DoActions.DoFindKeyByNumber(bank, key_num);
                                                    break;
                                                }
                                            case "--border-keys":
                                                {
                                                    Program.ShowKeyTopIndex();
                                                    break;
                                                }
                                            case "--add-key":
                                                {

                                                    bank = args[3];
                                                    key_num = args[5];
                                                    // nIdsx = args[4];
                                                    // key_type = args[6];
                                                    // access_level = args[7];

                                                    DoActions.DoSetKey(bank, nIdsx, key_num, key_type, access_level);
                                                    break;
                                                }
                                            case "--add-key-lsit":
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
                                                    bank = args[3];
                                                    key_num = args[4];

                                                    DoActions.DoClearKey(bank, key_num);
                                                    break;
                                                }
                                            case "--erase-key-all":
                                                {
                                                    DoActions.DoClearAllKeys();
                                                    break;
                                                }
                                            case "--erase-key-cache":
                                                {
                                                    DoActions.DoClearKeyCache();
                                                    break;
                                                }
                                            case "--events":
                                                {
                                                    DoActions.DoShowNewEvents();
                                                    break;
                                                }
                                            default:
                                                Console.WriteLine(Helpers.StringGenerateAnswer("Wrong command", false));
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ShowHelp();
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
