﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;
using Mono.CSharp;
using ScriptCs.Contracts;
using ScriptCs.SyntaxTreeParser;

namespace ScriptCs.Engine.Mono
{
    public class MonoScriptEngine : IScriptEngine
    {
        private readonly IScriptHostFactory _scriptHostFactory;
        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }

        public const string SessionKey = "MonoSession";

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
        }

        public ILog Logger { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("references", references);
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            references.PathReferences.UnionWith(scriptPackSession.References);
            var parser = new SyntaxParser();

            SessionState<Evaluator> sessionState;
            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                Logger.Debug("Creating session");
                var context = new CompilerContext(new CompilerSettings
                {
                    AssemblyReferences = references.PathReferences.ToList()
                }, new ConsoleReportPrinter());

                var evaluator = new Evaluator(context);
                var builder = new StringBuilder();

                foreach (var ns in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    builder.AppendLine(string.Format("using {0};", ns));
                }

                evaluator.Compile(builder.ToString());

                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                MonoHost.SetHost((ScriptHost)host);

                evaluator.ReferenceAssembly(typeof(MonoHost).Assembly);
                evaluator.InteractiveBaseClass = typeof(MonoHost);

                sessionState = new SessionState<Evaluator>
                {
                    References = new AssemblyReferences { Assemblies = new HashSet<Assembly>(references.Assemblies), PathReferences = new HashSet<string>(references.PathReferences) },
                    Session = evaluator
                };
                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null ? references : references.Except(sessionState.References);
                foreach (var reference in newReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.LoadAssembly(reference);
                }

                sessionState.References = new AssemblyReferences
                {
                    Assemblies = new HashSet<Assembly>(references.Assemblies),
                    PathReferences = new HashSet<string>(references.PathReferences)
                };
            }

            Logger.Debug("Starting execution");

            try
            {
                var parseResult = parser.Parse(code);
                if (!string.IsNullOrWhiteSpace(parseResult.TypeDeclarations))
                {
                    sessionState.Session.Compile(parseResult.TypeDeclarations);
                }

                if (!string.IsNullOrWhiteSpace(parseResult.MethodDeclarations))
                {
                    sessionState.Session.Run(parseResult.MethodDeclarations);
                }

                if (!string.IsNullOrWhiteSpace(parseResult.Evaluations))
                {
                    object scriptResult;
                    bool resultSet;

                    sessionState.Session.Evaluate(parseResult.Evaluations, out scriptResult, out resultSet);

                    Logger.Debug("Finished execution");
                    return new ScriptResult { ReturnValue = scriptResult };
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return new ScriptResult();
        }
    }
}
