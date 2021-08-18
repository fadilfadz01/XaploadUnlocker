using System.Diagnostics;
using System.IO;

namespace Xapload_Unlocker
{
    class ApplyReg
    {
        public static void ApplyRegEntries()
        {
            string TempPath = Path.GetTempPath();
            string RegFile = TempPath + "\\UnlockEntries.reg";
            while (!File.Exists(RegFile))
            {
                System.Threading.Thread.Sleep(1000);
            }
            Process regeditProcess = Process.Start("regedit.exe", "/s \"" + RegFile + "\"");
            regeditProcess.WaitForExit();
        }
    }
}
