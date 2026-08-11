using CasseiaCoreX.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CasseiaCoreX.Models
{
    public static class AndroidDebug
    {
        public static string AdbPath
        {
            // ADB 文件路径
            get
            {
                return Path.Combine(App.Root, "Assets", "ADB", "platform-tools", "adb.exe");
            }
        }
        public static async void InitAdbProcess()
        {
            // 初始化 ADB 进程
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = "start-server";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(pi);
            p.WaitForExit();
            p.StandardOutput.ReadToEnd();
            return;
        }

        public static void GetDevices(out List<string> devices)
        {
            // 获取连接的设备
            devices = new List<string>();
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = "devices";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(pi);
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd();
            string[] lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.ToString().Contains("List of devices attached"))
                {
                    continue;
                }
                if (line.ToString().Contains("device"))
                {
                    string id = line.ToString().Split('\t')[0];
                    devices.Add(id);
                }
            }
            return;
        }

        public static bool PairDevices(string host, string port, string pairCode, out string output)
        {
            // 使用配对码连接设备
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = $"pair {host}:{port} {pairCode}";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(pi);
            string rec = string.Empty;
            p.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    rec += e.Data + "\n";
                }
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    rec += e.Data + "\n";
                }
            };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            if (!p.WaitForExit(10000))
            {
                p.Kill();
                output = "配对超时，命令行输出如下\n" + rec;
                return false;
            }
            output = rec;
            return (p.ExitCode == 0 || output.Contains("Successfully paired to"));

        }

        public static bool ConnectDevices(string host, string port, out string output)
        {
            // 连接设备
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = $"connect {host}:{port}";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(pi);
            string rec = string.Empty;
            p.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    rec += e.Data + "\n";
                }
            };
            p.BeginOutputReadLine();
            
            if (!p.WaitForExit(10000))
            {
                p.Kill();
                output = "连接超时，命令输出如下\n" + rec;
                return false;
            }
            output = rec;
            return (p.ExitCode == 0 || output.Contains("connected to"));
        }

        public static bool KillAdbProcess(out string errorMessage)
        {
            // 结束所有 ADB 进程
            errorMessage = null;
            try
            {
                var adbProcesses = System.Diagnostics.Process.GetProcessesByName("adb");
                foreach (var process in adbProcesses)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return errorMessage == null;
        }

        public static bool ScreenShot(string deviceId, out string errorMessage, out string filePath, out string fileName)
        {
            // 截图
            errorMessage = null;
            fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            filePath = Path.Combine(App.Root, "Assets", "Screenshots", fileName);
            Directory.CreateDirectory(Path.Combine(App.Root, "Assets", "Screenshots"));

            try
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = "cmd.exe";
                pi.Arguments = $"/c {AdbPath} -s {deviceId} shell screencap \"/sdcard/{fileName}\"&&{AdbPath} -s {deviceId} pull \"/sdcard/{fileName}\" {filePath}&&{AdbPath} -s {deviceId} shell rm \"/sdcard/{fileName}\"&&exit";
                pi.RedirectStandardInput = true;
                pi.RedirectStandardOutput = true;
                pi.RedirectStandardError = true;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(pi);
                errorMessage = p.StandardError.ReadToEnd();
                p.StandardOutput.ReadToEnd();
                p.StandardInput.Close();
                p.WaitForExit();
                if (p.ExitCode != 0 || !String.IsNullOrEmpty(errorMessage))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static bool InstallApp(string apkPath, string deviceId, string userId, out string output)
        {
            // 安装应用
            string uidArg = string.IsNullOrEmpty(userId) ? "" : $" --user {userId}";
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = AdbPath;
            pi.Arguments = $"-s {deviceId} install{uidArg} -r -t -d \"{apkPath}\"";
            pi.RedirectStandardInput = false;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;

            Process p = Process.Start(pi);
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            output = $"{AdbPath} {pi.Arguments}\n" + (string.IsNullOrEmpty(stderr) ? stdout : stderr);
            return p.ExitCode == 0;
        }

        
    }
}
