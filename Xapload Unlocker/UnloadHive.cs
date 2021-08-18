using System;
using System.Runtime.InteropServices;

namespace Xapload_Unlocker
{
    class UnloadHive
    {
        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(string text);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Int32 RegUnLoadKey(IntPtr hKey, string lpSubKey);

        public static void UnloadRegistryHive(string keyName)
        {
            int retVal = RegUnLoadKey(HKEY_LOCAL_MACHINE, keyName);
            if (retVal != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("ERROR: Failed to unload registry hive.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
