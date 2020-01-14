using NLog;
using System;

namespace TSU
{
    public class LoggerProj : ILogger
    {
        public void Info(string s)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info(s);
        }

        public void Error(string s, Exception ex)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Error(s + "\n" + ex.ToString());
        }

        public void Error(string s)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Error(s);
        }

        public void Trace(string s)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Trace(s);
        }

        public void Warn(string s, Exception ex)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Warn(s, ex.ToString());
        }

        public void Fatal(string s, Exception ex)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(s, ex.ToString());
        }

    }
}
