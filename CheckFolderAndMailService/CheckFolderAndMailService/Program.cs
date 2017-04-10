using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CheckFolderAndMailService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            FolderMailerService DebuggingService = new FolderMailerService();
            DebuggingService.onDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new FolderService()
            };
            ServicesToRun = new ServiceBase[]
           {
                new FolderMailerService()
           };
            ServiceBase.Run(ServicesToRun);
#endif
        }

    }

    }
