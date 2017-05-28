using System;
using System.Data;
using System.Data.Common;
using log4net;

namespace DataProfiler
{
    public class ProfiledConnection : IDbConnection
    {
        private readonly IDbConnection _connection;
        private static readonly ILog Log = LogManager.GetLogger("DataProfiler");

        public ProfiledConnection(IDbConnection connection)
        {
            if(connection==null) throw new ArgumentNullException("connection");
            _connection = connection;           
            Log.DebugFormat("Using Session '{0}'", ProfiledConfiguration.Session);
        }

        public void Dispose()
        {
            if (_connection != null) _connection.Dispose();
        }

        public IDbTransaction BeginTransaction()
        {
            Log.Debug("Transaction Begun");
            return _connection.BeginTransaction();

        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            Log.DebugFormat("Transaction Begun ({0})", il);
            return _connection.BeginTransaction(il);
        }

        public void Close()
        {
            Log.Debug("Connection Closed");
            _connection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            Log.Debug("Command Created");
            return new ProfiledDbCommand((DbCommand)_connection.CreateCommand());
        }

        public void Open()
        {
            Log.Debug("Connection Open");
            _connection.Open();
        }

        public string ConnectionString
        {
            get { return _connection.ConnectionString; } 
            set{_connection.ConnectionString = value;}
        }
        public int ConnectionTimeout {
            get { return _connection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return _connection.Database; }
        }

        public ConnectionState State {
            get { return _connection.State; }
        }
    }
}