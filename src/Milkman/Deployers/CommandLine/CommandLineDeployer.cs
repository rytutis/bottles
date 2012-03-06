using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.CommandLine
{
    public class CommandLineDeployer : IDeployer<CommandLineExecution>
    {
        private readonly IProcessRunner _processRunner;

        public CommandLineDeployer(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public void Execute(CommandLineExecution directive, HostManifest host, IPackageLog log)
        {
            var processStartInfo = GetProcessStartInfo(directive);

            log.Trace("Executing the command '{0}' with args '{1}'", directive.FileName, directive.Arguments);
            ProcessReturn rtnVal = null;
            try
            {
                rtnVal = _processRunner.Run(processStartInfo, new TimeSpan(0, 0, directive.TimeoutInSeconds));
                log.Trace("Command completed with exit code '{0}'", rtnVal.ExitCode);
                log.Trace(rtnVal.OutputText);
                if (rtnVal.ExitCode != 0)
                {
                    log.MarkFailure(
                        "Because the process returned a non-zero number (a '{0}' in this case)".ToFormat(rtnVal.ExitCode));
                }
            }
            catch (Exception ex)
            {
                if (rtnVal != null)
                {
                    rtnVal.OutputText.SplitOnNewLine().Each(l => log.Trace(l));
                }

                log.MarkFailure(ex);
            }          
        }

        public string GetDescription(CommandLineExecution directive)
        {
            return "Execute '{0}'".ToFormat(directive);
        }


        public ProcessStartInfo GetProcessStartInfo(CommandLineExecution directive)
        {
            var fileName = directive.FileName;
            if (!Path.IsPathRooted(fileName))
            {
                fileName = directive.WorkingDirectory.AppendPath(directive.FileName);
            }

            return new ProcessStartInfo{
                FileName = fileName,
                Arguments = directive.Arguments,
                WorkingDirectory = directive.WorkingDirectory,
                ErrorDialog = false
                
            };
        }
    }
}