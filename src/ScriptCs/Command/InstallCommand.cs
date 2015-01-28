﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class InstallCommand : IInstallCommand
    {
        private readonly string _name;
        private readonly string _version;
        private readonly bool _allowPre;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IPackageInstaller _packageInstaller;
        private readonly ILog _logger;
        private readonly IFileSystemMigrator _fileSystemMigrator;

        public InstallCommand(
            string name,
            string version,
            bool allowPre,
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IPackageInstaller packageInstaller,
            ILog logger,
            IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _name = name;
            _version = version ?? string.Empty;
            _allowPre = allowPre;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
            _logger = logger;
            _fileSystemMigrator = fileSystemMigrator;
        }

        public CommandResult Execute()
        {
            _fileSystemMigrator.Migrate();

            _logger.Info("Installing packages...");
            var packages = GetPackages(_fileSystem.CurrentDirectory);
            try
            {
                _packageInstaller.InstallPackages(packages, _allowPre);
                _logger.Info("Package installation succeeded.");
                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Package installation failed: {0}.", ex, ex.Message);
                return CommandResult.Error;
            }
        }

        private IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                var packages = _packageAssemblyResolver.GetPackages(workingDirectory);
                foreach (var packageReference in packages)
                {
                    yield return packageReference;
                }

                yield break;
            }

            yield return new PackageReference(_name, new FrameworkName(".NETFramework,Version=v4.0"), _version);
        }
    }
}