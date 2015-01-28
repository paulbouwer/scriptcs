﻿using System;
using System.Linq;
using PowerArgs;

namespace ScriptCs.Argument
{
    public class ArgumentParser : IArgumentParser
    {
        public ScriptCsArgs Parse(string[] args)
        {
            //no args initialized REPL
            if (args == null || args.Length <= 0) 
                return new ScriptCsArgs { Repl = true };

            var installArgPosition = Array.FindIndex(args, x => x.ToLowerInvariant() == "-install");
            string packageVersion = null;
            if (installArgPosition == 0 && args.Length > 2 && !args[installArgPosition + 2].StartsWith("-"))
            {
                packageVersion = args[installArgPosition + 2];
                var argsList = args.ToList();
                argsList.RemoveAt(installArgPosition + 2);
                args = argsList.ToArray();
            }

            var scriptCsArgs = Args.Parse<ScriptCsArgs>(args);

            //if there is only 1 arg and it is a loglevel, it's also REPL
            if (args.Length == 2 && args.Any(x => x.ToLowerInvariant() == "-loglevel" || x.ToLowerInvariant() == "-log"))
            {
                scriptCsArgs.Repl = true;
            }

            if (!string.IsNullOrWhiteSpace(packageVersion))
                scriptCsArgs.PackageVersion = packageVersion;

            return scriptCsArgs;
        }
    }
}