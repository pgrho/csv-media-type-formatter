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
    internal sealed class TypedEnumerableCsvTypeHandler : CsvTypeHandler
    {
        private readonly CsvTypeHandler _ElementTypeHandler;

        public TypedEnumerableCsvTypeHandler(CsvTypeHandler elementTypeHandler)
            : base(typeof(IEnumerable<>).MakeGenericType(new[] { elementTypeHandler.Type }))
        {
            _ElementTypeHandler = elementTypeHandler;
        }

        internal override bool HasHeader => _ElementTypeHandler.HasHeader;

        internal override Task WriteHeaders(TextWriter writer, string path)
            => _ElementTypeHandler.WriteHeaders(writer, path);

        internal override Task WriteValues(TextWriter writer, object value)
            => _ElementTypeHandler.WriteValues(writer, value);

        public override async Task WriteToAsync(TextWriter writer, object value)
        {
            if (HasHeader)
            {
                await WriteHeaders(writer, null);
            }
            foreach (var v in (IEnumerable)value)
            {
                await _ElementTypeHandler.WriteValues(writer, v);
                await writer.WriteLineAsync();
            }
        }
    }
}