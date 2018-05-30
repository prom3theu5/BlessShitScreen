using Bless_Screen_Fix.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Bless_Screen_Fix.Services
{
    public class BlessHelperService
    {
        private readonly string _blessExePath;
        private readonly List<Port> _netstatPorts;

        public BlessHelperService(string blessPath)
        {
            _blessExePath = $"{blessPath}\\Binaries\\Win64\\Bless_BE.exe";
            var exists = File.Exists(_blessExePath);

            if (!exists)
                throw new FileNotFoundException("File was not found. Please confirm your bless folder path is correct.", "Bless_BE.exe");
            
            _netstatPorts = new List<Port>();
        }

        public void Launch()
        {
            bool launchedSuccessfully = false;

            while (!launchedSuccessfully)
            {
                bool attemptMade = false;
                bool completedSuccessfully = false;

                var processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName == "Bless" || process.ProcessName == "Bless_BE" || process.ProcessName == "BlessLauncher")
                    {
                        process.Kill();
                    }
                }
                Process.Start(_blessExePath);
                GetNetStatPorts();

                while (!completedSuccessfully)
                {
                    int num = 0;
                    int num2 = 0;
                    GetNetStatPorts();
                    foreach (var port in _netstatPorts)
                    {
                        if (port.Process_name.Contains("Bless"))
                        {
                            if (port.Connection == "ESTABLISHED")
                                num++;
                            else
                                num2++;
                        }
                    }
                    if (num == 1 && num2 == 2)
                        attemptMade = true;
                    else
                    {
                        if (num == 2 && num2 == 2)
                        {
                            launchedSuccessfully = true;
                        }
                    }
                    if (attemptMade && num2 == 2 && num == 0)
                    {
                        completedSuccessfully = true;
                    }
                    Thread.Sleep(500);
                }
            }

        }

        private void GetNetStatPorts()
        {
            _netstatPorts.Clear();

               using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        Arguments = "-a -n -o",
                        FileName = "netstat.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    process.Start();
                    StreamReader standardOutput = process.StandardOutput;
                    StreamReader standardError = process.StandardError;
                    string input = standardOutput.ReadToEnd() + standardError.ReadToEnd();
                    string a = process.ExitCode.ToString();
                    if (a == "0")
                    {
                        string[] array = Regex.Split(input, "\r\n");
                        foreach (string input2 in array)
                        {
                            string[] array3 = Regex.Split(input2, "\\s+");
                            if (array3.Length > 4 && (array3[1].Equals("UDP") || array3[1].Equals("TCP")))
                            {
                                string text = Regex.Replace(array3[2], "\\[(.*?)\\]", "1.1.1.1");
                                _netstatPorts.Add(new Port
                                {
                                    Protocol = (text.Contains("1.1.1.1") ? string.Format("{0}v6", array3[1]) : string.Format("{0}v4", array3[1])),
                                    Port_number = text.Split(new char[]
                                    {
                                    ':'
                                    })[1],
                                    Process_name = ((array3[1] == "UDP") ? LookupProcess((int)Convert.ToInt16(array3[4])) : LookupProcess((int)Convert.ToInt16(array3[5]))),
                                    Connection = array3[4]
                                });
                            }
                        }
                    }
                }
            }

        private string LookupProcess(int pid)
        {
            try
            {
                return Process.GetProcessById(pid).ProcessName;
            }
            catch (Exception)
            {
                return "-";
            }
        }
    }
}