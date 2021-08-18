using System;
using System.IO;

namespace Xapload_Unlocker
{
    class XaploadUnlocker
    {
        static void Main(string[] args)
        {
            string MainOS;
            int flag;
            do
            {
                flag = 0;
                Console.SetWindowSize(75, 19);
                Console.SetBufferSize(75, 19);
                Console.Title = "Xapload Unlocker.exe";
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Xapload Unlocker for WINDOWS PHONE");
                Console.WriteLine("Version 1.1.4.0 beta");
                Console.WriteLine("By Fadil Fadz");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Enter MainOS partition: ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                MainOS = Console.ReadLine();
                Console.WriteLine("");
                if (MainOS.Length != 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("ERROR: Input a valid partition Letter.");
                    Console.ReadKey();
                }
                else if (MainOS[1] != ':')
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("ERROR: Input a valid partition Letter.");
                    Console.ReadKey();
                }
                else if (MainOS[2] != '\\')
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("ERROR: Input a valid partition Letter.");
                    Console.ReadKey();
                }
                else
                {
                    string[] Check = { "Data", "DPP", "EFIESP" };
                    for (int num = 0; num < 3; num++)
                    {
                        if (Directory.Exists(MainOS + "\\" + Check[num]) == false)
                        {
                            flag = 1;
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("ERROR: MainOS partition not found. Make sure your MainOS is " + MainOS.ToUpper());
                            Console.ReadKey();
                            break;
                        }
                    }
                }
            }
            while (MainOS.Length != 3 || MainOS[1] != ':' || MainOS[2] != '\\' || flag != 0);
            String registryFile;
            if (File.Exists(MainOS + "\\Windows\\Packages\\registryFiles\\OEMsettings.reg") == true)
            {
                registryFile = MainOS + "\\Windows\\Packages\\registryFiles\\OEMsettings.reg";
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Permanent Unlock: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Supported\n");
            }
            else if (File.Exists(MainOS + "\\Windows\\Packages\\registryFiles\\SOFTWARE.reg") == true)
            {
                registryFile = MainOS + "\\Windows\\Packages\\registryFiles\\SOFTWARE.reg";
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Permanent Unlock: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Supported till Windows 10 Mobile update\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Permanent Unlock: ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unsupported\n");
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Loading Hives . . .\n");
            LoadHive.adjustPriviledge();
            LoadHive.loadRegistryHive(MainOS, "LSOFTWARE");
            Console.WriteLine("Generating Entries . . .\n");
            GenerateEntries.GenerateRegEntriesAsync();
            Console.WriteLine("Applying Registry . . .\n");
            ApplyReg.ApplyRegEntries();
            Console.WriteLine("Unloading Hive . . .\n");
            UnloadHive.UnloadRegistryHive("LSOFTWARE");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("Done!!");
            Console.ReadKey();
        }

    }
}
