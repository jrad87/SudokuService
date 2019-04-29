using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using Solver;


namespace WindowsService1
{
    public enum ServiceState {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }

    public partial class MyNewService : ServiceBase
    {
        private int eventId = 1;
        private Queue<String> fileQueue;
        private bool processingQueue;
        public MyNewService()
        {
            InitializeComponent();
            fileQueue = new Queue<string>();
            processingQueue = false;
            eventLog1 = new System.Diagnostics.EventLog();
            if(! System.Diagnostics.EventLog.SourceExists("MySource")) {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }

            fileSystemWatcher1 = new FileSystemWatcher("C:\\Users\\Laptop\\Desktop\\Sudoku");
            fileSystemWatcher1.IncludeSubdirectories = true;
            fileSystemWatcher1.NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.Attributes;
            fileSystemWatcher1.Filter = "*.txt";

            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        private void ProcessQueue()
        {
            while(fileQueue.Count > 0)
            {
                try
                {
                    string path = fileQueue.Dequeue();
                    try
                    {
                        IEnumerable<string> lines = File.ReadLines(path).DefaultIfEmpty();
                        eventLog1.WriteEntry("Change Detected", EventLogEntryType.Information, eventId++);
                        eventLog1.WriteEntry(lines.First());
                        if (lines.First() == "SUDOKU")
                        {
                            string solvedPuzzle = string.Join("", RecursiveBacktrackingSolver.solve(string.Join("", lines.Skip(1)).ToCharArray()));
                            string newFilePath = Path.GetDirectoryName(path) + @"\_" + Path.GetFileName(path);
                            if (!File.Exists(newFilePath))
                            {
                                File.WriteAllLines(newFilePath, Enumerable.Range(0, 9).Select(row => solvedPuzzle.Substring(9 * row, 9)).ToArray());
                                Task t = Task.Run(async delegate
                                {
                                    await Task.Delay(200);
                                    File.Delete(path);
                                    File.Move(newFilePath, path);
                                });
                            }
                            eventLog1.WriteEntry(
                                string.Format("Found Sudoku Puzzle {0} {1}", solvedPuzzle, newFilePath),
                                EventLogEntryType.Information,
                                eventId++
                            );

                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        fileQueue.Enqueue(path);
                    }
                }
                catch (Exception eLines)
                {
                    eventLog1.WriteEntry(String.Format("{0}, {1}", eLines.Message, eLines.GetType()), EventLogEntryType.Error, eventId++);
                }
            }
            processingQueue = false;
        }

        private void WatchForSudoku(object sender, System.IO.FileSystemEventArgs e) {
            if (processingQueue)
            {
                fileQueue.Enqueue(e.FullPath);
            }
            else
            {
                fileQueue.Enqueue(e.FullPath);
                processingQueue = true;
                ProcessQueue();
            }
        }

        private void FileError(object sender, ErrorEventArgs e) {
            eventLog1.WriteEntry(string.Format("File error {0}", e.ToString()), EventLogEntryType.Error, eventId++);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public void OnTimer(object sender, ElapsedEventArgs e) {
            eventLog1.WriteEntry(
                "Monitoring the system",
                EventLogEntryType.Information,
                eventId++
                );
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart Call");
            
            //Set service status to Start pending
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //
            fileSystemWatcher1.Changed += WatchForSudoku;
            fileSystemWatcher1.Error += FileError;
            fileSystemWatcher1.EnableRaisingEvents = true;
            
            // Now Set service status to running
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");

            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnContinue() {
            eventLog1.WriteEntry("In OnContinue");
        }
    }
}
