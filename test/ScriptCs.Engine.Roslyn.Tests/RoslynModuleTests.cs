﻿using System;
using System.Collections.Generic;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Xunit;
using Should;

namespace ScriptCs.Tests
{
    public class RoslynModuleTests
    {
        public class TheInitializeMethod
        {
            private readonly Mock<IModuleConfiguration> _configMock = new Mock<IModuleConfiguration>();
            private readonly IModuleConfiguration _config;
            private readonly RoslynModule _module = new RoslynModule();
            private readonly IDictionary<Type, object> _overrides = new Dictionary<Type, object>();
            
            public TheInitializeMethod()
            {
                _configMock.SetupGet(c => c.Debug).Returns(false);
                _configMock.SetupGet(c => c.IsRepl).Returns(false);
                _configMock.SetupGet(c => c.Cache).Returns(false);
                _configMock.SetupGet(c => c.Overrides).Returns(_overrides);
                _config = _configMock.Object;
            }

            [Fact]
            public void ShouldNotOverrideTheEngineIfOneIsRegistered()
            {
                var engine = new Mock<IScriptEngine>();
                _overrides[typeof (IScriptEngine)] = engine.Object;
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(engine.Object);
            }

            [Fact]
            public void ShouldRegisterThePersistantScriptEngineWhenCacheIsEnabled()
            {
                _configMock.SetupGet(c => c.Cache).Returns(true);
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(typeof(RoslynScriptPersistentEngine));
            }

            [Fact]
            public void ShouldRegisterTheScriptEngineWhenCacheIsDisabled()
            {
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(typeof(RoslynScriptEngine));
            }

            [Fact]
            public void ShouldRegisterTheInMemoryEngineWhenDebugIsEnabled()
            {
                _configMock.Setup(c => c.Debug).Returns(true);
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(typeof(RoslynScriptInMemoryEngine));
            }

            [Fact]
            public void ShouldRegisterTheReplEngineWhenReplIsEnabled()
            {
                _configMock.Setup(c => c.IsRepl).Returns(true);
                _module.Initialize(_config);
                _overrides[typeof(IScriptEngine)].ShouldEqual(typeof(RoslynReplEngine));
            }
        }
    }
}
