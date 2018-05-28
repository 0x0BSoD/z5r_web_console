using System;
using System.Globalization;
using System.Runtime.InteropServices;

using Newtonsoft.Json;
using ZGuard;

class Helpers
    {
    public static void ResetEventsIndex()
    {
        int hr;
        hr = ZGIntf.ZG_Ctr_WriteEventIdxs(Program.m_hCtr, 0x3, 0, 0);
        if (hr < 0)
        {
            Helpers.StringGenerateAnswer("Error ZG_Ctr_WriteEventIdxs", false);
            return;
        }
        Program.m_nAppReadEventIdx = 0;
    }
    public static bool FindKeyEnum(int nIdx, ref ZG_CTR_KEY pKey, int nPos, int nMax, IntPtr pUserData)
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
    public static int FindEraised(ref ZG_CTR_KEY[] aList, int nStart, int nBank)
    {
        int n = (nBank * Program.m_nMaxKeys);
        int nEnd = (n + Program.m_nMaxKeys);

        Console.WriteLine("Bank: {0}", nBank);
        Console.WriteLine("n keys: {0}", n);
        Console.WriteLine("nEnd keys: {0}", nEnd);
        Console.WriteLine("==============");

        for (int i = (n + nStart); i < nEnd; i++)
        {
            if (aList[i].fErased)
            {
                return i;
            }

        }

        for (int i = (n + nStart); i < nEnd; i++)
        {
            if (aList[i].fErased)
            {
                return i;
            }
        }
        return 0;
    }
    public static void WriteByteArray(byte[] bytes, string name)
    {
        Console.WriteLine(name);
        Console.WriteLine("------------------------".Substring(0, Math.Min(name.Length, "--------------------------------".Length)));
        Console.WriteLine(BitConverter.ToString(bytes));
        Console.WriteLine();
    }
    public static int CompareZKeyNums(Byte[] Left, Byte[] Right)
    {
        int n = Math.Min(Left[0], Right[0]);
        for (int i = n; i > 0; i--)
            if (Left[i] != Right[i])
                return (Left[i] > Right[i]) ? 1 : -1;
        if (Left[0] != Right[0])
            return (Left[0] > Right[0]) ? 1 : -1;
        return 0;
    }
    public static bool ParseKeyNum(ref byte[] rKeyNum, string sText)
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

        public static byte[] StructureToByteArray(object obj)
        {
            int num = Marshal.SizeOf(obj);
            byte[] array = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.StructureToPtr(obj, intPtr, true);
            Marshal.Copy(intPtr, array, 0, num);
            Marshal.FreeHGlobal(intPtr);
            return array;
        }

        public static void ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int num = Marshal.SizeOf(obj);
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.Copy(bytearray, 0, intPtr, num);
            obj = Marshal.PtrToStructure(intPtr, obj.GetType());
            Marshal.FreeHGlobal(intPtr);
        }

        public static void StringGenerateAnswer(object data, bool status)
        {
            var result = new
            {
                Status = status ? "ok" : "fail",
                Data = data
            };

            Console.WriteLine(JsonConvert.SerializeObject(result));

        }

        public static string codeTxt2Hex(string textCode)
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
}



