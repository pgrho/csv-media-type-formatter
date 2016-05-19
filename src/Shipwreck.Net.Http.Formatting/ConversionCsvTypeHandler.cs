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
    internal sealed class ConversionCsvTypeHandler : CsvTypeHandler
    {
        private readonly MethodInfo _Conversion;

        public ConversionCsvTypeHandler(MethodInfo conversion)
            : base(conversion.ReturnType)
        {
            _Conversion = conversion;
        }

        internal override bool HasHeader => false;
    }
}