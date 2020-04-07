using log4net;
using log4net.Config;
using System;
using System.Reflection;
using System.Xml;

namespace Techunk_Api
{
    public class log
    {
        public void main_log()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.log4net);
            XmlElement config = doc.DocumentElement;
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), config);

            Console.WriteLine("Hello world!");

            // Log some things
            log_.Info("Hello logging world!");
            log_.Error("Error!");
            log_.Warn("Warn!");
        }

        public static readonly ILog log_ = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
