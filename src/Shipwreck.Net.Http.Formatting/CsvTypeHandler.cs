using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    internal abstract class CsvTypeHandler
    {
        protected CsvTypeHandler(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        internal abstract bool HasHeader { get; }

        internal virtual Task WriteHeaders(TextWriter writer, string path)
        {
            if (path == null)
            {
                throw new NotSupportedException();
            }
            return WriteStringAsync(writer, path);
        }

        internal virtual Task WriteValues(TextWriter writer, object value)
            => WriteStringAsync(writer, value?.ToString());

        // TODO: Build dynamic method

        public virtual async Task WriteToAsync(TextWriter writer, object value)
        {
            if (HasHeader)
            {
                await WriteHeaders(writer, null);
                await writer.WriteLineAsync();
            }
            await WriteValues(writer, value);
        }

        protected static async Task WriteStringAsync(TextWriter writer, string value, bool forceQuote = false)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (!forceQuote && value.IndexOfAny(new[] { '"', ',', '\r', '\n' }) < 0)
            {
                await writer.WriteAsync(value);
            }
            else
            {
                var sb = new StringBuilder(value.Length + 8);
                sb.Append('"');
                foreach (var c in value)
                {
                    switch (c)
                    {
                        case '"':
                            sb.Append("\"\"");
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }
                };
                sb.Append('"');

                await writer.WriteAsync(sb.ToString());
            }
        }
    }
}