using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

using WinTerMul.Common.Kernel32;

namespace WinTerMul.Terminal
{
    internal class ProcessService : IDisposable
    {
        private readonly IKernel32Api _kernel32Api;
        private readonly InputArguments _inputArguments;

        private Process _process;
        private bool _shouldClose;

        public ProcessService(IKernel32Api kernel32Api, InputArguments inputArguments)
        {
            _kernel32Api = kernel32Api ?? throw new ArgumentNullException(nameof(kernel32Api));
            _inputArguments = inputArguments ?? throw new ArgumentNullException(nameof(inputArguments));
        }

        public void StartNewTerminal()
        {
            if (_process != null)
            {
                throw new InvalidOperationException("Can only start one terminal.");
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            _process.Start();

            _kernel32Api.FreeConsole();
            _kernel32Api.AttachConsole(_process.Id);
        }

        public void CloseTerminal()
        {
            _shouldClose = true;
        }

        public bool ShouldClose()
        {
            if (_process == null)
            {
                throw new InvalidOperationException("Terminal has been created.");
            }

            if (_shouldClose || _process.HasExited || IsProcessDead(_inputArguments.ParentProcessId))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (_process != null)
            {
                KillAllChildProcesses(_process.Id);
                _process.Dispose();
                _process = null;
            }
        }

        private static bool IsProcessDead(int id)
        {
            return Process.GetProcesses().All(x => x.Id != id);
        }

        private static void KillAllChildProcesses(int id)
        {
            if (id == 0)
            {
                return;
            }

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessID=" + id);

            var processes = searcher.Get();
            if (processes != null)
            {
                foreach (var process in processes)
                {
                    KillAllChildProcesses(Convert.ToInt32(process["ProcessID"]));
                }
            }

            try
            {
                var process = Process.GetProcessById(id);
                if (process != null)
                {
                    process.Kill();
                }
            }
            catch (ArgumentException)
            {
                // The process is not running or the identifier might be expired.
            }
        }
    }
}
