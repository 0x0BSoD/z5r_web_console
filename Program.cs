// CtrKeys.Program
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
    // VARS ANS STRUCTS =============================================
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MYKEY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] m_Num;

        public ZG_CTR_KEY_TYPE m_nType;

        public uint m_nAccess;
    }

    public class MyKeysComparer : IComparer<MYKEY>
    {
        public int Compare(MYKEY x, MYKEY y)
        {
            return Program.CompareZKeyNums(x.m_Num, y.m_Num);
        }
    }

    public static readonly string[] CtrTypeStrs = new string[10]
    {
        "",
        "Gate 2000",
        "Matrix II Net",
        "Matrix III Net",
        "Z5R Net",
        "Z5R Net 8000",
        "Guard Net",
        "Z-9 EHT Net",
        "EuroLock EHT net",
        "Z5R Web"
    };

    public static readonly string[] KeyModeStrs = new string[2]
    {
        "Touch Memory",
        "Proximity"
    };

    public static readonly string[] KeyTypeStrs = new string[4]
    {
        "",
        "Basic",
        "Blocking",
        "Master"
    };

    public static readonly string[] KeyTypeAbbrs = new string[4]
    {
        "",
        "N",
        "B",
        "M"
    };

    public static readonly string[] EvTypeStrs =
    {
            "",
            "Открыто кнопкой изнутри",
            "Ключ не найден в банке ключей",
            "Ключ найден, дверь открыта",
            "Ключ найден, доступ не разрешен",
            "Открыто оператором по сети",
            "Ключ найден, дверь заблокирована",
            "Попытка открыть заблокированную дверь кнопкой",
            "Дверь взломана",
            "Дверь оставлена открытой (timeout)",
            "Проход состоялся",
            "Сработал датчик 1",
            "Сработал датчик 2",
            "Перезагрузка контроллера",
            "Заблокирована кнопка открывания",
            "Попытка двойного прохода",
            "Дверь открыта штатно",
            "Дверь закрыта",
            "Пропало питание",
            "Включение электропитания",
            "Включение электропитания",
            "Включение замка (триггер)",
            "Отключение замка (триггер)",
            "Изменение состояния Режим",
            "Изменение состояния Пожара",
            "Изменение состояния Охраны",
            "Неизвестный ключ",
            "Совершен вход в шлюз",
            "Заблокирован вход в шлюз (занят)",
            "Разрешен вход в шлюз",
            "Заблокирован проход (Антипассбек)",
            "Hotel40",
            "Hotel41"
    };

    public static readonly string[] DirectStrs =
    {
            "",
            "Вход",
            "Выход"
    };

    public static readonly string[] EcSubEvStrs =
    {
            "",
            "Поднесена карта для входа",
            "(зарезервировано)",
            "Включено командой по сети",
            "Выключено командой по сети",
            "Включено по временной зоне",
            "Выключено по временной зоне",
            "Поднесена карта к контрольному устройству",
            "(зарезервировано)",
            "Выключено после отработки таймаута",
            "Выключено по срабатыванию датчика выхода"
    };

    public static readonly string[] FireSubEvStrs =
    {
            "",
            "Выключено по сети",
            "Включено по сети",
            "Выключено по входу FIRE",
            "Включено по входу FIRE",
            "Выключено по датчику температуры",
            "Включено по датчику температуры"
    };

    public static readonly string[] SecurSubEvStrs =
    {
            "",
            "Выключено по сети",
            "Включено по сети",
            "Выключено по входу ALARM",
            "Включено по входу ALARM",
            "Выключено по тамперу",
            "Включено по тамперу",
            "Выключено по датчику двери",
            "Включено по датчику двери"
    };

    public static readonly string[] ModeSubEvStrs =
    {
            "",
            "Установка командой по сети",
            "Отказано оператору по сети",
            "Началась временная зона",
            "Окончилась временная зона",
            "Установка картой",
            "Отказано изменению картой"
        };
    public static readonly string[] ModeStrs =
    {
            "",
            "Обычный",
            "Блокировка",
            "Свободный",
            "Ожидание"
    };

    public static readonly string[] HModeStrs =
    {
            "",
            "Обычный",
            "Блокировка",
            "Свободный",
            "???"
    };

    public static readonly string[] HotelSubEvStrs =
    {
            "",
            "Карта открытия",
            "Карта блокирующая",
            "Дополнительная функция",
            "создана резервная карта",
            "Network",
            "TimeZone",
            "обновлен счетчик",
            "обновлен криптоключ",
            "Pulse Z",
            "Изменено состояние"
    };

    public const byte CtrAddr = 2;

    public static IntPtr m_hCtr = IntPtr.Zero;

    public static bool m_fProximity;

    public static int m_nFoundKeyIdx;

    public static byte[] m_rFindNum;

    public const string KeysCacheFileName_D = "ctr{0}_keys.bin";

    public static int m_nSn;

    public static int m_nMaxBanks;

    public static int m_nMaxKeys;

    public static int m_nOptRead;

    public static int m_nOptWrite;

    private static ManualResetEvent m_oEvent = null;

    private static bool m_fThreadActive;

    private static Thread m_oThread = null;

    public static int m_nCtrMaxEvents;

    public static UInt32 m_nCtrFlags;

    public static bool m_fCtrNotifyEnabled;

    public static int m_nAppReadEventIdx;

    // HELPERS =======================================================
    private static bool ParseKeyNum(ref byte[] rKeyNum, string sText)
    {
        int num = sText.IndexOf(',');
        if (num != -1)
        {
            string[] array = sText.Split(new char[3]
            {
                ',',
                '[',
                ']'
            }, StringSplitOptions.RemoveEmptyEntries);
            int num2 = int.Parse(array[1]);
        }
        string[] array2 = sText.Split(',');
        if (array2.Length == 2)
        {
            byte b = Convert.ToByte(array2[0]);
            ushort num3 = Convert.ToUInt16(array2[1]);
            rKeyNum[0] = 3;
            rKeyNum[1] = (byte)num3;
            rKeyNum[2] = (byte)(num3 >> 8);
            rKeyNum[3] = b;
            num = sText.IndexOf('[');
            if (num != -1)
            {
                int num2 = sText.IndexOf(']', num + 1);
                int num4 = default(int);
                if (num2 != -1 && int.TryParse(sText.Substring(num + 1, num2 - num - 1), NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, out num4))
                {
                    rKeyNum[4] = (byte)num4;
                    rKeyNum[5] = (byte)(num4 >> 8);
                    rKeyNum[0] = 5;
                }
            }
        }
        else
        {
            int num5 = 1;
            for (int num6 = sText.Length - 2; num6 >= 0; num6 -= 2)
            {
                rKeyNum[num5] = byte.Parse(string.Concat(sText[num6], sText[num6 + 1]), NumberStyles.HexNumber);
                if (++num5 > 6)
                {
                    break;
                }
            }
            rKeyNum[0] = (byte)(num5 - 1);
        }
        return true;
    }

    private static byte[] StructureToByteArray(object obj)
    {
        int num = Marshal.SizeOf(obj);
        byte[] array = new byte[num];
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.StructureToPtr(obj, intPtr, true);
        Marshal.Copy(intPtr, array, 0, num);
        Marshal.FreeHGlobal(intPtr);
        return array;
    }

    private static void ByteArrayToStructure(byte[] bytearray, ref object obj)
    {
        int num = Marshal.SizeOf(obj);
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.Copy(bytearray, 0, intPtr, num);
        obj = Marshal.PtrToStructure(intPtr, obj.GetType());
        Marshal.FreeHGlobal(intPtr);
    }

    private static void DoClearKeyCache()
    {
        File.Delete($"ctr{Program.m_nSn}_keys.bin");
        Console.WriteLine("File delete Success.");
    }

    private static bool ZgProcessCb(int nPos, int nMax, IntPtr pUserData)
    {
        Console.Write("\r{0,5} / {1}", nPos, nMax);
        return true;
    }

    private static bool GetCtrList(ref ZG_CTR_KEY[] aList)
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
                Program.ByteArrayToStructure(array, ref obj);
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
                    Console.ReadLine();
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
                        Console.ReadLine();
                        return false;
                    }
                    array2.CopyTo(aList, num3);
                    for (int k = 0; k < num; k++)
                    {
                        byte[] array3 = Program.StructureToByteArray(array2[k]);
                        fileStream2.Write(array3, 0, array3.Length);
                    }
                }
                if (num < Program.m_nMaxKeys)
                {
                    ZG_CTR_KEY zG_CTR_KEY = default(ZG_CTR_KEY);
                    zG_CTR_KEY.fErased = true;
                    byte[] array3 = Program.StructureToByteArray(zG_CTR_KEY);
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

    private static bool GetNewList(ref List<MYKEY> oList)
    {
        Console.WriteLine("Enter file name:");
        string text = Console.ReadLine();
        if (text == "")
        {
            Console.WriteLine("Canceled.");
            return false;
        }
        if (!File.Exists(text))
        {
            Console.WriteLine("File not Found");
            return false;
        }
        text = Path.GetFullPath(text);
        StreamReader streamReader = new StreamReader(text);
        string text2;
        while ((text2 = streamReader.ReadLine()) != null)
        {
            string[] array = text2.Split(';');
            if (array.Length != 0)
            {
                MYKEY mYKEY = default(MYKEY);
                mYKEY.m_Num = new byte[16];
                if (Program.ParseKeyNum(ref mYKEY.m_Num, array[1]))
                {
                    Console.WriteLine(array[1]);
                    Console.WriteLine(mYKEY.m_Num);
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
        Console.WriteLine("Loaded {0} keys. Continue [y/n]?", oList.Count);
        return Console.ReadLine().ToUpper() == "Y";
    }

    private static int CompareZKeyNums(byte[] Left, byte[] Right)
    {
        int num = Math.Min(Left[0], Right[0]);
        for (int num2 = num; num2 > 0; num2--)
        {
            if (Left[num2] != Right[num2])
            {
                return (Left[num2] > Right[num2]) ? 1 : (-1);
            }
        }
        if (Left[0] != Right[0])
        {
            return (Left[0] > Right[0]) ? 1 : (-1);
        }
        return 0;
    }

    private static int FindEraised(ref ZG_CTR_KEY[] aList, int nStart, int nBank)
    {
        int num = nBank * Program.m_nMaxKeys;
        int num2 = num + Program.m_nMaxKeys;
        for (int i = num + nStart; i < num2; i++)
        {
            if (aList[i].fErased)
            {
                return i;
            }
        }
        return -1;
    }

    private static void SetCtrList(ref ZG_CTR_KEY[] aList, ref bool[] aSync)
    {
        bool flag = false;
        for (int i = 0; i < Program.m_nMaxBanks; i++)
        {
            int num = 0;
            int j = i * Program.m_nMaxKeys;
            while (num < Program.m_nMaxKeys)
            {
                if (aSync[j])
                {
                    num++;
                    j++;
                }
                else
                {
                    int num3 = num++;
                    int num5 = j++;
                    int num6 = 1;
                    for (int num7 = (i + 1) * Program.m_nMaxKeys; j < num7 && j - num5 < Program.m_nOptWrite; j++)
                    {
                        if (!aSync[j])
                        {
                            num6 = j - num5 + 1;
                        }
                        num++;
                    }
                    if (num6 > 0)
                    {
                        ZG_CTR_KEY[] array = new ZG_CTR_KEY[num6];
                        Array.Copy(aList, num5, array, 0, num6);
                        int num8 = ZGIntf.ZG_Ctr_WriteKeys(Program.m_hCtr, 0, array, array.Length, null, (IntPtr)0, i, true);
                        if (num8 < 0)
                        {
                            Console.WriteLine("Error ZG_Ctr_WriteKeys (bank num {0}) ({1}).", i, num8);
                            Console.ReadLine();
                            return;
                        }
                        Console.WriteLine("Updated keys {0}-{1} (bank num {2}).", num3, num3 + num6 - 1, i);
                        flag = true;
                    }
                }
            }
        }
        if (flag)
        {
            FileStream fileStream = new FileStream($"ctr{Program.m_nSn}_keys.bin", FileMode.Create, FileAccess.Write);
            for (int k = 0; k < aList.Length; k++)
            {
                byte[] array2 = Program.StructureToByteArray(aList[k]);
                fileStream.Write(array2, 0, array2.Length);
            }
            fileStream.Close();
        }
        else
        {
            Console.WriteLine("List not changed.");
        }
    }

    public static void WriteByteArray(byte[] bytes, string name)
    {
        Console.WriteLine(name);
        Console.WriteLine("--------------------------------".Substring(0, Math.Min(name.Length, "--------------------------------".Length)));
        Console.WriteLine(BitConverter.ToString(bytes));
        Console.WriteLine();
    }

    static void ShowEvents(int nStart, int nCount)
    {
        ZG_CTR_EVENT[] aEvents = new ZG_CTR_EVENT[6];
        ZG_CTR_EVENT rEv;
        int i = 0;
        int nIdx, nCnt;
        int hr;

        while (i < nCount)
        {
            nIdx = (nStart + i) % m_nCtrMaxEvents;
            nCnt = (nCount - i);
            if (nCnt > aEvents.Length)
                nCnt = aEvents.Length;
            if ((nIdx + nCnt) > m_nCtrMaxEvents)
                nCnt = (m_nCtrMaxEvents - nIdx);
            hr = ZGIntf.ZG_Ctr_ReadEvents(m_hCtr, nIdx, aEvents, nCnt, null, IntPtr.Zero);
            if (hr < 0)
            {
                Console.WriteLine("Ошибка ZG_Ctr_ReadEvents ({0}).", hr);
                Console.ReadLine();
                return;
            }
            for (int j = 0; j < nCnt; j++)
            {
                rEv = aEvents[j];
                switch (rEv.nType)
                {
                    case ZG_CTR_EV_TYPE.ZG_EV_ELECTRO_ON:
                    case ZG_CTR_EV_TYPE.ZG_EV_ELECTRO_OFF:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_EC_SUB_EV nSubEvent = new ZG_EC_SUB_EV();
                            UInt32 nPowerFlags = 0;
                            ZGIntf.ZG_Ctr_DecodeEcEvent(m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nPowerFlags);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Power flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                EvTypeStrs[(int)rEv.nType],
                                EcSubEvStrs[(int)nSubEvent], nPowerFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_FIRE_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_FIRE_SUB_EV nSubEvent = new ZG_FIRE_SUB_EV();
                            UInt32 nFireFlags = 0;
                            ZGIntf.ZG_Ctr_DecodeFireEvent(m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nFireFlags);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Fire flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                EvTypeStrs[(int)rEv.nType],
                                FireSubEvStrs[(int)nSubEvent], nFireFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_SECUR_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_SECUR_SUB_EV nSubEvent = new ZG_SECUR_SUB_EV();
                            UInt32 nSecurFlags = 0;
                            ZGIntf.ZG_Ctr_DecodeSecurEvent(m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nSecurFlags);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Security flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                EvTypeStrs[(int)rEv.nType],
                                SecurSubEvStrs[(int)nSubEvent], nSecurFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_MODE_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_CTR_MODE nMode = new ZG_CTR_MODE();
                            ZG_MODE_SUB_EV nSubEvent = new ZG_MODE_SUB_EV();
                            ZGIntf.ZG_Ctr_DecodeModeEvent(m_hCtr, rEv.aData, ref rTime, ref nMode, ref nSubEvent);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Mode: {7} Sub_event: {8}",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                EvTypeStrs[(int)rEv.nType],
                                ModeStrs[(int)nMode],
                                ModeSubEvStrs[(int)nSubEvent]);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_UNKNOWN_KEY:
                        {
                            Byte[] rKeyNum = new Byte[16];
                            ZGIntf.ZG_Ctr_DecodeUnkKeyEvent(m_hCtr, rEv.aData, rKeyNum);
                            Console.WriteLine("{0}.  Key \"{1}\"",
                                nIdx + j,
                                ZGIntf.CardNumToStr(rKeyNum, m_fProximity));
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_HOTEL40:
                    case ZG_CTR_EV_TYPE.ZG_EV_HOTEL41:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_HOTEL_MODE nMode = new ZG_HOTEL_MODE();
                            ZG_HOTEL_SUB_EV nSubEvent = new ZG_HOTEL_SUB_EV();
                            UInt32 nFlags = new UInt32();
                            ZGIntf.ZG_DecodeHotelEvent(rEv.aData, ref rTime, ref nMode, ref nSubEvent, ref nFlags);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Mode: {7} Sub_event: {8} flags: {9:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                EvTypeStrs[(int)rEv.nType],
                                HModeStrs[(int)nMode],
                                HotelSubEvStrs[(int)nSubEvent],
                                nFlags);
                        }
                        break;
                    default:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_CTR_DIRECT nDirect = new ZG_CTR_DIRECT();
                            int nKeyIdx = 0;
                            int nKeyBank = 0;
                            ZGIntf.ZG_Ctr_DecodePassEvent(m_hCtr, rEv.aData, ref rTime, ref nDirect, ref nKeyIdx, ref nKeyBank);
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} {7} (key_idx: {8}, bank#: {9})",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                DirectStrs[(int)nDirect],
                                EvTypeStrs[(int)rEv.nType],
                                nKeyIdx, nKeyBank);
                        }
                        break;
                }
            }
            i += nCnt;
        }
    }

    // MAIN FUNCTIONS ======================================================================
    private static void DoLoadKeysFromFile()
    {
        List<MYKEY> list = new List<MYKEY>();
        ZG_CTR_KEY[] array = null;
        if (Program.GetNewList(ref list) && Program.GetCtrList(ref array))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Program.WriteByteArray(list[i].m_Num, "arrayOne");
            }
            MyKeysComparer comparer = new MyKeysComparer();
            list.Sort(comparer);
            bool[] array2 = new bool[array.Length];
            MYKEY mYKEY = default(MYKEY);
            ZG_CTR_KEY zG_CTR_KEY;
            for (int j = 0; j < array.Length; j++)
            {
                zG_CTR_KEY = array[j];
                if (zG_CTR_KEY.fErased)
                {
                    array2[j] = true;
                }
                else
                {
                    mYKEY.m_Num = zG_CTR_KEY.rNum;
                    int num = list.BinarySearch(mYKEY, comparer);
                    if (num != -1)
                    {
                        mYKEY = list[num];
                        if (zG_CTR_KEY.nType != mYKEY.m_nType || zG_CTR_KEY.nAccess != mYKEY.m_nAccess)
                        {
                            array[j].nType = mYKEY.m_nType;
                            array[j].nAccess = mYKEY.m_nAccess;
                        }
                        else
                        {
                            array2[j] = true;
                        }
                        list.RemoveAt(num);
                    }
                    else
                    {
                        array[j].fErased = true;
                    }
                }
            }
            int[] array3 = new int[Program.m_nMaxBanks];
            for (int k = 0; k < array3.Length; k++)
            {
                array3[k] = 0;
            }
            for (int l = 0; l < list.Count; l++)
            {
                mYKEY = list[l];
                for (int m = 0; m < Program.m_nMaxBanks; m++)
                {
                    int num = Program.FindEraised(ref array, array3[m], m);
                    if (num == -1)
                    {
                        Console.WriteLine("Keys list overflow (bank: {0}).", m);
                        Console.ReadLine();
                        return;
                    }
                    zG_CTR_KEY = array[num];
                    zG_CTR_KEY.fErased = false;
                    zG_CTR_KEY.rNum = mYKEY.m_Num;
                    zG_CTR_KEY.nType = mYKEY.m_nType;
                    zG_CTR_KEY.nAccess = mYKEY.m_nAccess;
                    array[num] = zG_CTR_KEY;
                    array2[num] = false;
                    array3[m] = num + 1;
                }
            }
            Program.SetCtrList(ref array, ref array2);
            Console.WriteLine("Success.");
        }
    }

    private static void DoSaveKeysToFile()
    {
        Console.WriteLine("Enter file name:");
        string text = Console.ReadLine();
        if (text == "")
        {
            Console.WriteLine("Cancaled.");
        }
        else
        {
            ZG_CTR_KEY[] array = null;
            if (Program.GetCtrList(ref array))
            {
                StreamWriter streamWriter = new StreamWriter(text);
                for (int i = 0; i < Program.m_nMaxKeys; i++)
                {
                    ZG_CTR_KEY zG_CTR_KEY = array[i];
                    if (!zG_CTR_KEY.fErased)
                    {
                        streamWriter.WriteLine($"{ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity)}; {Program.KeyTypeAbbrs[(int)zG_CTR_KEY.nType]}; {zG_CTR_KEY.nAccess:X2}");
                    }
                }
                streamWriter.Close();
                Console.WriteLine("Success.");
            }
        }
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
                Console.ReadLine();
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
                            Console.ReadLine();
                            return;
                        }
                    }
                    ZG_CTR_KEY zG_CTR_KEY = array[j % array.Length];
                    if (!zG_CTR_KEY.fErased)
                    {
                        Console.WriteLine("{0} {1}, {2}, access: {3:X2}h.", j, ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity), Program.KeyTypeStrs[(int)zG_CTR_KEY.nType], zG_CTR_KEY.nAccess);
                    }
                }
            }
        }
        Console.WriteLine("Success.");
    }

    private static bool FindKeyEnum(int nIdx, ref ZG_CTR_KEY pKey, int nPos, int nMax, IntPtr pUserData)
    {
        bool flag = true;
        int num = (Program.m_rFindNum[0] < 6) ? Program.m_rFindNum[0] : 6;
        int num2 = 1;
        while (num2 <= num)
        {
            if (Program.m_rFindNum[num2] == pKey.rNum[num2])
            {
                num2++;
                continue;
            }
            flag = false;
            break;
        }
        if (flag)
        {
            Program.m_nFoundKeyIdx = nIdx;
            return false;
        }
        return true;
    }

    private static void DoFindKeyByNumber()
    {
        Console.WriteLine("Enter number of the bank, key number (-1 last used):");
        string text = Console.ReadLine();
        string[] array = text.Split(',');
        if (array.Length < 2)
        {
            Console.WriteLine("Wrong enter.");
        }
        else
        {
            Program.m_rFindNum = new byte[16];
            int nBankN = Convert.ToInt32(array[0]);
            int num;
            if (array[1] == "-1")
            {
                num = ZGIntf.ZG_Ctr_ReadLastKeyNum(Program.m_hCtr, Program.m_rFindNum);
                if (num < 0)
                {
                    Console.WriteLine("Error ZG_Ctr_ReadLastKeyNum ({0}).", num);
                    Console.ReadLine();
                    return;
                }
            }
            else if (!Program.ParseKeyNum(ref Program.m_rFindNum, array[1]))
            {
                Console.WriteLine("Wrong enter.");
                return;
            }
            Program.m_nFoundKeyIdx = -1;
            num = ZGIntf.ZG_Ctr_EnumKeys(Program.m_hCtr, 0, Program.FindKeyEnum, IntPtr.Zero, nBankN);
            if (num < 0)
            {
                Console.WriteLine("Error ZG_Ctr_EnumKeys ({0}).", num);
                Console.ReadLine();
            }
            else if (Program.m_nFoundKeyIdx != -1)
            {
                Console.WriteLine("Key {0} found (index={0}).", ZGIntf.CardNumToStr(Program.m_rFindNum, Program.m_fProximity), Program.m_nFoundKeyIdx);
            }
            else
            {
                Console.WriteLine("Key {0} not found.", ZGIntf.CardNumToStr(Program.m_rFindNum, Program.m_fProximity));
            }
        }
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
                Console.ReadLine();
                break;
            }
            Console.WriteLine("Bank {0}: {1}", i, num);
        }
        Console.WriteLine("Done.");
    }

    private static string codeTxt2Hex(string textCode)
    {
        int length = textCode.Length;
        int num = textCode.IndexOf(",");
        string value = textCode.Substring(0, num);
        string value2 = textCode.Substring(num + 1);
        int num2 = Convert.ToInt32(value);
        int num3 = Convert.ToInt32(value2);
        string str = num2.ToString("X");
        string str2 = num3.ToString("X");
        string text = "0000" + str2;
        string str3 = str + text.Substring(text.Length - 4);
        string text2 = "000000" + str3;
        return text2.Substring(text2.Length - 6);
    }

    private static bool DoSetKey(string keysList)
    {
        if (keysList == "")
        {
            Console.WriteLine("Canceled.");
            return false;
        }
        if (!File.Exists(keysList))
        {
            Console.WriteLine("File not Found");
            return false;
        }
        keysList = Path.GetFullPath(keysList);
        Console.WriteLine("Banks: {0}", Program.m_nMaxBanks);
        Program.DoClearAllKeys();
        for (int i = 0; i < Program.m_nMaxBanks; i++)
        {
            StreamReader streamReader = new StreamReader(keysList);
            int num = 0;
            string text;
            while ((text = streamReader.ReadLine()) != null)
            {
                if (!text.Contains("#"))
                {
                    string[] array = text.Split(';');
                    Console.WriteLine("bankNum: {3} | keyIndex: {0} | KeyType: {1} | Hex: {2}", num, "1", Program.codeTxt2Hex(array[1]), i);
                    int num2 = Convert.ToInt32(i);
                    int num3 = Convert.ToInt32(num);
                    int nType = Convert.ToInt32("1");
                    int num4 = Convert.ToInt32("FF", 16);
                    int num5;
                    if (num3 == -1)
                    {
                        num5 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num3, i);
                        if (num5 < 0)
                        {
                            Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex ({0}).", num5);
                            Console.ReadLine();
                            return false;
                        }
                    }
                    ZG_CTR_KEY[] array2 = new ZG_CTR_KEY[1];
                    array2[0].nType = (ZG_CTR_KEY_TYPE)nType;
                    array2[0].nAccess = (byte)num4;
                    array2[0].rNum = new byte[16];
                    if (array[2] == "-1")
                    {
                        num5 = ZGIntf.ZG_Ctr_ReadLastKeyNum(Program.m_hCtr, array2[0].rNum);
                        Console.WriteLine("ZG_Ctr_ReadLastKeyNum ({0}).", array2[0].rNum);
                        Program.WriteByteArray(array2[0].rNum, "ZG_Ctr_ReadLastKeyNum");
                        if (num5 < 0)
                        {
                            Console.WriteLine("Error ZG_Ctr_ReadLastKeyNum ({0}).", num5);
                            Console.ReadLine();
                            return false;
                        }
                    }
                    else if (!Program.ParseKeyNum(ref array2[0].rNum, Program.codeTxt2Hex(array[1])))
                    {
                        Console.WriteLine("Wrong enter.");
                        return false;
                    }
                    num5 = ZGIntf.ZG_Ctr_WriteKeys(Program.m_hCtr, num3, array2, 1, null, IntPtr.Zero, i, true);
                    if (num5 < 0)
                    {
                        Console.WriteLine("Error ZG_Ctr_WriteKeys ({0}).", num5);
                        Console.ReadLine();
                        return false;
                    }
                    Console.WriteLine("Succes.");
                    num++;
                }
            }
            Console.WriteLine("============================");
        }
        return true;
    }

    private static void DoClearKey()
    {
        Console.WriteLine("Enter number of the bank, key index (-1 last key):");
        string text = Console.ReadLine();
        string[] array = text.Split(',');
        if (array.Length < 2)
        {
            Console.WriteLine("Wrong enter.");
        }
        else
        {
            int nBankN = Convert.ToInt32(array[0]);
            int num = Convert.ToInt32(array[1]);
            int num3;
            if (num == -1)
            {
                int num2 = 0;
                num3 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num2, nBankN);
                if (num3 < 0)
                {
                    Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex ({0}).", num3);
                    Console.ReadLine();
                    return;
                }
                if (num2 == 0)
                {
                    Console.WriteLine("Key list empty.");
                    return;
                }
                num = num2 - 1;
            }
            num3 = ZGIntf.ZG_Ctr_ClearKeys(Program.m_hCtr, num, 1, null, IntPtr.Zero, nBankN, true);
            if (num3 < 0)
            {
                Console.WriteLine("Error ZG_Ctr_ClearKeys ({0}).", num3);
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Success.");
            }
        }
    }

    private static void DoClearAllKeys()
    {
        int num = 0;
        int num3;
        while (true)
        {
            if (num < Program.m_nMaxBanks)
            {
                int num2 = 0;
                num3 = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref num2, num);
                if (num3 < 0)
                {
                    Console.WriteLine("Error ZG_Ctr_GetKeyTopIndex ({0}).", num3);
                    Console.ReadLine();
                    return;
                }
                if (num2 == 0)
                {
                    Console.WriteLine("Key list empty.");
                    return;
                }
                Console.WriteLine("Erasing...");
                num3 = ZGIntf.ZG_Ctr_ClearKeys(Program.m_hCtr, 0, num2, null, IntPtr.Zero, num, true);
                if (num3 >= 0)
                {
                    Console.WriteLine("Success.");
                    num++;
                    continue;
                }
                break;
            }
            return;
        }
        Console.WriteLine("Error ZG_Ctr_ClearKeys ({0}).", num3);
        Console.ReadLine();
    }

    static void DoShowNewEvents()
    {
        int hr;
        int nWrIdx = 0;
        int nRdIdx = 0;
        hr = ZGIntf.ZG_Ctr_ReadEventIdxs(m_hCtr, ref nWrIdx, ref nRdIdx);
        if (hr < 0)
        {
            Console.WriteLine("Ошибка ZG_Ctr_ReadEventIdxs ({0}).", hr);
            Console.ReadLine();
            return;
        }
        int nNewCount;
        if (nWrIdx >= m_nAppReadEventIdx)
            nNewCount = (nWrIdx - m_nAppReadEventIdx);
        else
            nNewCount = (m_nCtrMaxEvents - m_nAppReadEventIdx + nWrIdx);
        if (nNewCount == 0)
            Console.WriteLine("Нет новых событий ({0}-{1}).", nRdIdx, nWrIdx);
        else
            Console.WriteLine("Доступно {0} новых событий ({1}-{2}).", nNewCount, nRdIdx, nWrIdx);
        int nShowCount;
        while (nNewCount > 0)
        {
            nShowCount = 25;
            if (nShowCount > nNewCount)
                nShowCount = nNewCount;
            ShowEvents(m_nAppReadEventIdx, nShowCount);
            nNewCount -= nShowCount;
            m_nAppReadEventIdx = (m_nAppReadEventIdx + nShowCount) % m_nCtrMaxEvents;
            Console.WriteLine("Нажмите Enter для продолжения или 'x' для прерывания.");
            String s = Console.ReadLine();
            if (s == "x")
            {
                Console.WriteLine("Прервано.");
                return;
            }
        }
        Console.WriteLine("Успешно.");
    }

    // MESSAGES AND THREADS =================================================================================================
    private static int CheckNotifyMsgs()
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
                Console.WriteLine("==> Bank {0}: top key border changed: ({1} -> {2}).", zG_N_KEY_TOP_INFO.nBankN, zG_N_KEY_TOP_INFO.nOldTopIdx, zG_N_KEY_TOP_INFO.nNewTopIdx);
            }
        }
        if (num2 == 262658)
        {
            num2 = ZGIntf.S_OK;
        }
        return num2;
    }

    private static void DoNotifyWork()
    {
        while (Program.m_fThreadActive)
        {
            if (Program.m_oEvent.WaitOne())
            {
                Program.m_oEvent.Reset();
                if (Program.m_hCtr != IntPtr.Zero)
                {
                    Program.CheckNotifyMsgs();
                }
            }
        }
    }

    private static void StartNotifyThread()
    {
        if (Program.m_oThread == null)
        {
            Program.m_fThreadActive = true;
            Program.m_oThread = new Thread(Program.DoNotifyWork);
            Program.m_oThread.Start();
        }
    }

    private static void StopNotifyThread()
    {
        if (Program.m_oThread != null)
        {
            Program.m_fThreadActive = false;
            Program.m_oEvent.Set();
            Program.m_oThread.Join();
            Program.m_oThread = null;
        }
    }

    // ENTRY POINT ============================================================
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Need more argumetns, <mode> <address:port>");
            Program.StopNotifyThread();
            if (Program.m_hCtr != IntPtr.Zero)
            {
                ZGIntf.ZG_CloseHandle(Program.m_hCtr);
            }
            ZGIntf.ZG_Finalyze();
        }
        else
        {
            string pszName = args[1];
            string text = args[0];
            // uint num = ZGIntf.ZG_GetVersion();
            UInt32 nVersion = ZGIntf.ZG_GetVersion();
            if ((((nVersion & 0xFF)) != ZGIntf.ZG_SDK_VER_MAJOR) || (((nVersion >> 8) & 0xFF) != ZGIntf.ZG_SDK_VER_MINOR))
            {
                Console.WriteLine("SDK Guard Version wrong");
                Console.ReadLine();
            }
            else
            {
                IntPtr intPtr = new IntPtr(0);
                int num2 = ZGIntf.ZG_Initialize(1u);
                if (num2 < 0)
                {
                    Console.WriteLine("Error ZG_Initialize ({0}).", num2);
                    Console.ReadLine();
                }
                else
                {
                    try
                    {
                        ZG_CVT_INFO pInfo = new ZG_CVT_INFO();
                        ZG_CVT_OPEN_PARAMS zG_CVT_OPEN_PARAMS = default(ZG_CVT_OPEN_PARAMS);
                        zG_CVT_OPEN_PARAMS.nPortType = ZP_PORT_TYPE.ZP_PORT_IP;
                        zG_CVT_OPEN_PARAMS.pszName = pszName;
                        zG_CVT_OPEN_PARAMS.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;
                        num2 = ZGIntf.ZG_Cvt_Open(ref intPtr, ref zG_CVT_OPEN_PARAMS, pInfo);
                        if (num2 < 0)
                        {
                            Console.WriteLine("Error ZG_Cvt_Open ({0}).", num2);
                            Console.ReadLine();
                        }
                        else
                        {
                            ZG_CTR_INFO zG_CTR_INFO = default(ZG_CTR_INFO);
                            num2 = ZGIntf.ZG_Ctr_Open(ref Program.m_hCtr, intPtr, 2, 0, ref zG_CTR_INFO, ZG_CTR_TYPE.ZG_CTR_UNDEF);
                            if (num2 < 0)
                            {
                                Console.WriteLine("Error ZG_Ctr_Open ({0}).", num2);
                                Console.ReadLine();
                            }
                            else
                            {
                                Program.m_nCtrMaxEvents = zG_CTR_INFO.nMaxEvents;
                                Program.m_nSn = zG_CTR_INFO.nSn;
                                Program.m_fProximity = ((zG_CTR_INFO.nFlags & 2) != 0);
                                Program.m_nMaxBanks = (((zG_CTR_INFO.nFlags & 1) == 0) ? 1 : 2);
                                Program.m_nMaxKeys = zG_CTR_INFO.nMaxKeys;
                                Program.m_nOptRead = zG_CTR_INFO.nOptReadItems;
                                Program.m_nOptWrite = zG_CTR_INFO.nOptWriteItems;
                                Console.WriteLine("{0} Address: {1}, s/n: {2}, v{3}.{4}, Bank count: {5}, Key Types: {6}.", Program.CtrTypeStrs[(int)zG_CTR_INFO.nType], zG_CTR_INFO.nAddr, zG_CTR_INFO.nSn, zG_CTR_INFO.nVersion & 0xFF, zG_CTR_INFO.nVersion >> 8 & 0xFF, Program.m_nMaxBanks, Program.KeyModeStrs[Program.m_fProximity ? 1 : 0]);
                                 Program.m_oEvent = new ManualResetEvent(false);
                                ZG_CTR_NOTIFY_SETTINGS pSettings = new ZG_CTR_NOTIFY_SETTINGS(4u, Program.m_oEvent.SafeWaitHandle, IntPtr.Zero, 0u, 0, 3000u, 0u);
                                num2 = ZGIntf.ZG_Ctr_SetNotification(Program.m_hCtr, pSettings);
                                if (num2 < 0)
                                {
                                    Console.WriteLine("Error ZG_Ctr_SetNotification ({0}).", num2);
                                    Console.ReadLine();
                                }
                                else
                                {
                                    Program.StartNotifyThread();
                                    Console.WriteLine("-----");
                                    if (text == "-h")
                                    {
                                        text = "10";
                                    }
                                    if (text != "")
                                    {
                                        Console.WriteLine();
                                        switch (Convert.ToInt16(text))
                                        {
                                            case 0:
                                                return;
                                            case 1:
                                                Program.ShowKeys();
                                                break;
                                            case 2:
                                                Program.DoFindKeyByNumber();
                                                break;
                                            case 3:
                                                Program.ShowKeyTopIndex();
                                                break;
                                            case 4:
                                                {
                                                    string keysList = args[2];
                                                    Program.DoSetKey(keysList);
                                                    break;
                                                }
                                            case 5:
                                                Program.DoLoadKeysFromFile();
                                                break;
                                            case 6:
                                                Program.DoSaveKeysToFile();
                                                break;
                                            case 7:
                                                Program.DoClearKey();
                                                break;
                                            case 8:
                                                Program.DoClearAllKeys();
                                                break;
                                            case 9:
                                                Program.DoClearKeyCache();
                                                break;
                                            case 10:
                                                Console.WriteLine("1 - Show Keys");
                                                Console.WriteLine("2 - Search key by number...");
                                                Console.WriteLine("3 - Top border of the keys...");
                                                Console.WriteLine("4 - Setup key...");
                                                Console.WriteLine("5 - Upload new key list...");
                                                Console.WriteLine("6 - Save keys to file or to cache");
                                                Console.WriteLine("7 - Erase key...");
                                                Console.WriteLine("8 - Erase all keys...");
                                                Console.WriteLine("9 - Earse key cache (delete tmp file)");
                                                Console.WriteLine("11 - Show Events");
                                                Console.WriteLine("0 - Quit");
                                                break;
                                            case 11:
                                                DoShowNewEvents();
                                                break;
                                            default:
                                                Console.WriteLine("Wrong command.");
                                                break;
                                        }
                                    }
                                    Console.WriteLine("-----");
                                }
                            }
                        }
                    }
                    finally
                    {
                        Program.StopNotifyThread();
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
}
