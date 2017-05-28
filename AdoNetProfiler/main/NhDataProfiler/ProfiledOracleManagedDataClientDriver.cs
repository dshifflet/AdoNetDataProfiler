using System;
using System.Data;
using System.Reflection;
using DataProfiler;
using NHibernate.Driver;
using NHibernate.Engine.Query;
using NHibernate.Util;
using Oracle.ManagedDataAccess.Client;
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local

namespace NhDataProfiler
{



    /// <summary>
    /// A NHibernate Driver for using the Oracle.ManagedDataAccess DataProvider
    /// </summary>
    public class ProfiledOracleManagedDataClientDriver : OracleManagedDataClientDriver
    {
        private const string DriverAssemblyName = "Oracle.ManagedDataAccess";

        private const string ConnectionTypeName = "Oracle.ManagedDataAccess.Client.OracleConnection";
        private const string CommandTypeName = "Oracle.ManagedDataAccess.Client.OracleCommand";

        private readonly PropertyInfo _oracleCommandBindByName;
        private readonly PropertyInfo _oracleDbType;
        private readonly object _oracleDbTypeRefCursor;
        private readonly object _oracleDbTypeXmlType;
        private readonly object _oracleDbTypeBlob;

        public ProfiledOracleManagedDataClientDriver()
        {
			var oracleCommandType = ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleCommand", DriverAssemblyName, true);
			_oracleCommandBindByName = oracleCommandType.GetProperty("BindByName");

			var parameterType = ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleParameter", DriverAssemblyName, true);
			_oracleDbType = parameterType.GetProperty("OracleDbType");

			var oracleDbTypeEnum = ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleDbType", DriverAssemblyName, true);
			_oracleDbTypeRefCursor = Enum.Parse(oracleDbTypeEnum, "RefCursor");
			_oracleDbTypeXmlType = Enum.Parse(oracleDbTypeEnum, "XmlType");
			_oracleDbTypeBlob = Enum.Parse(oracleDbTypeEnum, "Blob");
}

        public override IDbCommand CreateCommand()
        {
            var command = (OracleCommand)base.CreateCommand();
            return new ProfiledDbCommand(command);
        }

        
        protected override void OnBeforePrepare(IDbCommand command)
        {
            if (command is ProfiledDbCommand)
            {
                var cmd = (ProfiledDbCommand) command;

                base.OnBeforePrepare(cmd.Command);
                // need to explicitly turn on named parameter binding
                // http://tgaw.wordpress.com/2006/03/03/ora-01722-with-odp-and-command-parameters/
                _oracleCommandBindByName.SetValue(cmd.Command, true, null);
            }
            else
            {
                base.OnBeforePrepare(command);
                _oracleCommandBindByName.SetValue(command, true, null);
            }
            
            var detail = CallableParser.Parse(command.CommandText);

            if (!detail.IsCallable)
                return;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = detail.FunctionName;
            _oracleCommandBindByName.SetValue(command, false, null);

            var outCursor = command.CreateParameter();
            _oracleDbType.SetValue(outCursor, _oracleDbTypeRefCursor, null);

            outCursor.Direction = detail.HasReturn ? ParameterDirection.ReturnValue : ParameterDirection.Output;

            command.Parameters.Insert(0, outCursor);
        }

    }
}
