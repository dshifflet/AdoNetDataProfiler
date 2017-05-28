# AdoNetDataProfiler
DbCommand Profiler for ADO.NET (Stores Ids returned and Data and logs activity via Log4Net)

##What This Does
* Will log all SQL and Parameters to Log4Net and some DB activity.
* Can log the data to serialized DataTable files.
* Can log certain Id fields to files.

Should work with ADO.NET and NHibernate with OracleManagedClient

##TESTS
The DDL folder contains a DDL script to create the test database "Northwind" in Oracle.

Tests require a connection string config file.  It should be called _data-connectionStrings.config_
And look like 
'''
<?xml version="1.0" encoding="utf-8" ?>
<connectionStrings>
  <add name="northwind" connectionString="DATA SOURCE=TNS_NAME;USER ID=user;PASSWORD=password;enlist=dynamic;Connection Timeout=60;"/>
</connectionStrings>
'''

