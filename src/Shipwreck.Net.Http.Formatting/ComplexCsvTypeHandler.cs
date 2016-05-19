using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwreck.Net.Http.Formatting
{
    internal sealed class ComplexCsvTypeHandler : CsvTypeHandler
    {
        private readonly PropertyInfo[] _Properties;
        private readonly CsvTypeHandler[] _Handlers;

        public ComplexCsvTypeHandler(Type type, PropertyInfo[] properties) : base(type)
        {
            _Properties = properties;
            _Handlers = properties.Select(p => CsvMediaTypeFormatter.GetTypeHandler(p.PropertyType)).ToArray();
        }

        internal override bool HasHeader => true;

        internal override async Task WriteHeaders(TextWriter writer, string path)
        {
            for (var i = 0; i < _Properties.Length; i++)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(',');
                }
                await _Handlers[i].WriteHeaders(writer, _Properties[i].Name);
            }
        }

        internal override async Task WriteValues(TextWriter writer, object value)
        {
            for (var i = 0; i < _Properties.Length; i++)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(',');
                }
                if (value != null)
                {
                    await _Handlers[i].WriteValues(writer, _Properties[i].GetValue(value));
                }
            }
        }
    }
}