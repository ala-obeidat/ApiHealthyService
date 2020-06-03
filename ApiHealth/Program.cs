using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiHealth
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //new ApiHealth().StartingService();
            //while (true)
            //{
            //    Thread.Sleep(200);
            //}
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ApiHealth()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
