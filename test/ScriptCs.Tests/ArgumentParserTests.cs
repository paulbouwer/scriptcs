﻿using Moq;
using ScriptCs.Argument;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ArgumentParserTests
    {        
        public class ParseMethod
        {
            [Fact]
            public void ShouldHandleCommandLineArguments()
            {
                string[] args = { "server.csx", "-log", "error" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldEqual("server.csx");
                result.LogLevel.ShouldEqual(LogLevel.Error);
            }

            [Fact]
            public void ShouldHandleEmptyArray()
            {
                var parser = new ArgumentParser();
                var result = parser.Parse(new string[0]);

                result.ShouldNotBeNull();
                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldBeNull();
                result.Config.ShouldEqual(Constants.ConfigFilename);
            }

            [Fact]
            public void ShouldHandleNull()
            {
                var parser = new ArgumentParser();
                var result = parser.Parse(null);

                result.ShouldNotBeNull();
                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldBeNull();
                result.Config.ShouldEqual(Constants.ConfigFilename);
            }

            [Fact]
            public void ShouldSupportHelp() 
            {
                string[] args = { "-help" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.ShouldNotBeNull();
                result.ScriptName.ShouldBeNull();
                result.Help.ShouldBeTrue();
                result.LogLevel.ShouldBeNull();
            }

            [Fact]
            public void ShouldGoIntoReplIfOnlyLogLevelIsSet()
            {
                string[] args = { "-loglevel", "debug" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldGoIntoReplIfOnlyLogIsSet()
            {
                string[] args = { "-log", "debug" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.Repl.ShouldBeTrue();
                result.LogLevel.ShouldEqual(LogLevel.Debug);
            }

            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberFollowsPackageToInstallName()
            {
                string[] args = { "-install", "glimpse.scriptcs", "1.0.1" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.PackageVersion.ShouldEqual("1.0.1");
                result.Install.ShouldEqual("glimpse.scriptcs");
            }

            [Fact]
            public void ShouldSetVersionIfPackageVersionNumberSpecifiedExplicitly()
            {
                string[] args = { "-install", "glimpse.scriptcs", "-packageversion", "1.0.1" };

                var parser = new ArgumentParser();
                var result = parser.Parse(args);

                result.PackageVersion.ShouldEqual("1.0.1");
                result.Install.ShouldEqual("glimpse.scriptcs");
            }
        }
    }
}