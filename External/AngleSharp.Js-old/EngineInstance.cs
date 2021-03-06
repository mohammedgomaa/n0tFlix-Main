namespace AngleSharp.Js
{
    using AngleSharp.Dom;
    using Jint;
    using Jint.Native;
    using Jint.Native.Object;
    using Jint.Runtime.Environments;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal sealed class EngineInstance
    {
        #region Fields

        private readonly Engine _engine;
        private readonly PrototypeCache _prototypes;
        private readonly ReferenceCache _references;
        private readonly IEnumerable<Assembly> _libs;
        private readonly LexicalEnvironment _lexicals;
        private readonly LexicalEnvironment _variables;
        private readonly DomNodeInstance _window;

        #endregion Fields

        #region ctor

        public EngineInstance(IWindow window, IDictionary<String, Object> assignments, IEnumerable<Assembly> libs)
        {
            var context = window.Document.Context;
            var logger = context.GetService<IConsoleLogger>();
            _engine = new Engine();
            _prototypes = new PrototypeCache(_engine);
            _references = new ReferenceCache();
            _libs = libs;
            _engine.SetValue("console", new ConsoleInstance(_engine, logger));

            foreach (var assignment in assignments)
            {
                _engine.SetValue(assignment.Key, assignment.Value);
            }

            _window = GetDomNode(window);
            _lexicals = LexicalEnvironment.NewDeclarativeEnvironment(_engine, _engine.ExecutionContext.LexicalEnvironment);
            _variables = LexicalEnvironment.NewDeclarativeEnvironment(_engine, _engine.ExecutionContext.VariableEnvironment);

            foreach (var lib in libs)
            {
                this.AddConstructors(_window, lib);
                this.AddConstructors(_window, lib);
                this.AddInstances(_window, lib);
            }
        }

        #endregion ctor

        #region Properties

        public IEnumerable<Assembly> Libs => _libs;

        public DomNodeInstance Window => _window;

        public LexicalEnvironment Lexicals => _lexicals;

        public LexicalEnvironment Variables => _variables;

        public Engine Jint => _engine;

        #endregion Properties

        #region Methods

        public DomNodeInstance GetDomNode(Object obj) => _references.GetOrCreate(obj, CreateInstance);

        public ObjectInstance GetDomPrototype(Type type) => _prototypes.GetOrCreate(type, CreatePrototype);

        public JsValue RunScript(String source, JsValue context)
        {
            lock (_engine)
            {
                _engine.EnterExecutionContext(Lexicals, Variables);
                _engine.Execute(source);
                _engine.LeaveExecutionContext();
                return _engine.GetCompletionValue();
            }
        }

        #endregion Methods

        #region Helpers

        private DomNodeInstance CreateInstance(Object obj) => new DomNodeInstance(this, obj);

        private ObjectInstance CreatePrototype(Type type) => new DomPrototypeInstance(this, type);

        #endregion Helpers
    }
}