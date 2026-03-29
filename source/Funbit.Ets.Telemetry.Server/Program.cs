using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Funbit.Ets.Telemetry.Server.Helpers;

namespace Funbit.Ets.Telemetry.Server
{
    static class Program
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateMutexA")]
        private static extern int CreateMutex(int lpMutexAttributes, int bInitialOwner, string lpName);
        [DllImport("kernel32.dll")]
        private static extern int GetLastError();
        private const int ErrorAlreadyExists = 183;

        public static bool UninstallMode;
        public static bool InternalMode;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            UninstallMode = args.Length >= 1 && args.Any(a => a.Trim() == "-uninstall");
            InternalMode = args.Length >= 1 && args.Any(a => a.Trim() == "-internal");
            if (!InternalMode)
            {
                var internalModeFromEnv = Environment.GetEnvironmentVariable("ANDESMP_INTERNAL_MODE");
                InternalMode = string.Equals(internalModeFromEnv, "1", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(internalModeFromEnv, "true", StringComparison.OrdinalIgnoreCase);
            }

            // check if another instance is running
            CreateMutex(0, -1,
                Uac.IsProcessElevated()
                    ? "Ets2Telemetry_8F63CCBE353DE22BD1A86308AD675001_UAC"
                    : "Ets2Telemetry_8F63CCBE353DE22BD1A86308AD675001");
            bool bAnotherInstanceRunning = GetLastError() == ErrorAlreadyExists;
            if (bAnotherInstanceRunning)
            {
                if (!InternalMode)
                {
                    MessageBox.Show(@"Another ETS2/ATS Telemetry Server instance is already running!", @"Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return;
            }

            log4net.Config.XmlConfigurator.Configure();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
