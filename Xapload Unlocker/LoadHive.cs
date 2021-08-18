using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ProcessPrivileges;

namespace Xapload_Unlocker
{
    class LoadHive
    {
        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(string text);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Int32 RegLoadKey(IntPtr hKey, string lpSubKey, string lpFile);

        public static void loadRegistryHive(string MainOS, string keyName)
        {
            string path = MainOS + "\\Windows\\System32\\Config\\SOFTWARE";
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                int retVal = RegLoadKey(HKEY_LOCAL_MACHINE, keyName, path);
                if (retVal != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("ERROR: Failed to load registry hive.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("ERROR: File does not exist in path.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static void adjustPriviledge()
        {
            try
            {
                // Access token handle reused within the using block.
                using (AccessTokenHandle accessTokenHandle =
                    Process.GetCurrentProcess().GetAccessTokenHandle(
                        TokenAccessRights.AdjustPrivileges | TokenAccessRights.Query))
                {
                    // Enable privileges using the same access token handle.
                    AdjustPrivilegeResult backupResult = accessTokenHandle.EnablePrivilege(Privilege.Backup);
                    AdjustPrivilegeResult restoreResult = accessTokenHandle.EnablePrivilege(Privilege.Restore);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(ex.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
