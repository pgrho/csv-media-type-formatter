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
    public sealed class CsvMediaTypeFormatter : MediaTypeFormatter
    {
        private static readonly Dictionary<Type, CsvTypeHandler> _Handlers = new Dictionary<Type, CsvTypeHandler>();

        /// <summary>
        /// <see cref="CsvMediaTypeFormatter" />クラスの新しいインスタンスを初期化します。
        /// </summary>
        public CsvMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/comma-separated-values"));

            SupportedEncodings.Add(new UTF8Encoding(false));

            foreach (var e in Encoding.GetEncodings())
            {
                if (e.Name == "utf-8")
                {
                    continue;
                }
                SupportedEncodings.Add(e.GetEncoding());
            }
        }

        // TODO: Separator Char
        // TODO: Quotation
        // TODO: NewLine

        public override bool CanReadType(Type type) => false;

        public override bool CanWriteType(Type type) => type != null;

        internal static CsvTypeHandler GetTypeHandler(Type type)
        {
            CsvTypeHandler h;
            lock (_Handlers)
            {
                if (!_Handlers.TryGetValue(type, out h))
                {
                    _Handlers[type] = h = CreateTypeHandler(type);
                }
            }
            return h;
        }
        private static CsvTypeHandler CreateTypeHandler(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var typed = type.GetInterfaces().FirstOrDefault(_ => _.IsConstructedGenericType && _.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (typed == null)
                {
                    return new UnknownEnumerableCsvTypeHandler();
                }
                else
                {
                    return new TypedEnumerableCsvTypeHandler(GetTypeHandler(typed.GetGenericArguments()[0]));
                }
            }
            if (typeof(IFormattable).IsAssignableFrom(type)
                || typeof(IConvertible).IsAssignableFrom(type))
            {
                return new ValueCsvTypeHandler(type);
            }
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return new NullableCsvTypeHandler(GetTypeHandler(type.GetGenericArguments()[0]));
            }

            var conv = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(_ => (_.Name == "op_Implicit" || _.Name == "op_Explicit") && _.ReturnType == typeof(string));

            if (conv != null)
            {
                return new ConversionCsvTypeHandler(conv);
            }
            else
            {
                var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(_ => _.CanRead && _.GetCustomAttribute<IgnoreDataMemberAttribute>() == null)
                                    .ToArray();

                if (props.Length == 0)
                {
                    return new ValueCsvTypeHandler(type);
                }
                else
                {
                    return new ComplexCsvTypeHandler(type, props);
                }
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
            => Task.Run(async () =>
            {
                using (var sw = new StreamWriter(writeStream, SelectCharacterEncoding(content?.Headers), 1024, true))
                {
                    await GetTypeHandler(type).WriteToAsync(sw, value);
                }
            });
    }
}