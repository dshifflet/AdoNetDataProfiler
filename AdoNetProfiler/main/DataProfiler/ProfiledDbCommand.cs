﻿using System;
using System.Data;
using System.Data.Common;
using log4net;

namespace DataProfiler
{
    public class ProfiledDbCommand : DbCommand
    {
        public DbCommand Command { get; private set; }
        private static readonly ILog Log = LogManager.GetLogger("ProfiledDbCommand");


        public ProfiledDbCommand(DbCommand command)
        {
            if(command==null) throw new ArgumentNullException("command");
            Command = command;
        }
        
        public new void Dispose()
        {
            Command.Dispose();
        }

        public override void Prepare()
        {
            Command.Prepare();
        }

        public override void Cancel()
        {
            Command.Cancel();
        }

        public new IDbDataParameter CreateParameter()
        {
            return Command.CreateParameter();
        }

        public override int ExecuteNonQuery()
        {
            ProfiledDataLogger.LogSql(Command, Log);
            var affected = Command.ExecuteNonQuery();            
            Log.DebugFormat("{0} rows affected", affected);
            return affected;
        }

        public new IDataReader ExecuteReader()
        {
            ProfiledDataLogger.LogSql(Command, Log);
            return LogReader(Command.ExecuteReader(), Guid.NewGuid(), Command);
        }

        public new IDataReader ExecuteReader(CommandBehavior behavior)
        {
            ProfiledDataLogger.LogSql(Command, Log);
            return LogReader(Command.ExecuteReader(behavior), Guid.NewGuid(), Command);
        }

        private IDataReader LogReader(IDataReader reader, Guid guid, IDbCommand command)
        {
            return ProfiledDataLogger.LogIdsFromReader(
                ProfiledDataLogger.LogReader(reader, Command, guid, Log, ProfiledConfiguration.Session, ProfiledConfiguration.LogDataTo, ProfiledConfiguration.LogData)
                , guid, Log, ProfiledConfiguration.Session, ProfiledConfiguration.LogDataTo, ProfiledConfiguration.LogData, command);            
        }

        public override object ExecuteScalar()
        {
            ProfiledDataLogger.LogSql(Command, Log);
            return Command.ExecuteScalar();
        }
        
        public new DbConnection Connection
        {
            get { return Command.Connection; }
            set { Command.Connection = value; }
        }

        public new DbTransaction Transaction
        {
            get { return Command.Transaction; }
            set { Command.Transaction = value; }
        }

        public override string CommandText
        {
            get { return Command.CommandText; }
            set { Command.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return Command.CommandTimeout; }
            set { Command.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return Command.CommandType; }
            set { Command.CommandType = value; }
        }

        public new IDataParameterCollection Parameters
        {
            get { return Command.Parameters; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return Command.UpdatedRowSource; }
            set { Command.UpdatedRowSource = value; }
        }

        protected override DbParameter CreateDbParameter()
        {
            return Command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            
            ProfiledDataLogger.LogSql(Command, Log);
            var reader = Command.ExecuteReader(behavior);
            return (DbDataReader) LogReader(reader, Guid.NewGuid(), Command);            
        }

        protected override DbConnection DbConnection {
            get { return Command.Connection; }
            set { Command.Connection = value; }
            }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return Command.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return Command.Transaction; }
            set { Command.Transaction = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return Command.DesignTimeVisible; }
            set { Command.DesignTimeVisible = value; }
        }
    }
}