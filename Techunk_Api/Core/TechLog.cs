using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Techunk_Api.Core
{
    public class TechLog
    {
        public void main_log()
        {
            XmlDocument doc = new XmlDocument();
            var assembly = typeof(TechLog).GetTypeInfo().Assembly;
            Stream log4net_config = assembly.GetManifestResourceStream("log4net.config");
            doc.Load(log4net_config);
            XmlElement config = doc.DocumentElement;
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), config);

            Console.WriteLine("Hello world!");

            // Log some things
            log.Info("Configuración log cargada");
        }

        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}