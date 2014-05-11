﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using ScriptCs.Argument;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using System.Runtime.CompilerServices;

namespace ScriptCs
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            SetProfile();

            IConsole console = new ScriptConsole();

            var parser = new ArgumentHandler(new ArgumentParser(console), new ConfigFileParser(console), new FileSystem());
            var arguments = parser.Parse(args);

            var commandArgs = arguments.CommandArguments;
            var scriptArgs = arguments.ScriptArguments;

            if (!string.IsNullOrWhiteSpace(commandArgs.Output))
            {
                console = new FileConsole(commandArgs.Output, console);
            }

            var configurator = new LoggerConfigurator(commandArgs.LogLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger)
                .Cache(commandArgs.Cache)
                .Debug(commandArgs.Debug)
                .LogLevel(commandArgs.LogLevel)
                .ScriptName(commandArgs.ScriptName)
                .Repl(commandArgs.Repl);

            var modules = GetModuleList(commandArgs.Modules);
            var extension = Path.GetExtension(commandArgs.ScriptName);


            if (string.IsNullOrWhiteSpace(extension) && !commandArgs.Repl)
            {
                // No extension was given, i.e we might have something like
                // "scriptcs foo" to deal with. We activate the default extension,
                // to make sure it's given to the LoadModules below.
                extension = ".csx";

                if (!string.IsNullOrWhiteSpace(commandArgs.ScriptName))
                {
                    // If the was in fact a script specified, we'll extend it
                    // with the default extension, assuming the user giving
                    // "scriptcs foo" actually meant "scriptcs foo.csx". We
                    // perform no validation here thought; let it be done by
                    // the activated command. If the file don't exist, it's
                    // up to the command to detect and report.

                    commandArgs.ScriptName += extension;
                }
            }

            scriptServicesBuilder.LoadModules(extension, modules);

            var commandFactory = new CommandFactory(scriptServicesBuilder);
            var command = commandFactory.CreateCommand(commandArgs, scriptArgs);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }

        private static string[] GetModuleList(string modulesArg)
        {
            var modules = new string[0];

            if (modulesArg != null)
            {
                modules = modulesArg.Split(',');
            }

            return modules;
        }

        private static void SetProfile()
        {
            var profileOptimizationType = Type.GetType("System.Runtime.ProfileOptimization");
            if (profileOptimizationType != null)
            {
                var setProfileRoot = profileOptimizationType.GetMethod("SetProfileRoot", BindingFlags.Public | BindingFlags.Static);
                setProfileRoot.Invoke(null, new object[] {typeof (Program).Assembly.Location});

                var startProfile = profileOptimizationType.GetMethod("StartProfile", BindingFlags.Public | BindingFlags.Static);
                startProfile.Invoke(null, new object[] {typeof (Program).Assembly.GetName().Name + ".profile"});
            }
        }
    }
}