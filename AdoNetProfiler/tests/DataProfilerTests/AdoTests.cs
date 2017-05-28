using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DataProfiler;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace DataProfilerTests
{
    [TestClass]
    public class AdoTests
    {
        private static readonly ILog Log = LogManager.GetLogger("AdoTests");

        [TestMethod]
        public void CanGetRecords()
        {
            //have to use this for the NH profiler....
            ProfiledConfiguration.LogData = true;
            ProfiledConfiguration.LogIds = true;
            ProfiledConfiguration.LogDataTo = new DirectoryInfo("./LogData");

            using (var cn = new ProfiledConnection(
                new OracleConnection(ConfigurationManager.ConnectionStrings["northwind"].ConnectionString)))
            {
                cn.Open();
                Console.WriteLine("Using {0}", ProfiledConfiguration.Session);
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = @"select * from customer where customer_id=:customerId";
                    var customerId = cmd.CreateParameter();
                    customerId.ParameterName = "customerId";
                    customerId.Value = 4;
                    cmd.Parameters.Add(customerId);
                    var cnt = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cnt++;
                        }
                    }
                    Assert.IsTrue(cnt>0);
                }

                //Confirm we have some profiled data...
                var idFiles = ProfiledConfiguration.LogDataTo.GetFiles(string.Format("{0}*.ids", ProfiledConfiguration.Session));
                var dataFiles = ProfiledConfiguration.LogDataTo.GetFiles(string.Format("{0}*.data", ProfiledConfiguration.Session));
                Assert.IsTrue(idFiles.Any());
                Assert.IsTrue(dataFiles.Any());
            }
        }
    }
}
