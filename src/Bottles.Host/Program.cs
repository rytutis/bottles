﻿using System;
using System.IO;
using Bottles.Exploding;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;
using Topshelf;

namespace Bottles.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            setupLog4Net();

            HostFactory.Run(h =>
            {
                h.SetDescription("Bottle Host");
                h.SetServiceName("bottle-host");
                h.SetDisplayName("display");

                h.Service<BottleHost>(c =>
                {
                    c.ConstructUsing(n =>
                    {
                        var fileSystem = new FileSystem();
                        var packageExploder = new PackageExploder(new ZipFileService(fileSystem),
                                                                  new PackageExploderLogger(ConsoleWriter.Write),
                                                                  fileSystem);
                        return new BottleHost(packageExploder, fileSystem);
                    });
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                });
            });
        }

        static void setupLog4Net()
        {
            var fileName = getFileName();
            var fs = new FileSystem();
            var configFolder = fs.SearchUpForDirectory(System.Environment.CurrentDirectory, "config") ?? ".";
            var log4NetFilePath = configFolder.AppendPath(fileName);
            Console.WriteLine("Using '{0}' for log4net.config", log4NetFilePath);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetFilePath));
        }

        static string getFileName()
        {
            try
            {
                var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                var query = "SELECT * FROM Win32_Services where ProcessID = " + processId;
                var searcher = new System.Management.ManagementObjectSearcher(query);
                foreach (var obj in searcher.Get())
                {
                    return obj["Name"].ToString() + "log4net.config";
                }

            }
            catch (Exception)
            {

                //swallow
            }

            return "log4net.config";
        }
    }
}
