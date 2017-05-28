using System;
using System.Collections.Generic;
using System.Configuration;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Util;
using Configuration = NHibernate.Cfg.Configuration;
using System.Data;
using System.IO;
using DataProfiler;
using NhDataProfiler;
using NHibernate.Mapping.ByCode;
using System.Linq;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace NHibernateTests
{
    [TestClass]
    public class NhibernateTests
    {
        private static readonly ILog Log = LogManager.GetLogger("NHibernateTests");
        private Configuration _nhConfig;
        public NhibernateTests()
        {
            ProfiledConfiguration.LogData = true;
            ProfiledConfiguration.LogIds = true;
            ProfiledConfiguration.LogDataTo = new DirectoryInfo("./NhLogData");
            _nhConfig = ConfigureNh(ConfigurationManager.ConnectionStrings["northwind"].ConnectionString);
        }
        
        [TestMethod]
        public void CanExecuteNhSqlQuery()
        {
            Console.WriteLine("Using {0}", ProfiledConfiguration.Session);

            /* Create a session and execute a query: */
            using (var factory = _nhConfig.BuildSessionFactory())
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var qry = session.CreateSQLQuery(                    
                        @"select * from customer where customer_id=:customerId");
                qry.SetParameter("customerId", 4);
                var list = qry.List();
                Assert.IsTrue(list.Any());
                tx.Commit();
            }
            DoesProfiledDataExist();
        }

        [TestMethod]
        public void CanExecuteNhQuery()
        {
            using (var factory = _nhConfig.BuildSessionFactory())
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var test = session.QueryOver<Customer>().Where(o => o.Id == 4).List();
                Assert.IsTrue(test.Any());
            }
            DoesProfiledDataExist();
        }

        private void DoesProfiledDataExist()
        {
            //Confirm we have some profiled data...
            var idFiles = ProfiledConfiguration.LogDataTo.GetFiles(string.Format("{0}*.ids", ProfiledConfiguration.Session));
            var dataFiles = ProfiledConfiguration.LogDataTo.GetFiles(string.Format("{0}*.data", ProfiledConfiguration.Session));
            Assert.IsTrue(idFiles.Any());
            Assert.IsTrue(dataFiles.Any());
        }

#region NHibernate

        private Configuration ConfigureNh(string connectionString)
        {
            if(connectionString==null) throw new ArgumentNullException("connectionString");

            var cfg = new Configuration()
                .DataBaseIntegration(db =>
                {
                    db.ConnectionString = connectionString;                    
                    db.Dialect<Oracle12cDialect>();
                    //db.Driver<NHibernate.Driver.OracleManagedDataClientDriver>();
                    db.Driver<ProfiledOracleManagedDataClientDriver>();
                    db.ConnectionProvider<NHibernate.Connection.DriverConnectionProvider>();
                    db.IsolationLevel = IsolationLevel.ReadCommitted;
                    db.Timeout = 60;
                });


            var hibernateMappings = RegisterMappings().Select(map =>
            {
                //If you have two persistent classes with the same (unqualified) name, you should set auto-import="false". 
                //NHibernate will throw an exception if you attempt to assign two classes to the same "imported" name.
                //Say two classes having same name from different assemblies (say ECTM.Models.MyClass and ECTM.DTO.MyClass)
                var hbm = map.CompileMappingForAllExplicitlyAddedEntities();
                hbm.autoimport = false;
                return hbm;
            }).ToArray();
            hibernateMappings.ToList().ForEach(cfg.AddMapping);
            return cfg;
        }

        private IEnumerable<ModelMapper> RegisterMappings()
        {
            var mapper = new ModelMapper();
            //MAPPINGS REGISTERED HERE
            mapper.AddMapping(new CustomerMapping());

            return new List<ModelMapper> { mapper };
        }
#endregion
    }
}
