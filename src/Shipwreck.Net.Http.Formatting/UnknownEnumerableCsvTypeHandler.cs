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
    internal sealed class UnknownEnumerableCsvTypeHandler : CsvTypeHandler
    {
        public UnknownEnumerableCsvTypeHandler()
            : base(typeof(IEnumerable))
        {
        }

        internal override bool HasHeader => false;

        public override async Task WriteToAsync(TextWriter writer, object value)
        {
            Type pt = null;
            CsvTypeHandler h = null;

            foreach (var v in (IEnumerable)value)
            {
                var t = v?.GetType();

                if (t != null)
                {
                    if (t != pt)
                    {
                        h = CsvMediaTypeFormatter.GetTypeHandler(t);

                        if (pt != null)
                        {
                            await writer.WriteLineAsync();
                        }
                        pt = t;

                        if (h.HasHeader)
                        {
                            await h.WriteHeaders(writer, null);
                            await writer.WriteLineAsync();
                        }
                    }
                    await h.WriteValues(writer, v);
                    await writer.WriteLineAsync();
                }
                else if (pt == null)
                {
                    await h.WriteValues(writer, null);
                    await writer.WriteLineAsync();
                }
                else
                {
                    await writer.WriteLineAsync();
                }
            }
        }
    }
}