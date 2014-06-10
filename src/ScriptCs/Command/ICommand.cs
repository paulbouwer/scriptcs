﻿using System.Collections.Generic;

namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand
    {
        string[] ScriptArgs { get; }
    }

    public interface ISaveCommand : ICommand
    {
    }

    public interface ICleanCommand : ICommand
    {
    }

    public interface IInstallCommand : ICommand
    {
    }

    public interface IInvalidCommand : ICommand
    {
    }

    public interface IHelpCommand : ICommand
    {
    }

    public interface ICompositeCommand : ICommand
    {
        List<ICommand> Commands { get; }
    }

    public interface IVersionCommand : ICommand
    {
    }

    public interface IDeferredCreationCommand<TCommand> : ICommand where TCommand : ICommand
    {
    }

    public interface ICommand
    {
        CommandResult Execute();
    }

    public interface ICrossAppDomainCommand
    {
        CommandResult Result { get; }

        void Execute();
    }

    public interface ICrossAppDomainScriptCommand : ICrossAppDomainCommand
    {
        string[] ScriptArgs { get; }
    }
}