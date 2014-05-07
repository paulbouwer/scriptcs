﻿using ScriptCs.Hosting;
using System.IO;

namespace ScriptCs.Command
{
    public class CommandFactory
    {
        private readonly IScriptServicesBuilder _scriptServicesBuilder;
        private readonly IInitializationServices _initializationServices;

        public CommandFactory(IScriptServicesBuilder scriptServicesBuilder)
        {
            _scriptServicesBuilder = scriptServicesBuilder;
            _initializationServices = _scriptServicesBuilder.InitializationServices;
        }

        public ICommand CreateCommand(ScriptCsArgs args, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            var fileSystem = _initializationServices.GetFileSystem();
            var logger = _initializationServices.Logger;
            var packageAssemblyResolver = _initializationServices.GetPackageAssemblyResolver();
            ScriptServices scriptServices = null;

            if (args.Help)
            {
                return new ShowUsageCommand(logger, isValid: true);
            }

            if (args.Global)
            {
                var currentDir = fileSystem.ModulesFolder;
                if (!fileSystem.DirectoryExists(currentDir))
                {
                    fileSystem.CreateDirectory(currentDir);
                }

                fileSystem.CurrentDirectory = currentDir;
            }

            _initializationServices.GetInstallationProvider().Initialize();

            if (args.Repl)
            {
                scriptServices = _scriptServicesBuilder.Build();
                var replCommand = new ExecuteReplCommand(
                    args.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.ScriptPackResolver,
                    scriptServices.Engine,
                    scriptServices.FilePreProcessor,
                    scriptServices.ObjectSerializer,
                    scriptServices.Logger,
                    scriptServices.Console,
                    scriptServices.AssemblyResolver);

                return replCommand;
            }

            if (args.ScriptName != null)
            {
                scriptServices = _scriptServicesBuilder.Build();
                var executeCommand = new ExecuteScriptCommand(
                    args.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.Executor,
                    scriptServices.ScriptPackResolver,
                    scriptServices.Logger,
                    scriptServices.AssemblyResolver);

                var currentDirectory = fileSystem.CurrentDirectory;
                var packageFile = Path.Combine(currentDirectory, Constants.PackagesFile);
                var packagesFolder = Path.Combine(currentDirectory, Constants.PackagesFolder);

                if (fileSystem.FileExists(packageFile) && !fileSystem.DirectoryExists(packagesFolder))
                {
                    var installCommand = new InstallCommand(
                        null,
                        null,
                        false,
                        fileSystem,
                        scriptServices.PackageAssemblyResolver,
                        scriptServices.PackageInstaller,
                        scriptServices.Logger);

                    return new CompositeCommand(installCommand, executeCommand);
                }

                return executeCommand;
            }

            if (args.Clean)
            {
                var saveCommand = new SaveCommand(packageAssemblyResolver, logger);

                if (args.Global)
                {
                    var currentDirectory = fileSystem.ModulesFolder;
                    fileSystem.CurrentDirectory = currentDirectory;
                    if (!fileSystem.DirectoryExists(currentDirectory))
                    {
                        fileSystem.CreateDirectory(currentDirectory);
                    }
                }

                var cleanCommand = new CleanCommand(
                    args.ScriptName,
                    fileSystem,
                    logger);

                return new CompositeCommand(saveCommand, cleanCommand);
            }

            if (args.Save)
            {
                return new SaveCommand(packageAssemblyResolver, logger);
            }

            if (args.Version)
            {
                return new VersionCommand(_scriptServicesBuilder.ConsoleInstance);
            }

            if (args.Install != null)
            {
                var installCommand = new InstallCommand(
                    args.Install,
                    args.PackageVersion,
                    args.AllowPreRelease,
                    fileSystem,
                    packageAssemblyResolver,
                    _initializationServices.GetPackageInstaller(),
                    logger);

                var saveCommand = new SaveCommand(packageAssemblyResolver, logger);

                return new CompositeCommand(installCommand, saveCommand);
            }

            return new ShowUsageCommand(logger, isValid: false);
        }
    }
}