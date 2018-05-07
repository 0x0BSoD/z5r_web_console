// HELPERS =======================================================
using System;
using System.Globalization;
using System.Runtime.InteropServices;
class Helpers
    {
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
    }

