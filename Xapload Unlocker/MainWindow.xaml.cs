using Microsoft.Win32;
using ProcessPrivileges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xapload_Unlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Int32 RegLoadKey(IntPtr hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Int32 RegUnLoadKey(IntPtr hKey, string lpSubKey);

        [DllImport("kernel32.dll")]
        static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

        public string[] Drive = new string[2];
        string[] PhysicalDrive = new string[2];
        string[] VolumeLabel = new string[2];
        bool result = false;
        string registryFile;
        bool isDeveloperUnlocked = false;
        bool isInteropUnlocked = false;
        bool flag = false;
        Process process = new Process();

        public MainWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            _ = GetDevices();
            GetPriviledge();
        }

        private async Task GetDevices()
        {
            await Task.Run(() =>
            {
                try
                {
                    while (!result)
                    {
                        int i = 0;
                        foreach (ManagementObject logical in new ManagementObjectSearcher("select * from Win32_LogicalDisk").Get())
                        {
                            string Label = string.Empty;
                            foreach (ManagementObject partition in logical.GetRelated("Win32_DiskPartition"))
                            {
                                foreach (ManagementObject drive in partition.GetRelated("Win32_DiskDrive"))
                                {
                                    if (drive["PNPDeviceID"].ToString().Contains("VEN_QUALCOMM&PROD_MMC_STORAGE") || drive["PNPDeviceID"].ToString().Contains("VEN_MSFT&PROD_PHONE_MMC_STOR") || drive["PNPDeviceID"].ToString().Contains("VEN_MSFT&PROD_VIRTUAL_DISK") || drive["PNPDeviceID"].ToString().Contains("VEN_PASSMARK&PROD_OSFDISK"))
                                    {
                                        Label = logical["VolumeName"] == null ? "" : logical["VolumeName"].ToString();
                                        if ((Drive[i] == null) || string.Equals(Label, "MainOS", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            Drive[i] = logical["Name"].ToString();
                                            PhysicalDrive[i] = drive["DeviceID"].ToString();
                                            VolumeLabel[i] = Label;
                                            Dispatcher.Invoke(() =>
                                            {
                                                DriveCombo.Items.Add($"{VolumeLabel[i]} ({Drive[i]})");
                                            });
                                            i++;
                                        }
                                        if (string.Equals(Label, "MainOS", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            result = true;
                                            break;
                                        }
                                    }
                                }
                                if (string.Equals(Label, "MainOS", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        DeviceBlock.Text = $"Mass Storage Mode connected: ";
                        DriveCombo.SelectedIndex = 0;
                        DriveCombo.Visibility = Visibility.Visible;
                        UnlockBtn.IsEnabled = true;
                        CheckSupport();
                    });
                }
                catch { }
            });
        }

        private async void CheckSupport()
        {
            if (File.Exists($"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\OEMsettings.reg"))
            {
                registryFile = $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\OEMsettings.reg";
            }
            else if (File.Exists($"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\SOFTWARE.reg"))
            {
                registryFile = $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\SOFTWARE.reg";
            }
            else
            {
                PermUnlockBox.IsEnabled = false;
                return;
            }
            await DecompressGzip(System.IO.Path.GetTempPath());
            IsPermUnlocked();
        }

        private void IsPermUnlocked()
        {
            string regFileEntries = File.ReadAllText($"{System.IO.Path.GetTempPath()}{System.IO.Path.GetFileNameWithoutExtension(registryFile)}");
            if (regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\AppModelUnlock", StringComparison.OrdinalIgnoreCase) >= 0 && regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\System\controlset001\Control\CI", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isDeveloperUnlocked = true;
                isInteropUnlocked = true;
                PermUnlockBox.IsEnabled = false;
                PermUnlockBox.IsChecked = true;
            }
            else if (regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\AppModelUnlock", StringComparison.OrdinalIgnoreCase) >= 0 && regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\System\controlset001\Control\CI", StringComparison.OrdinalIgnoreCase) < 0)
            {
                isDeveloperUnlocked = true;
                isInteropUnlocked = false;
                if (UnlockTypeCombo.SelectedIndex == 0)
                {
                    PermUnlockBox.IsEnabled = false;
                    PermUnlockBox.IsChecked = true;
                }
                else
                {
                    PermUnlockBox.IsEnabled = true;
                    PermUnlockBox.IsChecked = false;
                }
            }
            else if (regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\AppModelUnlock", StringComparison.OrdinalIgnoreCase) < 0 && regFileEntries.IndexOf(@"HKEY_LOCAL_MACHINE\System\controlset001\Control\CI", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isDeveloperUnlocked = false;
                isInteropUnlocked = true;
                if (UnlockTypeCombo.SelectedIndex == 1)
                {
                    PermUnlockBox.IsEnabled = false;
                    PermUnlockBox.IsChecked = true;
                }
                else
                {
                    PermUnlockBox.IsEnabled = true;
                    PermUnlockBox.IsChecked = false;
                }
            }
            else
            {
                isDeveloperUnlocked = false;
                isInteropUnlocked = false;
                PermUnlockBox.IsEnabled = true;
                PermUnlockBox.IsChecked = false;
            }
            flag = true;
        }

        private void DriveCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
            {
                UnlockBtn.IsEnabled = true;
                CheckSupport();
            }
        }

        private void UnlockTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
            {
                IsPermUnlocked();
            }
        }

        private async void UnlockBtn_Click(object sender, RoutedEventArgs e)
        {
            UnlockText.Visibility = Visibility.Visible;
            UnlockBtn.Content = "Unlocking";
            UnlockBtn.IsEnabled = false;
            await Task.Delay(1);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Dispatcher.Invoke(() =>
                {
                    GetResourceFile("Developer Unlock.reg", 1);
                    GetResourceFile("Developer Unlock.txt", 1);
                    GetResourceFile("Interop Unlock.reg", 1);
                    GetResourceFile("Interop Unlock.txt", 1);
                    if (RegLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSOFTWARE", $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\System32\\Config\\SOFTWARE") != 0 || RegLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSYSTEM", $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\System32\\Config\\SYSTEM") != 0)
                    {
                        RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSOFTWARE");
                        RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSYSTEM");
                        MessageBox.Show("Failed to load the registry hive.", "ERROR");
                        UnlockBtn.IsEnabled = true;
                        return;
                    }
                    if (UnlockTypeCombo.SelectedIndex == 0)
                    {
                        DeveloperUnlock();
                    }
                    else if (UnlockTypeCombo.SelectedIndex == 1)
                    {
                        InteropUnlock();
                    }
                    else
                    {
                        DeveloperUnlock();
                        InteropUnlock();
                    }
                    if (RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSOFTWARE") != 0 || RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSYSTEM") != 0)
                    {
                        MessageBox.Show("Failed to unload the registry hive.", "ERROR");
                        UnlockBtn.IsEnabled = true;
                        return;
                    }
                    if (PermUnlockBox.IsChecked == true)
                    {
                        PermanentUnlock();
                    }
                });
            }).Start();
            UnlockText.Visibility = Visibility.Collapsed;
            UnlockBtn.Content = "Unlocked";
        }

        private void DeveloperUnlock()
        {
            try
            {
                process.StartInfo.FileName = "Reg.exe";
                process.StartInfo.Arguments = $"import \"{System.IO.Path.GetTempPath()}Developer Unlock.reg\"";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSOFTWARE");
                RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSYSTEM");
                MessageBox.Show("Failed to unlock the device.", "ERROR");
                UnlockBtn.IsEnabled = true;
                return;
            }
        }

        private void InteropUnlock()
        {
            try
            {
                process.StartInfo.FileName = "Reg.exe";
                process.StartInfo.Arguments = $"import \"{System.IO.Path.GetTempPath()}Interop Unlock.reg\"";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSOFTWARE");
                RegUnLoadKey(new IntPtr(unchecked((int)0x80000002)), "LSYSTEM");
                MessageBox.Show("Failed to unlock the device.", "ERROR");
                UnlockBtn.IsEnabled = true;
                return;
            }
        }

        private void PermanentUnlock()
        {
            GetResourceFile("7za.exe", 1);
            bool winPhone8 = false;
            if (MimeTypes.GetContentType(registryFile) == "application/x-gzip-compressed")
            {
                SetFileAttributes(registryFile, 128);
                File.Move(registryFile, $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\RegistryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}.gz");
                process.StartInfo.FileName = $"7za.exe";
                process.StartInfo.Arguments = $"e \"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}.gz\" -aoa -o\"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\\"";
                process.StartInfo.WorkingDirectory = $"{System.IO.Path.GetTempPath()}";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            else
            {
                winPhone8 = true;
                File.Move($"{registryFile}", $"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}");
            }
            if ((UnlockTypeCombo.SelectedIndex == 0 || UnlockTypeCombo.SelectedIndex == 2) && !isDeveloperUnlocked)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/u /c type \"{System.IO.Path.GetTempPath()}Developer Unlock.txt\" >> \"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}\"";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            if ((UnlockTypeCombo.SelectedIndex == 1 || UnlockTypeCombo.SelectedIndex == 2) && !isInteropUnlocked)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/u /c type \"{System.IO.Path.GetTempPath()}Interop Unlock.txt\" >> \"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}\"";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            if (winPhone8)
            {
                File.Move($"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}", $"{registryFile}");
            }
            else
            {
                process.StartInfo.FileName = $"7za.exe";
                process.StartInfo.Arguments = $"u \"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}.gz\" \"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}\"";
                process.StartInfo.WorkingDirectory = $"{System.IO.Path.GetTempPath()}";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                File.Move($"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}.gz", registryFile);
                SetFileAttributes(registryFile, 4);
                File.Delete($"{Drive[DriveCombo.SelectedIndex]}\\Windows\\Packages\\registryFiles\\{System.IO.Path.GetFileNameWithoutExtension(registryFile)}");
            }
        }

        private async Task DecompressGzip(string path)
        {
            using (var inputFileStream = new FileStream(registryFile, FileMode.Open))
            using (var gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
            using (var outputFileStream = new FileStream($"{path}{System.IO.Path.GetFileNameWithoutExtension(registryFile)}", FileMode.Create))
            {
                await gzipStream.CopyToAsync(outputFileStream);
            }
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Xapload Unlocker\nVersion {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\nDeveloped by Fadil Fadz (fadilfadz01)\nA tool to unlock the app sideloading on Windows Phone having unlocked bootloader.\n\nCopyright © 2023\n\nhttps://github.com/fadilfadz01/XaploadUnlocker", "About");
        }

        private void GetPriviledge()
        {
            try
            {
                using (AccessTokenHandle accessTokenHandle =
                    Process.GetCurrentProcess().GetAccessTokenHandle(
                        TokenAccessRights.AdjustPrivileges | TokenAccessRights.Query))
                {
                    AdjustPrivilegeResult backupResult = accessTokenHandle.EnablePrivilege(Privilege.Backup);
                    AdjustPrivilegeResult restoreResult = accessTokenHandle.EnablePrivilege(Privilege.Restore);
                }
            }
            catch (Exception ex) { }
        }

        public static string GetResourceFile(string resourceName, int dump)
        {
            var embeddedResource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(s => s.Contains(resourceName)).ToArray();
            if (!string.IsNullOrWhiteSpace(embeddedResource[0]))
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResource[0]))
                {
                    if (dump == 0)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            return result;
                        }
                    }
                    else
                    {
                        var data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        File.WriteAllBytes($@"{System.IO.Path.GetTempPath()}{resourceName}", data);
                    }
                }
            }
            return null;
        }
    }
}
