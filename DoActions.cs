using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Newtonsoft.Json;
using ZGuard;
class DoActions
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MYKEY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] m_Num;
        public ZG_CTR_KEY_TYPE m_nType;
        public UInt32 m_nAccess;

    }

    public class MyKeysComparer : IComparer<MYKEY>
    {
        public int Compare(MYKEY x, MYKEY y)
        {
            return Helpers.CompareZKeyNums(x.m_Num, y.m_Num);
        }
    }

    public static void DoClearKeyCache()
    {
        File.Delete($"ctr{Program.m_nSn}_keys.bin");
        Helpers.StringGenerateAnswer("File delete Success", true);
    }

    public static void DoSetKey(string bank, string nIdsx, string key_num, string key_type, string access_level)
    {
        int nBankN, nKeyIdx, nKeyType, nKeyAccess;

        nBankN = Convert.ToInt32(bank);
        nKeyIdx = Convert.ToInt32(nIdsx);
        nKeyType = Convert.ToInt32(key_type);
        nKeyAccess = Convert.ToInt32(access_level, 16);

        int hr;
        if (nKeyIdx == -1)
        {
            hr = ZGIntf.ZG_Ctr_GetKeyTopIndex(Program.m_hCtr, ref nKeyIdx, nBankN);
            if (hr < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Ctr_GetKeyTopIndex", false);
                return;
            }
        }
        ZG_CTR_KEY[] aKeys = new ZG_CTR_KEY[1];
        aKeys[0].nType = (ZG_CTR_KEY_TYPE)nKeyType;
        aKeys[0].nAccess = (Byte)nKeyAccess;
        aKeys[0].rNum = new Byte[16];
        if (key_num == "-1")
        {
            hr = ZGIntf.ZG_Ctr_ReadLastKeyNum(Program.m_hCtr, aKeys[0].rNum);
            if (hr < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Ctr_ReadLastKeyNum", false);
                return;
            }
        }
        else if (!Helpers.ParseKeyNum(ref aKeys[0].rNum, key_num))
        {
            return;
        }
        hr = ZGIntf.ZG_Ctr_WriteKeys(Program.m_hCtr, nKeyIdx, aKeys, 1, null, IntPtr.Zero, nBankN);
        if (hr < 0)
        {
            Helpers.StringGenerateAnswer("Error ZG_Ctr_WriteKeys", false);
            return;
        }
        Helpers.StringGenerateAnswer(key_num + " set", true);

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
                byte[] array2 = Helpers.StructureToByteArray(aList[k]);
                fileStream.Write(array2, 0, array2.Length);
            }
            fileStream.Close();
        }
        else
        {
            Console.WriteLine("List not changed.");
        }
    }
    public static void DoSaveKeysToFile()
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
                        streamWriter.WriteLine($"{ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity)}; {Event_strs.KeyTypeAbbrs[(int)zG_CTR_KEY.nType]}; {zG_CTR_KEY.nAccess:X2}");
                    }
                }
                streamWriter.Close();
                Console.WriteLine("Success.");
            }
        }
    }
    public static void DoFindKeyByNumber(string bank, string key_num)
    {
        Program.m_rFindNum = new byte[16];
        int nBankN = Convert.ToInt32(bank);
        int num;
        if (key_num == "-1")
        {
            num = ZGIntf.ZG_Ctr_ReadLastKeyNum(Program.m_hCtr, Program.m_rFindNum);
            if (num < 0)
            {
                Console.WriteLine("Error ZG_Ctr_ReadLastKeyNum ({0}).", num);
                Console.ReadLine();
                return;
            }
        }
        else if (!Helpers.ParseKeyNum(ref Program.m_rFindNum, key_num))
        {
            Console.WriteLine("Wrong enter.");
            return;
        }
        Program.m_nFoundKeyIdx = -1;
        num = ZGIntf.ZG_Ctr_EnumKeys(Program.m_hCtr, 0, Helpers.FindKeyEnum, IntPtr.Zero, nBankN);
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
    public static bool DoLoadKeysFromFile(string keysList)
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
        DoActions.DoClearAllKeys();
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
                        Helpers.WriteByteArray(array2[0].rNum, "ZG_Ctr_ReadLastKeyNum");
                        if (num5 < 0)
                        {
                            Console.WriteLine("Error ZG_Ctr_ReadLastKeyNum ({0}).", num5);
                            return false;
                        }
                    }
                    else if (!Helpers.ParseKeyNum(ref array2[0].rNum, Helpers.codeTxt2Hex(array[1])))
                    {
                        Console.WriteLine("Wrong enter.");
                        return false;
                    }
                    num5 = ZGIntf.ZG_Ctr_WriteKeys(Program.m_hCtr, num3, array2, 1, null, IntPtr.Zero, i, true);
                    if (num5 < 0)
                    {
                        Console.WriteLine("Error ZG_Ctr_WriteKeys ({0}).", num5);
                        return false;
                    }
                    num++;
                }
            }
            Console.WriteLine("{\"Bank\": \""+ i +"\", \"Status\": \"ok\"}");
        }
        return true;
    }

    public static void DoClearKey(string bank, string key_num)
    {
        int nBankN = Convert.ToInt32(bank);
        int num = Convert.ToInt32(key_num);
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

    public static void DoClearAllKeys()
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
                    Helpers.StringGenerateAnswer("Error ZG_Ctr_GetKeyTopIndex", false);
                    return;
                }
                if (num2 == 0)
                {
                    Helpers.StringGenerateAnswer("keys list empty", true);
                    return;
                }
                num3 = ZGIntf.ZG_Ctr_ClearKeys(Program.m_hCtr, 0, num2, null, IntPtr.Zero, num, true);
                if (num3 >= 0)
                {
                    Helpers.StringGenerateAnswer("keys cleared", true);
                    num++;
                    continue;
                }
                break;
            }
            return;
        }
        Helpers.StringGenerateAnswer("Error ZG_Ctr_ClearKeys", false);
    }

    public static void DoShowNewEvents()
    {
        int hr;
        int nWrIdx = 0;
        int nRdIdx = 0;
        List<string> ls_result = new List<string>();

        hr = ZGIntf.ZG_Ctr_ReadEventIdxs(Program.m_hCtr, ref nWrIdx, ref nRdIdx);
        if (hr < 0)
        {
            Helpers.StringGenerateAnswer("Error ZG_Ctr_ReadEventIdxs", false);
            return;
        }
        int nNewCount;
        if (nWrIdx >= Program.m_nAppReadEventIdx)
            nNewCount = (nWrIdx - Program.m_nAppReadEventIdx);
        else
            nNewCount = (Program.m_nCtrMaxEvents - Program.m_nAppReadEventIdx + nWrIdx);
        if (nNewCount == 0)
        {
            Helpers.StringGenerateAnswer("empty", true);
            return;
        }
        int nShowCount;
        while (nNewCount > 0)
        {
            nShowCount = 25;
            if (nShowCount > nNewCount)
                nShowCount = nNewCount;
            ls_result = ShowEvents(Program.m_nAppReadEventIdx, nShowCount);


            nNewCount -= nShowCount;
            Program.m_nAppReadEventIdx = (Program.m_nAppReadEventIdx + nShowCount) % Program.m_nCtrMaxEvents;
        }

        string output = "[" + String.Join(",", ls_result) + "]";
        Helpers.StringGenerateAnswer(output, true);
        Helpers.ResetEventsIndex();
    }
    static List<string> ShowEvents(int nStart, int nCount)
    {
        ZG_CTR_EVENT[] aEvents = new ZG_CTR_EVENT[6];
        ZG_CTR_EVENT rEv;
        int i = 0;
        int nIdx, nCnt;
        int hr;
        List<string> ls_result = new List<string>();
        string key = null;

        while (i < nCount)
        {
            nIdx = (nStart + i) % Program.m_nCtrMaxEvents;
            nCnt = (nCount - i);
            if (nCnt > aEvents.Length)
                nCnt = aEvents.Length;
            if ((nIdx + nCnt) > Program.m_nCtrMaxEvents)
                nCnt = (Program.m_nCtrMaxEvents - nIdx);
            hr = ZGIntf.ZG_Ctr_ReadEvents(Program.m_hCtr, nIdx, aEvents, nCnt, null, IntPtr.Zero);
            if (hr < 0)
            {
                Helpers.StringGenerateAnswer("Error ZG_Ctr_ReadEvents", false);
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
                            ZGIntf.ZG_Ctr_DecodeEcEvent(Program.m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nPowerFlags);

                            Helpers.StringGenerateAnswer(Event_strs.EvTypeStrs[(int)rEv.nType], true);

                            // For debug
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Power flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                Event_strs.EvTypeStrs[(int)rEv.nType],
                                Event_strs.EcSubEvStrs[(int)nSubEvent], nPowerFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_FIRE_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_FIRE_SUB_EV nSubEvent = new ZG_FIRE_SUB_EV();
                            UInt32 nFireFlags = 0;
                            ZGIntf.ZG_Ctr_DecodeFireEvent(Program.m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nFireFlags);

                            Helpers.StringGenerateAnswer(Event_strs.EvTypeStrs[(int)rEv.nType], true);

                            // For debug
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Fire flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                Event_strs.EvTypeStrs[(int)rEv.nType],
                                Event_strs.FireSubEvStrs[(int)nSubEvent], nFireFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_SECUR_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_SECUR_SUB_EV nSubEvent = new ZG_SECUR_SUB_EV();
                            UInt32 nSecurFlags = 0;
                            ZGIntf.ZG_Ctr_DecodeSecurEvent(Program.m_hCtr, rEv.aData, ref rTime, ref nSubEvent, ref nSecurFlags);

                            Helpers.StringGenerateAnswer(Event_strs.EvTypeStrs[(int)rEv.nType], true);

                            // For debug
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Sub_event: {7} Security flags: {8:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                Event_strs.EvTypeStrs[(int)rEv.nType],
                                Event_strs.SecurSubEvStrs[(int)nSubEvent], nSecurFlags);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_MODE_STATE:
                        {
                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_CTR_MODE nMode = new ZG_CTR_MODE();
                            ZG_MODE_SUB_EV nSubEvent = new ZG_MODE_SUB_EV();
                            ZGIntf.ZG_Ctr_DecodeModeEvent(Program.m_hCtr, rEv.aData, ref rTime, ref nMode, ref nSubEvent);

                            Helpers.StringGenerateAnswer(Event_strs.EvTypeStrs[(int)rEv.nType], true);

                            // For debug
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Mode: {7} Sub_event: {8}",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                Event_strs.EvTypeStrs[(int)rEv.nType],
                                Event_strs.ModeStrs[(int)nMode],
                                Event_strs.ModeSubEvStrs[(int)nSubEvent]);
                        }
                        break;
                    case ZG_CTR_EV_TYPE.ZG_EV_UNKNOWN_KEY:
                        {
                            Byte[] rKeyNum = new Byte[16];
                            ZGIntf.ZG_Ctr_DecodeUnkKeyEvent(Program.m_hCtr, rEv.aData, rKeyNum);

                            key = ZGIntf.CardNumToStr(rKeyNum, Program.m_fProximity);
                            Helpers.StringGenerateAnswer("Unknown key: " + key, true);

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

                            Helpers.StringGenerateAnswer(Event_strs.EvTypeStrs[(int)rEv.nType], true);

                            // For debug
                            Console.WriteLine("{0}. {1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2} {6} Mode: {7} Sub_event: {8} flags: {9:X2}h",
                                nIdx + j,
                                rTime.nDay, rTime.nMonth,
                                rTime.nHour, rTime.nMinute, rTime.nSecond,
                                Event_strs.EvTypeStrs[(int)rEv.nType],
                                Event_strs.HModeStrs[(int)nMode],
                                Event_strs.HotelSubEvStrs[(int)nSubEvent],
                                nFlags);
                        }
                        break;
                    default:
                        {

                            ZG_EV_TIME rTime = new ZG_EV_TIME();
                            ZG_CTR_DIRECT nDirect = new ZG_CTR_DIRECT();

                            int nKeyIdx = 0;
                            int nKeyBank = 0;

                            ZG_CTR_KEY[] array = new ZG_CTR_KEY[1];

                            ZGIntf.ZG_Ctr_DecodePassEvent(Program.m_hCtr, rEv.aData, ref rTime, ref nDirect, ref nKeyIdx, ref nKeyBank);
                            int keyIndex = nKeyIdx;
                            if (key == null)
                            {
                                int num = ZGIntf.ZG_Ctr_ReadKeys(Program.m_hCtr, keyIndex, array, 1, null, IntPtr.Zero, nKeyBank);

                                ZG_CTR_KEY zG_CTR_KEY = array[keyIndex % array.Length];
                                if (!zG_CTR_KEY.fErased)
                                {
                                    key = ZGIntf.CardNumToStr(zG_CTR_KEY.rNum, Program.m_fProximity);
                                }
                            }

                            try
                            {
                                key = key.Split()[1];
                            }
                            catch
                            {
                                key = "none";
                            }

                            var text_tmp = new
                            {
                                date = rTime.nDay + "." + rTime.nMonth + "T" + rTime.nHour + ":" + rTime.nMinute,
                                direction = Event_strs.DirectStrs[(int)nDirect],
                                event_id = Event_strs.EvTypeStrs[(int)rEv.nType],
                                key = key
                            };

                            ls_result.Add(JsonConvert.SerializeObject(text_tmp));
                            key = null;
                        }
                        break;
                }
            }
            i += nCnt;
        }
        return ls_result;
    }
}
