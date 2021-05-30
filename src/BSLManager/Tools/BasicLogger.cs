using System;
using System.IO;

namespace BSLManager.Tools
{
    public static class BasicLogger
    {
        public static void Log(string log)
        {
            File.AppendAllLines($"Log-{Guid.NewGuid()}.txt", new []{ log });
        }
    }
}