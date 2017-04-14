using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Picard
{
    internal sealed class IRDirectiveEmiter
    {
        // Internal Instance Data
        private readonly ThreadLocal<Func<string>> _identifierGenerator = new ThreadLocal<Func<string>>(() =>
        {
            var identifier = 0;
            return () => string.Format("@{0}_{1}", Thread.CurrentThread.ManagedThreadId, identifier++);
        });
        
        private readonly StringBuilder _directives = new StringBuilder();
        private readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        // Properties
        internal string Directives => _directives.ToString();

        // Methods
        internal void AddData(string identifier, object operand)
        {
            _data.Add(identifier, operand);
        }

        internal void AddDirective(string directive)
        {
            _directives.AppendLine(directive);
        }

        // Todo: Make it ThreadSafe!
        internal string NextIdentifier()
        {
            return _identifierGenerator.Value();
        }

        internal T GetData<T>(string key)
        {
            return (T)_data[key];
        }
    }
}