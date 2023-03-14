using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Common.Settings
{
    public class SeqSettings
    {
        public string Host { get; init; }
        public int Port { get; init; }
        public string ServerUrl => $"http://{Host}:{Port}";
    }
}