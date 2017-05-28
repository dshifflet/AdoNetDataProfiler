using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace DataProfiler
{
    public static class ProfiledDataLogger
    {
        public static IDataReader LogIdsFromReader(IDataReader reader, Guid guid,
            ILog log, string session, DirectoryInfo logDataTo, bool logIds, IDbCommand command)
        {            
            if (logDataTo != null && logIds)
            {
                logDataTo.Create();
                var logFile = new FileInfo(string.Concat(logDataTo.FullName, @"\", session, "_", guid, ".ids"));
                var ids = new Dictionary<string, List<object>>();


                var nhFieldMap = GetNhFieldMap(command);

                log.DebugFormat("Output To {0}", logFile.FullName);
                using (var table = new DataTable())
                {
                    table.Load(reader);                   
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            if (row[column.ColumnName] == DBNull.Value) continue;
                            //todo make this _ID thing settable
                            var mapping = column.ColumnMapping;


                            string matchedColumn = null;
                            string matchedDataColumn = null;
                            var match = ProfiledConfiguration.IdMatcher.IsMatch(column.ColumnName.ToUpper());
                            if (match)
                            {
                                matchedColumn = column.ColumnName;
                                matchedDataColumn = column.ColumnName;
                            }
                            else if (nhFieldMap.Any())
                            {
                                //Check this dictionary...
                                string val;
                                if (nhFieldMap.TryGetValue(column.ColumnName, out val))
                                {
                                    match = ProfiledConfiguration.IdMatcher.IsMatch(val.ToUpper());
                                    if (match)
                                    {
                                        matchedColumn = val;
                                        matchedDataColumn = column.ColumnName;
                                    }
                                }
                            }


                            if (match)
                            {
                                List<object> list;
                                if (!ids.TryGetValue(matchedColumn, out list))
                                {
                                    list = new List<object>();
                                    ids.Add(matchedColumn, list);
                                }

                                var val = row[matchedDataColumn];
                                if (!list.Contains(val))
                                {
                                    list.Add(val);
                                }
                            }

                        }
                    }
                    if (ids.Any())
                    {
                        //todo this is really bad
                        using (var sw = new StreamWriter(logFile.Create()))
                        {
                            foreach (var item in ids)
                            {
                                sw.WriteLine("=>{0}", item.Key);
                                foreach (var element in item.Value)
                                {
                                    if (element is DateTime)
                                    {
                                        sw.WriteLine("#{0}", element);
                                    }
                                    else if (element is string)
                                    {
                                        sw.WriteLine("\"{0}\"", element);
                                    }
                                    else
                                    {
                                        sw.WriteLine(element);
                                    }

                                }
                            }
                        }
                    }
                    return table.CreateDataReader();
                }
            }
            return reader;
        }

        private static Dictionary<string, string> GetNhFieldMap(IDbCommand command)
        {
            var result = new Dictionary<string, string>();
            
            var fieldMatch = ProfiledConfiguration.NhFieldSelect.Matches(command.CommandText);
            if (fieldMatch.Count <= 0) return result;

            var first = fieldMatch[0];
            if (first.Groups.Count <= 1) return result;

            var txt = first.Groups[1].ToString();
            var splits = txt.Split(',');
            foreach (var item in splits)
            {
                var parse = ProfiledConfiguration.NhInnerFieldSelect.Matches(item);
                if (parse.Count > 0 && parse[0].Groups.Count == 3)
                {
                    var fieldSplits = parse[0].Groups[1].ToString().Split('.');
                    if (fieldSplits.Length > 1)
                    {
                        var fieldName = fieldSplits[1];
                        var sqlName = parse[0].Groups[2].ToString();       
                        result.Add(sqlName, fieldName);
                    }
                }
            }
            return result;
        }

        public static IDataReader LogReader(IDataReader reader, IDbCommand command, Guid guid, 
            ILog log, string session, DirectoryInfo logDataTo, bool logData)
        {
            if (logDataTo != null && logData)
            {
                logDataTo.Create();
                var logFile = new FileInfo(string.Concat(logDataTo.FullName, @"\", session, "_", guid, ".data"));
                log.DebugFormat("Output To {0}", logFile.FullName);
                using (var table = new DataTable())
                {
                    table.Load(reader);
                    table.TableName = guid.ToString();
                    if (!table.ExtendedProperties.ContainsKey("LoggedCommandText"))
                    {
                        table.ExtendedProperties.Add("LoggedCommandText", command.CommandText);
                    }
                    if (!table.ExtendedProperties.ContainsKey("LoggedCommandParameters"))
                    {
                        table.ExtendedProperties.Add("LoggedCommandParameters",
                            string.Join("\r\n", GetParametersFormatted(command)));
                    }
                    table.WriteXml(logFile.FullName, XmlWriteMode.WriteSchema);
                    return table.CreateDataReader();
                }
            }
            return reader;
        }

        public static void LogSql(IDbCommand command, ILog log)
        {
            log.DebugFormat("{0}", command.CommandText);
            if (command.Parameters.Count == 0) return;
            log.DebugFormat("------ PARAMETERS ------");
            foreach (var s in GetParametersFormatted(command))
            {
                log.Debug(s);
            }
        }

        private static string[] GetParametersFormatted(IDbCommand command)
        {
            string[] result = new string[command.Parameters.Count];

            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var parameter = (IDataParameter)command.Parameters[i];
                if (parameter.Value is string)
                {
                    result[i] = string.Format("{0}: \"{1}\"", parameter.ParameterName, parameter.Value);
                }
                else
                {
                    result[i] = string.Format("{0}: {1}", parameter.ParameterName, parameter.Value);
                }

            }
            return result;
        }

    }
}
