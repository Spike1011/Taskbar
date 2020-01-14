using System;

namespace TSU
{
    public interface ILogger
    {
        void Info(string s);
        void Error(string s, Exception ex);
        void Error(string s);
        void Trace(string s);
        void Warn(string s, Exception ex);
        void Fatal(string s, Exception ex);
    }
}
