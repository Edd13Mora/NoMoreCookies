﻿using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.IO;

namespace NoMoreCookiesService
{
    public partial class MainService : ServiceBase
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lib);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr Module, string Function);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr ProcHandle, IntPtr BaseAddress, string Buffer, int size, int NumOfBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr ProcessHandle, IntPtr Address, int Size, uint AllocationType, uint Protection);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr ProcessHandle, IntPtr ThreadAttributes, uint StackSize, IntPtr StartAddress, IntPtr Parameter, uint CreationFlags, [Out] uint ThreadID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr Handle, uint TimeInMilli);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr ProcessHandle, IntPtr Address, int Size, uint FreeType);

        public MainService()
        {
            InitializeComponent();
        }

        public static unsafe int strlen(string s)
        {
            int length = 0;
            fixed (char* pStr = s)
            {
                length = *(((int*)pStr) - 1);
            }
            return length;
        }

        protected override void OnStart(string[] args)
        {
            string Config = File.ReadAllText(Environment.CurrentDirectory + "\\NoMoreConfig.txt");
            string DllPath = null;
            if (Config == "XMode: Disabled")
            {
                if (Environment.Is64BitProcess)
                {
                    DllPath = @"C:\NoMoreCookies_x64.dll";
                }
                else
                {
                    DllPath = @"C:\NoMoreCookies.dll";
                }
            }
            else if (Config == "XMode: Enabled")
            {
                if (Environment.Is64BitProcess)
                {
                    DllPath = @"C:\XNoMoreCookies.dll";
                }
                else
                {
                    DllPath = @"C:\XNoMoreCookies_x64.dll";
                }
            }
            if (DllPath != null)
            {
                foreach (Process ProcessInject in Process.GetProcesses())
                {
                    try
                    {
                        if (ProcessInject.Id != Process.GetCurrentProcess().Id)
                        {
                            IntPtr LoadLibraryA = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                            IntPtr Allocation = VirtualAllocEx(ProcessInject.Handle, IntPtr.Zero, strlen(DllPath), 0x00001000 | 0x00002000, 0x04);
                            WriteProcessMemory(ProcessInject.Handle, Allocation, DllPath, strlen(DllPath), 0);
                            IntPtr RemoteThread = CreateRemoteThread(ProcessInject.Handle, IntPtr.Zero, 0, LoadLibraryA, Allocation, 0, 0);
                            WaitForSingleObject(RemoteThread, 4000);
                            VirtualFreeEx(ProcessInject.Handle, Allocation, strlen(DllPath), 0x00008000);
                            CloseHandle(RemoteThread);
                            CloseHandle(ProcessInject.Handle);


                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            this.Stop();
        }

        protected override void OnStop()
        {

        }
    }
}