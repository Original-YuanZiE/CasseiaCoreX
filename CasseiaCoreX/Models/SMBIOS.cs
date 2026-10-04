using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CasseiaCoreX.Models
{
    public static class SMBIOS
    {
        // 此类由 ChatGPT 生成，参考了微软官方文档和其他开源项目
        // 'RSMB'
        // 注意 GetSystemFirmwareTable 要的是 DWORD signature。
        // 0x52534D42 对应 RSMB。
        private const uint RSMB = 0x52534D42;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetSystemFirmwareTable(
            uint FirmwareTableProviderSignature,
            uint FirmwareTableID,
            IntPtr pFirmwareTableBuffer,
            uint BufferSize);

        public class SystemInfo
        {
            public string Manufacturer { get; set; } = "";
            public string ProductName { get; set; } = "";
            public string Version { get; set; } = "";
            public string SerialNumber { get; set; } = "";
            public string SKU { get; set; } = "";
            public string Family { get; set; } = "";
        }

        public class BaseboardInfo
        {
            public string Manufacturer { get; set; } = "";
            public string Product { get; set; } = "";
            public string Version { get; set; } = "";
            public string SerialNumber { get; set; } = "";
            public string AssetTag { get; set; } = "";
        }

        private static byte[] GetRawSMBIOS()
        {
            // 第一次调用只问大小。
            uint size = GetSystemFirmwareTable(
                RSMB,
                0,
                IntPtr.Zero,
                0);

            if (size == 0)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)size);

            try
            {
                uint result = GetSystemFirmwareTable(
                    RSMB,
                    0,
                    buffer,
                    size);

                if (result == 0)
                {
                    throw new Win32Exception(
                        Marshal.GetLastWin32Error());
                }

                byte[] data = new byte[result];
                Marshal.Copy(buffer, data, 0, (int)result);

                return data;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static SystemInfo GetSystemInfo()
        {
            byte[] raw = GetRawSMBIOS();

            foreach (var structure in EnumerateStructures(raw))
            {
                if (structure.Type != 1)
                {
                    continue;
                }

                return new SystemInfo
                {
                    Manufacturer =
                        structure.GetString(0x04),

                    ProductName =
                        structure.GetString(0x05),

                    Version =
                        structure.GetString(0x06),

                    SerialNumber =
                        structure.GetString(0x07),

                    SKU =
                        structure.Length > 0x19
                            ? structure.GetString(0x19)
                            : "",

                    Family =
                        structure.Length > 0x1A
                            ? structure.GetString(0x1A)
                            : ""
                };
            }

            return new SystemInfo();
        }

        public static BaseboardInfo GetBaseboardInfo()
        {
            byte[] raw = GetRawSMBIOS();

            foreach (var structure in EnumerateStructures(raw))
            {
                if (structure.Type != 2)
                {
                    continue;
                }

                return new BaseboardInfo
                {
                    Manufacturer =
                        structure.GetString(0x04),

                    Product =
                        structure.GetString(0x05),

                    Version =
                        structure.GetString(0x06),

                    SerialNumber =
                        structure.GetString(0x07),

                    AssetTag =
                        structure.GetString(0x08)
                };
            }

            return new BaseboardInfo();
        }

        private static IEnumerable<SMBIOSStructure> EnumerateStructures(
            byte[] raw)
        {
            /*
             * RawSMBIOSData:
             *
             * byte Used20CallingMethod
             * byte MajorVersion
             * byte MinorVersion
             * byte DmiRevision
             * uint Length
             * byte SMBIOSTableData[]
             *
             * 所以前 8 byte 不是 SMBIOS table。
             */

            if (raw.Length < 8)
            {
                yield break;
            }

            uint tableLength = BitConverter.ToUInt32(raw, 4);

            int tableStart = 8;

            int tableEnd = Math.Min(
                raw.Length,
                tableStart + (int)tableLength);

            int position = tableStart;

            while (position + 4 <= tableEnd)
            {
                byte type = raw[position];
                byte length = raw[position + 1];

                if (length < 4 ||
                    position + length > tableEnd)
                {
                    yield break;
                }

                int stringStart = position + length;
                int nextStructure = FindStructureEnd(
                    raw,
                    stringStart,
                    tableEnd);

                yield return new SMBIOSStructure(
                    raw,
                    position,
                    length,
                    stringStart,
                    nextStructure);

                // Type 127 = End-of-Table
                if (type == 127)
                {
                    yield break;
                }

                position = nextStructure;
            }
        }

        private static int FindStructureEnd(
            byte[] data,
            int stringStart,
            int tableEnd)
        {
            /*
             * 每个 SMBIOS structure 后面是：
             *
             * "String 1\0String 2\0String 3\0\0"
             *
             * 连续两个 NUL 才代表这个 structure 真正结束。
             */

            for (int i = stringStart; i + 1 < tableEnd; i++)
            {
                if (data[i] == 0 &&
                    data[i + 1] == 0)
                {
                    return i + 2;
                }
            }

            return tableEnd;
        }

        private class SMBIOSStructure
        {
            private readonly byte[] data;
            private readonly int start;
            private readonly int stringStart;
            private readonly int end;

            public byte Type => data[start];

            public byte Length { get; }

            public SMBIOSStructure(
                byte[] data,
                int start,
                byte length,
                int stringStart,
                int end)
            {
                this.data = data;
                this.start = start;
                this.stringStart = stringStart;
                this.end = end;
                Length = length;
            }

            public string GetString(int fieldOffset)
            {
                if (fieldOffset >= Length)
                {
                    return "";
                }

                byte stringIndex =
                    data[start + fieldOffset];

                if (stringIndex == 0)
                {
                    return "";
                }

                return ReadString(stringIndex);
            }

            private string ReadString(int wantedIndex)
            {
                int currentIndex = 1;
                int position = stringStart;

                while (position < end - 1)
                {
                    int zero = position;

                    while (zero < end &&
                           data[zero] != 0)
                    {
                        zero++;
                    }

                    if (currentIndex == wantedIndex)
                    {
                        return Encoding.ASCII.GetString(
                            data,
                            position,
                            zero - position);
                    }

                    currentIndex++;
                    position = zero + 1;
                }

                return "";
            }
        }
    }
}
