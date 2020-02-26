using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimioAPI;
using SimioAPI.Extensions;
using System.Net;
using System.IO;
using System.Xml;

namespace DirectConnect
{
    struct DbColumnInfo
    {
        public string Name;
        public Type Type;
    }


    public static class DirectConnectUtils
    {
        private static SqlConnection _connection;
        private static Int32 _connectionTimeOut = 600;
        private static string _connectionString = string.Empty;
        private static CultureInfo _cultureInfo = CultureInfo.CurrentCulture;
        private static string _dateTimeFormatString = string.Empty;

        public const double MaxSqlFloat =  1.0E+308; // designed to be memorable
        public const double MinSqlFloat = -MaxSqlFloat; // designed to be memorable

        ////public const double MaxSqlReal = 1.0E+38; // designed to be memorable
        ////public const double MinSqlReal = -MaxSqlReal; // designed to be memorable


        /// <summary>
        /// Returns a list of tuples that are ClrType and corresponding SqlType(as strings)
        /// that it should convert to. E.g. "System.Boolean", "BIT"
        /// See: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/sql-clr-type-mapping?redirectedfrom=MSDN
        /// Note: This assumes SQL Server 2008+ and.NET Framework 3.5 SP1
        /// Also, there are imperfect corner cases: the SQL Time is only 0-23:59:59.9999999 and
        /// therefore probably not a good fit for TimeSpan.
        /// </summary>
        private static List<Tuple<string, String>> ClrToSqlList { get; set; } = new List<Tuple<string, string>>
            {
                new Tuple<String, string> ("System.Boolean", "BIT")
                ,new Tuple<String, string> ("System.Byte", "TINYINT")
                ,new Tuple<String, string> ("System.Int16", "SMALLINT")
                ,new Tuple<String, string> ("System.Int32", "INT")
                ,new Tuple<String, string> ("System.Int64", "BIGINT")
                ,new Tuple<String, string> ("System.SByte", "SMALLINT")
                ,new Tuple<String, string> ("System.UInt16", "INT")
                ,new Tuple<String, string> ("System.UInt32", "BIGINT")
                ,new Tuple<String, string> ("System.UInt64", "DECIMAL(20)")
                ,new Tuple<String, string> ("System.Decimal", "DECIMAL(29,4)")
                ,new Tuple<String, string> ("System.Single", "FLOAT")
                ,new Tuple<String, string> ("System.Double", "FLOAT")
                ,new Tuple<String, string> ("System.String", "NVARCHAR(MAX)")
                ,new Tuple<String, string> ("System.DateTime", "DATETIME")
                ,new Tuple<String, string> ("System.TimeSpan", "TIME")
                ,new Tuple<String, string> ("System.DateTimeOffset", "DATETIMEOFFSET")
                ,new Tuple<string, string> ("System.Guid", "UNIQUEIDENTIFIER")
                ,new Tuple<string, string> ("System.Object", "SQL_VARIANT")
            };


        public static void SetConnectionTimeOut(Int32 connectionTimeOut)
        {
            _connectionTimeOut = connectionTimeOut;
        }

        public static void SetConnectionAndConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            SetConnection(_connectionString);
        }

        public static void SetDateTimeFormatString(string dateTimeFormatString)
        {
            _dateTimeFormatString = dateTimeFormatString;
        }

        /// <summary>
        /// Set the connection that will be live throughout the life of this model.
        /// Employs globals _connection and _connectionString.
        /// </summary>
        public static void SetConnection(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(connectionString);
            }
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public static void ClearConnectionAndConnectionString()
        {
            _connectionString = string.Empty;
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// Communicate with the database to get column information about the given table.
        /// No data is returned.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="useStoredProcedure"></param>
        /// <returns></returns>
        internal static IEnumerable<DbColumnInfo> GetColumnInfoForTable(string tableName, bool useStoredProcedure)
        {
            CheckConnection();

            using (var cmd = _connection.CreateCommand())
            {
                if (useStoredProcedure)
                {
                    cmd.CommandText = tableName;
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandText = $"SELECT * FROM [{tableName}] WHERE 1 = 0";
                }

                cmd.CommandTimeout = _connectionTimeOut;

                using (var da = new SqlDataAdapter())
                {
                    da.SelectCommand = cmd;
                    using (var ds = new DataSet())
                    {
                        da.Fill(ds);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                        {
                            var name = col.ColumnName;
                            var type = col.DataType;

                            yield return new DbColumnInfo()
                            {
                                Name = name,
                                Type = type
                            };
                        }
                    }
                } // using SqlDataAdapter
            } // using CreateCommand
        }

        /// <summary>
        /// Called during the model load to set up an 'action' tables that are
        /// used to specify export actions for (1) tables, (2) logs
        /// </summary>
        /// <param name="model"></param>
        public static void NewModelSetup(IModel model)
        {
            // Add ExportTableActions
            var exportTableActionsList = model.NamedLists["ExportTableActions"];
            if (exportTableActionsList == null)
            {
                exportTableActionsList = model.NamedLists.AddStringList("ExportTableActions");
                var firstRow = exportTableActionsList.Rows.Create();
                firstRow.Properties[0].Value = "DropCreateAndRepopulate";
                var secondRow = exportTableActionsList.Rows.Create();
                secondRow.Properties[0].Value = "TruncateAndRepopulate";
                var thirdRow = exportTableActionsList.Rows.Create();
                thirdRow.Properties[0].Value = "UpdateAndInsert";
                var fourthRow = exportTableActionsList.Rows.Create();
                fourthRow.Properties[0].Value = "UpdateInsertAndDelete";
                var fifthRow = exportTableActionsList.Rows.Create();
                fifthRow.Properties[0].Value = "Insert";
            }

            // Add ExportLogActions
            var exportLogActionsList = model.NamedLists["ExportLogActions"];
            if (exportLogActionsList == null)
            {
                exportLogActionsList = model.NamedLists.AddStringList("ExportLogActions");
                var firstRow = exportLogActionsList.Rows.Create();
                firstRow.Properties[0].Value = "DropCreateAndRepopulate";
                var secondRow = exportLogActionsList.Rows.Create();
                secondRow.Properties[0].Value = "TruncateAndRepopulate";
            }

            // Add TableExportConfig Table
            var tableExportConfig = model.Tables["TableExportConfig"];
            if (tableExportConfig == null)
            {
                tableExportConfig = model.Tables.Create("TableExportConfig");
                var tn = tableExportConfig.Columns.AddTableReferenceColumn("TableName");
                tn.IsKey = true;
                tn.PlanningSettings.VisibleInTables = false;
                var en = tableExportConfig.Columns.AddBooleanColumn("Enabled", true);
                en.PlanningSettings.VisibleInTables = false;
                var os = tableExportConfig.Columns.AddListReferenceColumn("Action");
                os.ListName = "ExportTableActions";
                os.DefaultString = "TruncateAndRepopulate";

                tableExportConfig.Columns.AddStringColumn("SqlServerTableName", "");
                tableExportConfig.Columns.AddStringColumn("PreSaveStoredProcedure", "");
                tableExportConfig.Columns.AddStringColumn("PostSaveStoredProcedure", "");
            }

            // Add TableExportExcludeForUpdate Table
            var tableExportExcludeForUpdate = model.Tables["TableExportExcludeForUpdate"];
            if (tableExportExcludeForUpdate == null)
            {
                tableExportExcludeForUpdate = model.Tables.Create("TableExportExcludeForUpdate");
                var tn = tableExportExcludeForUpdate.Columns.AddForeignKeyColumn("TableName");
                tn.TableKey = "TableExportConfig.TableName";
                tn.PlanningSettings.VisibleInTables = false;
                var cn = tableExportExcludeForUpdate.Columns.AddStringColumn("ColumnName", String.Empty);
                cn.PlanningSettings.VisibleInTables = false;
            }

            // Add LogExportConfig Table
            var logExportConfig = model.Tables["LogExportConfig"];
            if (logExportConfig == null)
            {
                logExportConfig = model.Tables.Create("LogExportConfig");
                var ln = logExportConfig.Columns.AddStringColumn("LogName", "");
                ln.IsKey = true;
                ln.PlanningSettings.VisibleInTables = false;
                var en = logExportConfig.Columns.AddBooleanColumn("Enabled", true);
                en.PlanningSettings.VisibleInTables = false;
                var os = logExportConfig.Columns.AddListReferenceColumn("Action");
                os.ListName = "ExportLogActions";
                os.DefaultString = "TruncateAndRepopulate";
                logExportConfig.Columns.AddStringColumn("SqlServerTableName", "");
                var firstRow = logExportConfig.Rows.Create();
                firstRow.Properties[0].Value = "ResourceUsageLog";
                var secondRow = logExportConfig.Rows.Create();
                secondRow.Properties[0].Value = "ResourceStateLog";
                var thirdRow = logExportConfig.Rows.Create();
                thirdRow.Properties[0].Value = "ResourceCapacityLog";
                var fourthRow = logExportConfig.Rows.Create();
                fourthRow.Properties[0].Value = "ResourceInfoLog";
                var fifthRow = logExportConfig.Rows.Create();
                fifthRow.Properties[0].Value = "ConstraintLog";
                var sixthRow = logExportConfig.Rows.Create();
                sixthRow.Properties[0].Value = "TransporterUsageLog";
                var seventhRow = logExportConfig.Rows.Create();
                seventhRow.Properties[0].Value = "MaterialUsageLog";
                var eighthRow = logExportConfig.Rows.Create();
                eighthRow.Properties[0].Value = "TaskLog";
                var ninthRow = logExportConfig.Rows.Create();
                ninthRow.Properties[0].Value = "TaskStateLog";
                var tenthRow = logExportConfig.Rows.Create();
                tenthRow.Properties[0].Value = "StateObservationLog";
                var eleventhRow = logExportConfig.Rows.Create();
                eleventhRow.Properties[0].Value = "TallyObservationLog";
                var twelfthRow = logExportConfig.Rows.Create();
                twelfthRow.Properties[0].Value = "TargetResults";
            }
        }

        /// <summary>
        /// Given a tableName, create a dataset from a SQL Table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="useStoredProcedure"></param>
        /// <returns></returns>
        internal static DataSet GetDataSet(string tableName, bool useStoredProcedure)
        {
            CheckConnection();

            using (var cmd = _connection.CreateCommand())
            {
                if (useStoredProcedure)
                {
                    cmd.CommandText = tableName;
                    cmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    cmd.CommandText = $"SELECT * FROM {tableName}";
                }

                cmd.CommandTimeout = _connectionTimeOut;
                using (var da = new SqlDataAdapter())
                {
                    da.SelectCommand = cmd;
                    using (var ds = new DataSet())
                    {
                        da.Fill(ds);
                        return ds;
                    }
                }
            }
        }

        /// <summary>
        /// Construct a SQL CREATE TABLE command for the given Simio Table.
        /// The SQL columns are built from both the Simio Columns and StateColumns
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string BuildSqlCreateCommandFromSimioTable(ITable table, string tableName)
        {
            try
            {
                bool firstColumn = true;
                string sqlCreate = $"CREATE TABLE [{tableName}] (";
                // Add Property Columns
                foreach (var col in table.Columns)
                {
                    if (col.Name != "Id")
                    {
                        if (firstColumn == false)
                        {
                            sqlCreate += ", ";
                        }
                        else
                        {
                            firstColumn = false;
                        }

                        if (col.IsKey)
                        {
                            sqlCreate += $"[{col.Name}] nvarchar(100) not null primary key";
                        }
                        else
                        {
                            sqlCreate += $"[{col.Name}] {GetSimioTableColumnType(col)} Default '{col.DefaultString}'";
                        }
                    }
                }

                // Add State Columns
                foreach (var stateCol in table.StateColumns)
                {
                    if (stateCol.Name != "Id")
                    {
                        if (firstColumn == false)
                        {
                            sqlCreate += ", ";
                        }
                        else
                        {
                            firstColumn = false;
                        }
                        sqlCreate += $"[{stateCol.Name}] {GetSimioTableStateColumnType(stateCol)}";
                    }
                }

                // Add Id Column, which are non-null IDENTITY columns
                if (firstColumn == false)
                {
                    sqlCreate += ", Id int not null identity(1, 1)";
                }
                else
                {
                    sqlCreate += "Id int not null identity(1, 1)";
                }
                sqlCreate += ")";

                return sqlCreate;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot build SQL CREATE command. Err={ex}");
            }
        }


        /// <summary>
        /// Export the SimioTables to the SQL database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tableName"></param>
        /// <param name="tablesSaved"></param>
        public static void SaveSimioTablesToDB(IModel model, string tableName, ref int tablesSaved)
        {
            CheckConnection();

            tablesSaved = 0;
            var exportConfigTable = model.Tables["TableExportConfig"];
            var exportExcludeForUpdate = model.Tables["TableExportExcludeForUpdate"];

            foreach (var table in model.Tables)
            {
                try
                {
                    if (table.Name == tableName || tableName.Length == 0)
                    {
                        using (var cmd = _connection.CreateCommand())
                        {
                            //  Get a list of table exports if found in exportConfigTable and enabled
                            var export = exportConfigTable.Rows.OfType<IRow>()
                                .Where(r => r.Properties["TableName"].Value.Trim().ToLowerInvariant() == table.Name.Trim().ToLowerInvariant()
                                    && r.Properties["Enabled"].Value.Trim().ToLowerInvariant() == "true")
                                .ToList();


                            if (export.Count > 0)
                            {
                                // match simio table name with desired Sql server table name
                                string SQLTableName = export[0].Properties["SQLServerTableName"].Value;
                                if (SQLTableName == "")
                                    SQLTableName = table.Name;

                                Int32 actionInt = 4;
                                if (export[0].Properties["Action"].Value == "DropCreateAndRepopulate") actionInt = 0;
                                else if (export[0].Properties["Action"].Value == "TruncateAndRepopulate") actionInt = 1;
                                else if (export[0].Properties["Action"].Value == "UpdateAndInsert") actionInt = 2;
                                else if (export[0].Properties["Action"].Value == "UpdateInsertAndDelete") actionInt = 3;

                                string preSaveStoredProcedure = export[0].Properties["PreSaveStoredProcedure"].Value;
                                string postSaveStoredProcedure = export[0].Properties["PostSaveStoredProcedure"].Value;

                                CopySimioTableToSqlTable(table, SQLTableName, exportExcludeForUpdate, cmd, actionInt, preSaveStoredProcedure, postSaveStoredProcedure, ref tablesSaved);

                            } // is exportCount > 0
                        } // using cmd
                    } // tablename exists
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Table={table.Name} Err={ex.Message}");
                }
            }
        }

        /// <summary>
        /// Given a Simio Log, produce a DataTable object (with the name tableName).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="runtimeLog"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static DataTable ConvertLogToDataTableWithTypes<T>(IRuntimeLog<T> runtimeLog,
            string tableName
             )
                where T : IRuntimeLogRecord
        {
            string marker = "";
            DataTable dt = new DataTable(tableName);

            if (!runtimeLog.Any())
                return dt;

            try
            {
                // Create a DataTable Column for each property of the Simio Log
                marker = $"Creating DataTable Columns for Log Properties";
                List<string> columnNames = new List<string>();
                foreach (PropertyInfo pi in typeof(T).GetProperties())
                {
                    Type propertyType = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                    Type validType = GetValidClrType(propertyType);
                    dt.Columns.Add(pi.Name, validType);
                }

                // If needed, find the method the fetches the value for the custom columns using the DisplayName
                // We'll use it below as we invoke it for each custom column in each record.
                MethodInfo GetCustomValueMethod = null;

                if (runtimeLog.RuntimeLogExpressions != null)
                {
                    marker = $"Creating DataTable colums for custom log expressions";
                    // ... and also create a Column for each Custom Column
                    foreach (ILogExpression logExpression in runtimeLog.RuntimeLogExpressions)
                    {
                        var dataFormat = logExpression.DataFormat; // Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                        Type columnType;
                        switch ( dataFormat )
                        {
                            case ExpressionDataFormat.Real:
                                {
                                    columnType = typeof(double);
                                }
                                break;
                            case ExpressionDataFormat.Integer:
                                {
                                    columnType = typeof(int);
                                }
                                break;
                            case ExpressionDataFormat.Boolean:
                                {
                                    columnType = typeof(bool);
                                }
                                break;
                            case ExpressionDataFormat.DateTime:
                                {
                                    columnType = typeof(DateTime);
                                }
                                break;
                            case ExpressionDataFormat.TimeSpan:
                                {
                                    columnType = typeof(double);
                                }
                                break;
                            case ExpressionDataFormat.String:
                                {
                                    columnType = typeof(string);
                                }
                                break;
                            case ExpressionDataFormat.Color:
                                {
                                    columnType = typeof(string);
                                }
                                break;

                            default:
                                columnType = typeof(string);
                                break;
                        }
                        //Type validType = GetValidClrType(propertyType);
                        Type validType = GetValidClrType(columnType);
                        dt.Columns.Add(logExpression.DisplayName, validType);
                    }

                    // If we have any runtimelogExpressions, then create a reference
                    // it the GetCustomColumnValue method, which we'll use when create a datarow.
                    if (runtimeLog.RuntimeLogExpressions.Any())
                    {
                        marker = "Getting Method to retrieve value for log expressions";
                        foreach (MethodInfo mi in typeof(T).GetMethods())
                        {
                            if (mi.Name == "GetCustomColumnValue")
                            {
                                ParameterInfo[] parameters = mi.GetParameters();
                                if (parameters.Length == 1)
                                {
                                    GetCustomValueMethod = mi;
                                    goto DoneLookingForMethod;
                                }
                            }
                        DoneLookingForMethod:;
                        }
                    } // Do we have any custom columns?
                } // Check if we have access to RuntimeLogRecords

                int recordCount = 0;
                // Create a DataRow (and add it to the DataTable) from:
                // 1. The properties in each RuntimeLogRecord
                // 2. The custome properties (if any) using GetCustomValueMethod
                foreach (var record in runtimeLog)
                {
                    recordCount += 1;
                    DataRow dr = dt.NewRow();

                    marker = $"Adding DataRows. Record={recordCount}. Log Properties.";
                    // Look at each property in the record and get its name and value (as a string)
                    foreach (PropertyInfo pi in typeof(T).GetProperties())
                    {
                        try
                        {
                            object fieldValue = pi.GetValue(record);

                            fieldValue = FixValueForDatabase(fieldValue);
                            dr[pi.Name] = fieldValue;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Record={recordCount}. Column={pi.Name} Err={ex}");
                        }

                    } // for each property

                    // Now add the Custom columns
                    // Invoke our previously found method on the current record for each name of custom columns.
                    marker = $"Adding DataRows. Record={recordCount}. Log Expressions.";
                    foreach (ILogExpression logExpression in runtimeLog.RuntimeLogExpressions)
                    {
                        string expressionName = logExpression.DisplayName;

                        try
                        {
                            object fieldValue = GetCustomValueMethod?.Invoke(record, new object[] { expressionName });

                            fieldValue = FixValueForDatabase(fieldValue);
                            dr[expressionName] = fieldValue;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Record={recordCount}. Custom Column={expressionName} Err={ex}");
                        }
                    }

                    dt.Rows.Add(dr);
                } // for each record

                dt.AcceptChanges();
                return dt;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Table(Log)={tableName} Error={ex.Message}");
            }

        }

        /// <summary>
        /// Make adjustments to the fieldvalue so that it can survive BulkCopy:
        /// 1. Null is replaced with DBNull
        /// 2. Floating points with NaN are replaced with DBNull.
        /// 3. DateTimes that are Min or Max valued are replaced with DBNull.
        /// 4. Floating points with Infinity are replaced with MAX SQL value
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private static object FixValueForDatabase( object fieldValue )
        {
            try
            {
                if (fieldValue == null)
                {
                    fieldValue = DBNull.Value;
                }
                else
                {
                    TypeCode tc = Type.GetTypeCode(fieldValue.GetType());

                    // Floating point NotANumber (NaN) throw database errors. Convert to nulls.
                    switch (tc)
                    {
                        case TypeCode.DateTime:
                            DateTime dtValue = (DateTime)fieldValue;

                            if (dtValue == DateTime.MaxValue || dtValue == DateTime.MinValue)
                                fieldValue = DBNull.Value;

                            break;

                        case TypeCode.Single:
                            if (Single.IsNaN((Single)fieldValue))
                                fieldValue = DBNull.Value;
                            else if ( Single.IsPositiveInfinity((Single)fieldValue))
                            {
                                fieldValue = MaxSqlFloat;
                            }
                            else if ( Single.IsNegativeInfinity((Single)fieldValue))
                            {
                                fieldValue = MinSqlFloat;
                            }

                            break;

                        case TypeCode.Double:
                            if (Double.IsNaN((double)fieldValue))
                                fieldValue = DBNull.Value;
                            else if (Double.IsPositiveInfinity((Double)fieldValue))
                            {
                                fieldValue = MaxSqlFloat;
                            }
                            else if (Double.IsNegativeInfinity((Double)fieldValue))
                            {
                                fieldValue = MinSqlFloat;
                            }
                            break;

                    } // switch
                }

                return fieldValue;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error fixing FieldValue. Err={ex}");
            }
        }

        /// <summary>
        /// Given a TargetResults 'table', produce a DataTable object (with the name tableName).
        /// The 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetResults"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static DataTable ConvertTargetResultsToDataTable(ITargetResults targetResults,
            string tableName
             )
        {
            string marker = "";
            DataTable dt = new DataTable(tableName);

            if (!targetResults.Any())
                return dt;

            try
            {
                // Create a DataTable Column for each property of the Target Results
                marker = $"Creating DataTable Columns for Target Properties";
                List<string> columnNames = new List<string>();
                StringBuilder sb1 = new StringBuilder();
                foreach (PropertyInfo pi in typeof(ITargetResult).GetProperties())
                {
                    Type validType = GetValidClrType(pi.PropertyType);
                    dt.Columns.Add(pi.Name, validType);
                    sb1.AppendLine($" [{pi.Name} Type={validType.Name}]");
                }

                int recordCount = 0;
                StringBuilder sb2 = new StringBuilder();
                // Create a DataRow (and add it to the DataTable) from:
                // 1. The properties in each RuntimeLogRecord
                // 2. The custome properties (if any) using GetCustomValueMethod
                int nn = targetResults.ToList().Count();

                foreach (var record in targetResults.ToList())
                {
                    recordCount += 1;
                    DataRow dr = dt.NewRow();

                    marker = $"Adding DataRows. Record={recordCount}. TargetResults Properties.";

                    try
                    {
                        // Look at each property in the record and get its name and value (as a string)
                        foreach (PropertyInfo pi in typeof(ITargetResult).GetProperties())
                        {
                            try
                            {
                                object fieldValue = pi.GetValue(record);
                                fieldValue = FixValueForDatabase(fieldValue);
                                dr[pi.Name] = fieldValue;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException($" Column={pi.Name} Err={ex}");
                            }

                        } // for each property
                        sb2.AppendLine($" {recordCount}. [{dt.Columns[0].ColumnName}={dr[0]}] [{dt.Columns[1].ColumnName}={dr[1]}]"); // debug

                        dt.Rows.Add(dr);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($" Record={recordCount} Err={ex}");
                    }
                } // for each record

                dt.AcceptChanges();
                return dt;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Table(Log)={tableName} Error={ex.Message}");
            }

        }

        internal static DataTable ConvertDataTableToSqlTypes(DataTable dtPlain, List<DbColumnInfo> sqlColumnInfoList)
        {
            return null;
        }

        /// <summary>
        /// A hashset of all the numeric types
        /// </summary>
        private static HashSet<Type> NumericTypes = new HashSet<Type>
        {
        typeof(int), typeof(Int16), typeof(Int32), typeof(Int64),
        typeof(float), typeof(double),
        typeof(byte), typeof(sbyte),
        typeof(uint), typeof(UInt16), typeof(UInt32), typeof(UInt64),
        typeof(decimal)
        };

        /// <summary>
        /// A hashset of all the floating point types (single and double; not decimal)
        /// </summary>
        private static HashSet<Type> FloatingPointTypes = new HashSet<Type>
        {
        typeof(float), typeof(double)
        };

        internal static bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        internal static bool IsFloatingPointType(Type type)
        {
            return FloatingPointTypes.Contains(type) ||
                   FloatingPointTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Look at the connection string to make sure it non-empty.
        /// Then examine the connection and open it.
        /// If it cannot be opened, throw an exception.
        /// </summary>
        private static void CheckConnection()
        {
            if (_connectionString.Length == 0)
            {
                throw new Exception("ConnectionString Is Blank");
            }
            else if (_connection == null)
            {
                SetConnection(_connectionString);
            }

            if (_connection.State == ConnectionState.Closed)
            {
                throw new Exception("Connection Is Closed.  Fix Connection String, Save and Reopen Model");
            }

        }

        /// <summary>
        /// Based upon the action, Delete, Truncate or Create tables,
        /// and then Bulkcopy to the table.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="dt"></param>
        /// <param name="cmd"></param>
        /// <param name="actionInt"></param>
        public static void CopySimioLogToSqlTable(string logName, string SQLlogName, DataTable dt, SqlCommand cmd, int actionInt)
        {
            string marker = "";
            try
            {
                string strCheckTable = $"IF OBJECT_ID('{SQLlogName}', 'U') IS NOT NULL SELECT 'true' ELSE SELECT 'false'";
                marker = $"Check for Table. {strCheckTable}";

                // find table
                cmd.CommandText = strCheckTable;
                cmd.CommandType = CommandType.Text;
                var tableFound = Convert.ToBoolean(cmd.ExecuteScalar());

                if (actionInt == 0 && tableFound == true)
                {
                    string sqlDelete = $"DROP TABLE [{SQLlogName}]";
                    marker = $"Drop Table: {sqlDelete}";
                    cmd.CommandText = sqlDelete;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                    tableFound = false;
                }
                else if (actionInt == 1 && tableFound == true)
                {
                    string sqlTrunc = $"TRUNCATE TABLE [{SQLlogName}]";
                    marker = $"Truncate Table: {sqlTrunc}";
                    cmd.CommandText = sqlTrunc;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

                // Table does not exist
                if (tableFound == false)
                {
                    bool firstColumn = true;
                    string sqlCreate = $"CREATE TABLE [{SQLlogName}] (";
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (firstColumn == false)
                        {
                            sqlCreate += ", ";
                        }
                        else
                        {
                            firstColumn = false;
                        }
                        string sqlType = GetSqlFromClr(col.DataType.ToString());
                        sqlCreate += $"[{col.ColumnName}] {sqlType} ";
                    }
                    if (firstColumn == false)
                    {
                        sqlCreate += ", Id int not null identity(1, 1) primary key)";
                    }
                    else
                    {
                        sqlCreate += "Id int not null identity(1, 1) primary key)";
                    }
                    cmd.CommandText = sqlCreate;
                    marker = $"Create Table: {sqlCreate}";
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

                // if repopulate or new table
                if (actionInt <= 1 || tableFound == false)
                {
                    marker = $"BulkCopy to {SQLlogName}. Timeout={_connectionTimeOut}";
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection))
                    {
                        bulkCopy.BulkCopyTimeout = _connectionTimeOut;
                        bulkCopy.DestinationTableName = SQLlogName;
                        bulkCopy.WriteToServer(dt);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Action={marker} Err={ex}");
            }

        }

        /// <summary>
        /// Based upon the action, Delete, Truncate or Create tables,
        /// and then Bulkcopy or Merge to the SQL table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        /// <param name="cmd"></param>
        /// <param name="actionInt"></param>
        public static void CopySimioTableToSqlTable(ITable table, string tableName, ITable exportExcludeForUpdate, SqlCommand cmd, int actionInt, string preSaveStoredProcedure, string postSaveStoredProcedure, ref int count)
        {
            string marker = "";
            try
            {


                string strCheckTable = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL SELECT 'true' ELSE SELECT 'false'";
                marker = $"Check for Table. {strCheckTable}";

                // find table
                cmd.CommandText = strCheckTable;
                cmd.CommandType = CommandType.Text;
                var tableFound = Convert.ToBoolean(cmd.ExecuteScalar());

                if (actionInt == 0 && tableFound == true)
                {
                    string sqlDelete = $"DROP TABLE [{tableName}]";
                    marker += " Delete={sqlDelete}";
                    cmd.CommandText = sqlDelete;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                    tableFound = false;
                }
                else if (actionInt == 1 && tableFound == true)
                {
                    string sqlTrunc = $"TRUNCATE TABLE [{tableName}]";
                    marker += $" Delete={sqlTrunc}";
                    cmd.CommandText = sqlTrunc;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

                // If the SQL Table does not exist, then create it from the Simio Table
                if (tableFound == false)
                {
                    string sqlCreateCommand = BuildSqlCreateCommandFromSimioTable(table, tableName);
                    marker += $" SQLCreate={sqlCreateCommand}";

                    cmd.CommandText = sqlCreateCommand;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

                // call PreSaveStoredProcedure
                if (preSaveStoredProcedure.Length > 0)
                {
                    marker += $"PreSave={preSaveStoredProcedure}";
                    cmd.CommandText = preSaveStoredProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

                // get db column names...These are retrieved from database in the GetColumnInfoForTable method
                List<GridDataColumnInfo> sqlColumnInfoList = new List<GridDataColumnInfo>();

                marker += " GetColumnInfo";
                foreach (var ci in DirectConnectUtils.GetColumnInfoForTable(tableName, false))
                {
                    sqlColumnInfoList.Add(new GridDataColumnInfo { Name = ci.Name, Type = ci.Type });
                }

                // make sure simio tables and datababase tables align
                marker += " CompareColumns";
                CheckSimioTableColumnsAgainstDatabaseColumns(table, sqlColumnInfoList);

                // get table
                var dt = ConvertSimioTableToDataTable(table, sqlColumnInfoList);

                // if repopulate or new table
                if (actionInt <= 1 || tableFound == false)
                {
                    marker += " BulkCopy";
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection))
                    {
                        bulkCopy.BulkCopyTimeout = _connectionTimeOut;
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dt);
                        count++;
                    }
                }
                else // merge data
                {
                    marker += " Merge";
                    // Merge Data
                    string exceptionMessage = String.Empty;
                    count = MergeData(cmd, dt, table, tableName, exportExcludeForUpdate, actionInt, count, out exceptionMessage);
                    if (exceptionMessage.Length > 0)
                    {
                        throw new Exception(exceptionMessage);
                    }
                }

                // call PostSaveStoredProcedure
                if (postSaveStoredProcedure.Length > 0)
                {
                    marker += $" PreSave={preSaveStoredProcedure}";
                    cmd.CommandText = postSaveStoredProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Table={tableName} Action={actionInt} Marker={marker} Err={ex}");
            }

        }


        /// <summary>
        /// Given a model context and log name, create a DataTable for the log.
        /// Right now it is a switch statement, but a future enhancement would be 
        /// to get the Simio Logs programmatically.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="logName"></param>
        /// <returns></returns>
        private static DataTable GetDataTableWithTypesForLog(IModel model, string logName)
        {
            try
            {
                DataTable dt = null;

                switch (logName)
                {
                    case "ResourceUsageLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.ResourceUsageLog, logName);
                        break;
                    case "ResourceStateLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.ResourceStateLog, logName);
                        break;
                    case "ResourceCapacityLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.ResourceCapacityLog, logName);
                        break;
                    case "ResourceInfoLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.ResourceInfoLog, logName);
                        break;
                    case "ConstraintLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.ConstraintLog, logName);
                        break;
                    case "TransporterUsageLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.TransporterUsageLog, logName);
                        break;
                    case "MaterialUsageLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.MaterialUsageLog, logName);
                        break;
                    case "TaskLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.TaskLog, logName);
                        break;
                    case "TaskStateLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.TaskStateLog, logName);
                        break;
                    case "StateObservationLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.StateObservationLog, logName);
                        break;
                    case "TallyObservationLog":
                        dt = ConvertLogToDataTableWithTypes(model.Plan.TallyObservationLog, logName);
                        break;
                    case "TargetResults":
                        dt = ConvertTargetResultsToDataTable(model.Plan.TargetResults, logName);
                        break;

                    default:
                        throw new Exception($"Log Not Found: {logName}");
                } // switch

                return dt;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot convert Log={logName} to DataTable. Err={ex.Message}");
            }  // try/catch
        }


        /// <summary>
        /// Select a SQL type given a CLR type
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public static string GetSqlFromClr(string clrType)
        {
            string sqlType = "";

            Tuple<string, string> tuple = ClrToSqlList.SingleOrDefault(rr => rr.Item1.ToLower() == clrType.ToLower());
            if (tuple != null)
                sqlType = tuple.Item2;
            else
                sqlType = "NVARCHAR(MAX)";

            return sqlType;
        }

        /// <summary>
        /// Select a valid CLR type that will work with SQL Types
        /// If it is not in the ClrToSql list, then it is set to System.String, which will default to NVARCHAR(MAX)
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public static Type GetValidClrType(Type clrType)
        {
            string validType = "";
            string clrTypeName = clrType.ToString();

            Tuple<string, string> tuple = ClrToSqlList
                .SingleOrDefault(rr => rr.Item1.ToLower() == clrTypeName.ToLower());

            if (tuple != null)
                validType = tuple.Item1;
            else
                validType = "System.String"; // default if we don't know what to do with it

            return Type.GetType(validType);
        }




        /// <summary>
        /// In this version of the log there are a few enhancements:
        /// </summary>
        /// <param name="model"></param>
        /// <param name="askSave"></param>
        /// <param name="showResults"></param>
        public static void SaveSimioLogsToDB(IModel model, ref int count)
        {

            count = 0;
            CheckConnection();

            // How have we decided to save back to the database (LogExportConfig)?
            var logExportConfig = model.Tables["LogExportConfig"];

            using (var cmd = _connection.CreateCommand())
            {
                foreach (var logRow in logExportConfig.Rows)
                {
                    if (logRow.Properties["Enabled"].Value.Trim().ToLowerInvariant() == "true")
                    {
                        string logName = logRow.Properties["LogName"].Value;
                        string SQLlogName = logRow.Properties["SQLServerTableName"].Value;
                        if (SQLlogName == "")
                            SQLlogName = logName;

                        try
                        {
                            DataTable dt = GetDataTableWithTypesForLog(model, logName);

                            Int32 actionInt = 0;
                            if (logRow.Properties["Action"].Value == "DropCreateAndRepopulate") actionInt = 0;
                            else if (logRow.Properties["Action"].Value == "TruncateAndRepopulate") actionInt = 1;

                            CopySimioLogToSqlTable(logName, SQLlogName, dt, cmd, actionInt);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Log={SQLlogName} Err={ex}");
                        }
                    } // check if log enabled
                } // foreach logrow
            } // using command from connection

        }

        /// <summary>
        /// Merge new records with the old.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dt"></param>
        /// <param name="table"></param>
        /// <param name="exportExcludeForUpdate"></param>
        /// <param name="actionInt"></param>
        /// <param name="count"></param>
        /// <param name="exceptionMessage"></param>
        /// <returns></returns>
        public static Int32 MergeData(SqlCommand cmd, DataTable dt, ITable table, 
            string tableName, ITable exportExcludeForUpdate, Int32 actionInt, Int32 count, 
            out string exceptionMessage)
        {
            bool firstColumn = true;
            bool firstUpdateColumn = true;
            bool tempCreated = false;
            string keyColumnName = string.Empty;
            string updateSQL = string.Empty;
            string insertSQL = string.Empty;
            string valuesSQL = string.Empty;
            exceptionMessage = string.Empty;

            // Add Property Columns
            foreach (var col in table.Columns)
            {
                if (col.Name != "Id")
                {
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        if (dataColumn.ColumnName == col.Name)
                        {
                            if (firstColumn == false)
                            {
                                insertSQL += ", ";
                                valuesSQL += ", ";
                            }
                            else
                            {
                                firstColumn = false;
                            }
                            insertSQL += col.Name;
                            valuesSQL += "Temp." + col.Name;
                            if (col.IsKey) keyColumnName = col.Name;
                            else
                            {
                                //  not excluded
                                var excludeUpate = exportExcludeForUpdate
                                    .Rows.OfType<IRow>()
                                    .Where(r => r.Properties["TableName"].Value
                                    .Trim().ToLowerInvariant() == table.Name.Trim().ToLowerInvariant() 
                                        && r.Properties["ColumnName"].Value.Trim().ToLowerInvariant() == col.Name.Trim().ToLowerInvariant())
                                    .ToList();

                                if (excludeUpate.Count == 0)
                                {
                                    if (firstUpdateColumn == false)
                                    {
                                        updateSQL += ", ";
                                    }
                                    else
                                    {
                                        firstUpdateColumn = false;
                                    }
                                    updateSQL += "T." + col.Name + " = Temp." + col.Name;
                                }
                            } // check for 'key' column
                            break;
                        } // If Simio and SQL column names match
                    } // Foreach DataColumn
                }   // Is column name 'Id'?
            } // foreach Simio table column

            // Add State Columns
            foreach (var stateCol in table.StateColumns)
            {
                if (stateCol.Name != "Id")
                {
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        if (dataColumn.ColumnName == stateCol.Name)
                        {
                            if (firstColumn == false)
                            {
                                insertSQL += ", ";
                                valuesSQL += ", ";
                            }
                            else
                            {
                                firstColumn = false;
                            }
                            //  not excluded
                            var excludeUpate = exportExcludeForUpdate
                                .Rows.OfType<IRow>()
                                .Where(r => r.Properties["TableName"].Value.Trim().ToLowerInvariant() == table.Name.Trim().ToLowerInvariant() 
                                    && r.Properties["ColumnName"].Value.Trim().ToLowerInvariant() == stateCol.Name.Trim().ToLowerInvariant())
                                .ToList();

                            if (excludeUpate.Count == 0)
                            {
                                if (firstUpdateColumn == false)
                                {
                                    updateSQL += ", ";
                                }
                                else
                                {
                                    firstUpdateColumn = false;
                                }
                                updateSQL += "T." + stateCol.Name + " = Temp." + stateCol.Name;
                            }
                            insertSQL += stateCol.Name;
                            valuesSQL += "Temp." + stateCol.Name;
                            break;
                        } // do the names match?
                    } // foreach DataColumn
                } // check if name is 'Id'
            } // foreach Simio statecolumn

            try
            {
                if (keyColumnName.Length == 0 && actionInt < 4)
                {
                    throw new Exception("Key column not found in table.  Either add a key column or use Truncate or Drop Options Instead");
                }
                if (updateSQL.Length == 0 && actionInt < 4)
                {
                    throw new Exception("Table has no columns other than the key column to update.  Use Truncate or Drop Options Instead");
                }
                else
                {
                    string sqlCreateTemp = $"SELECT * INTO #TmpTable FROM {tableName} WHERE ID < 0";
                    cmd.CommandText = sqlCreateTemp;
                    cmd.ExecuteNonQuery();
                    cmd.CommandTimeout = _connectionTimeOut;
                    tempCreated = true;

                    //Bulk insert into temp table
                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(_connection))
                    {
                        bulkcopy.BulkCopyTimeout = _connectionTimeOut;
                        bulkcopy.DestinationTableName = "#TmpTable";
                        bulkcopy.WriteToServer(dt);
                        bulkcopy.Close();
                    }

                    string sql;

                    if (actionInt == 4)
                    {
                        sql = $"INSERT INTO {tableName} ( " + insertSQL + " ) ";
                        sql += "SELECT  " + valuesSQL + " ";
                        sql += "FROM #TmpTable AS TEMP;";
                    }
                    else
                    {
                        sql = $"MERGE INTO {tableName} AS T USING #TmpTable AS TEMP ";
                        sql += "ON ( T." + keyColumnName + " = TEMP." + keyColumnName + ") ";
                        sql += "WHEN MATCHED THEN UPDATE SET " + updateSQL + " ";
                        sql += "WHEN NOT MATCHED BY TARGET THEN INSERT( " + insertSQL + " ) VALUES ( " + valuesSQL + " ) ";
                        // include delete
                        if (actionInt == 3)
                        {
                            sql += " WHEN NOT MATCHED BY SOURCE THEN DELETE;";
                        }
                        else
                        {
                            sql += ";";
                        }
                    }

                    // Updating destination table, and dropping temp table
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    count++;
                }
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }
            finally
            {
                // Drop Temp Table
                if (tempCreated)
                {
                    cmd.CommandText = "DROP TABLE #TmpTable";
                    cmd.CommandTimeout = _connectionTimeOut;
                    cmd.ExecuteNonQuery();
                }
            }
            return count;
        }

        /// <summary>
        /// A schema check of the Simio Table columns against the SQL database columns.
        /// We are looking to make sure the the column names in Simio tables and SQL table match (case insensitive)
        /// (and vice versa). Detailed exceptions are thrown if this is not the case.
        /// </summary>
        /// <param name="simioTable"></param>
        /// <param name="dbColumnNames"></param>
        internal static void CheckSimioTableColumnsAgainstDatabaseColumns(ITable simioTable, List<GridDataColumnInfo> dbColumnNames)
        {
            foreach (var dbColumnName in dbColumnNames)
            {
                if (dbColumnName.Name == "Id")
                {
                    break; // ?? should this be continue ??
                }
                Boolean foundFlag = false;
                foreach (var col in simioTable.Columns)
                {
                    if (dbColumnName.Name == col.Name)
                    {
                        foundFlag = true;
                        break;
                    }
                }
                if (foundFlag == false) // if not in columns, check the states
                {
                    foreach (var stateCol in simioTable.StateColumns)
                    {
                        if (dbColumnName.Name == stateCol.Name)
                        {
                            foundFlag = true;
                            break;
                        }
                    }
                }
                if (foundFlag == false)
                {
                    string exceptionMessage = $" {dbColumnName.Name} column name exists in database, but not in Simio table.  "
                    + "The column names must be the same.  DropCreateAndRepopulate table in TableExportConfig table to make sure the "
                    + "Simio table structure and the database table structure are aligned.";
                    throw new Exception(exceptionMessage);
                }
            }

            foreach (var col in simioTable.Columns)
            {
                Boolean foundFlag = false;
                foreach (var dbColumnName in dbColumnNames)
                {
                    if (dbColumnName.Name == col.Name)
                    {
                        foundFlag = true;
                        break;
                    }
                }
                if (foundFlag == false)
                {
                    string exceptionMessage = $" {col.Name} column name exists in Simio table, but not in database.  "
                    + "The column names must be the same.  DropCreateAndRepopulate table in TableExportConfig table to make sure the "
                    + "Simio table structure and the database table structure are aligned.";
                    throw new Exception(exceptionMessage);
                }
            }

            foreach (var stateCol in simioTable.StateColumns)
            {
                Boolean foundFlag = false;
                foreach (var dbColumnName in dbColumnNames)
                {
                    if (dbColumnName.Name == stateCol.Name)
                    {
                        foundFlag = true;
                        break;
                    }
                }
                if (foundFlag == false)
                {
                    string exceptionMessage = $"{stateCol.Name} state column name exists in Simio table, but not in database.  "
                    + "The column names must be the same.  DropCreateAndRepopulate table in TableExportConfig table to make sure the "
                    + "Simio table structure and the database table structure are aligned.";
                    throw new Exception(exceptionMessage);
                }
            }
        }

        /// <summary>
        /// Convert a Simio Table to a System.Data.DataTable.
        /// The table can contain either Property or State values
        /// Reals that cannot be parsed or are NaN, infinity, or -infinity are converted to null.
        /// DateTimes that cannot be parsed are converted to null.
        /// The returned result is a Microsoft DataTabe.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="sqlColumnInfoList"></param>
        /// <returns></returns>
        internal static DataTable ConvertSimioTableToDataTable(ITable table, List<GridDataColumnInfo> sqlColumnInfoList)
        {
            List<string[]> tableList = new List<string[]>();
            int rowNumber = 0;

            // get all column names
            List<string> colNames = new List<string>();

            // get property column names
            List<string> propColNames = new List<string>();

            // get property column names
            List<string> colDataTypes = new List<string>();
            List<string> stateColDataTypes = new List<string>();

            // get column data
            foreach (var col in table.Columns)
            {
                foreach (var sqlColumnInfo in sqlColumnInfoList)
                {
                    if (sqlColumnInfo.Name == col.Name)
                    {
                        colNames.Add(col.Name);
                        propColNames.Add(col.Name);
                        colDataTypes.Add(GetSimioTableColumnType(col));
                    }
                }
            }

            // get state column names
            List<string> stateColNames = new List<string>();
            foreach (var stateCol in table.StateColumns)
            {
                foreach (var dbColumnName in sqlColumnInfoList)
                {
                    if (dbColumnName.Name == stateCol.Name)
                    {
                        colNames.Add(stateCol.Name);
                        stateColNames.Add(stateCol.Name);
                        stateColDataTypes.Add(GetSimioTableStateColumnType(stateCol));
                    }
                }
            }
            tableList.Add(colNames.ToArray());

            // Get Row Data
            foreach (var row in table.Rows)
            {
                rowNumber++;
                int arrayIdx = -1;
                List<string> thisRow = new List<string>();
                // get properties
                foreach (var array in propColNames)
                {
                    arrayIdx++;
                    if (row.Properties[array.ToString()].Value != null)
                        thisRow.Add(GetFormattedStringValue(row.Properties[array.ToString()].Value, colDataTypes[arrayIdx]));
                    else thisRow.Add(GetFormattedStringValue("", colDataTypes[arrayIdx]));
                }
                arrayIdx = -1;
                // get states
                foreach (var array in stateColNames)
                {
                    arrayIdx++;
                    if (table.StateRows[rowNumber - 1].StateValues[array.ToString()].PlanValue != null)
                        thisRow.Add(GetFormattedStringValue(table.StateRows[rowNumber - 1].StateValues[array.ToString()].PlanValue.ToString(), stateColDataTypes[arrayIdx]));
                    else thisRow.Add(GetFormattedStringValue("", stateColDataTypes[arrayIdx]));
                }
                tableList.Add(thisRow.ToArray());
            }

            // New table.
            var dataTable = new DataTable();
            dataTable.TableName = table.Name;

            // Get max columns.
            int columns = 0;
            foreach (var array in tableList)
            {
                if (array.Length > columns)
                {
                    columns = array.Length;
                }
            }

            // Add columns.
            for (int cc = 0; cc < columns; cc++)
            {
                var array = tableList[0];
                dataTable.Columns.Add(array[cc]);
            }

            // Remove Column Headings
            if (tableList.Count > 0)
            {
                tableList.RemoveAt(0);
            }

            // sort rows
            //var sortedList = list.OrderBy(x => x[0]).ThenBy(x => x[3]).ToList();

            // Add rows.
            foreach (var array in tableList)
            {
                dataTable.Rows.Add(array);
            }

            return dataTable;
        }

        /// <summary>
        /// Unit Strings, such as length, volume, ...
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static string GetUnitString(IProperty prop)
        {
            IUnitBase unitBase = prop.Unit;

            ITimeUnit timeUnit = unitBase as ITimeUnit;
            if (timeUnit != null)
            {
                return timeUnit.Time.ToString();
            }
            ITravelRateUnit travalrateunit = unitBase as ITravelRateUnit;
            if (travalrateunit != null)
            {
                return travalrateunit.TravelRate.ToString();
            }
            ILengthUnit lengthunit = unitBase as ILengthUnit;
            if (lengthunit != null)
            {
                return lengthunit.Length.ToString();
            }
            ICurrencyUnit currencyunit = unitBase as ICurrencyUnit;
            if (currencyunit != null)
            {
                return currencyunit.Currency.ToString();
            }
            IVolumeUnit volumeunit = unitBase as IVolumeUnit;
            if (volumeunit != null)
            {
                return volumeunit.Volume.ToString();
            }
            IMassUnit massunit = unitBase as IMassUnit;
            if (massunit != null)
            {
                return massunit.Mass.ToString();
            }
            IVolumeFlowRateUnit volumeflowrateunit = unitBase as IVolumeFlowRateUnit;
            if (volumeflowrateunit != null)
            {
                return volumeflowrateunit.Volume.ToString() + "/" + volumeflowrateunit.Time.ToString();
            }
            IMassFlowRateUnit massflowrateunit = unitBase as IMassFlowRateUnit;
            if (massflowrateunit != null)
            {
                return massflowrateunit.Mass.ToString() + "/" + massflowrateunit.Time.ToString();
            }
            ITravelAccelerationUnit timeaccelerationunit = unitBase as ITravelAccelerationUnit;
            if (timeaccelerationunit != null)
            {
                return timeaccelerationunit.Length.ToString() + "/" + timeaccelerationunit.Time.ToString();
            }
            ICurrencyPerTimeUnit currencepertimeunit = unitBase as ICurrencyPerTimeUnit;
            if (currencepertimeunit != null)
            {
                return currencepertimeunit.CurrencyPerTimeUnit.ToString();
            }

            return "none";
        }

        /// <summary>
        /// Format the valueString according to its Simio dataType.
        /// Note: TryParse is used to avoid exception raising.
        /// </summary>
        /// <param name="valueString"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static string GetFormattedStringValue(String valueString, String dataType)
        {

            switch (dataType)
            {
                case "int":
                    {
                        if (valueString.Length > 0)
                        {
                            if (Int64.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out long intProp))
                            {
                                valueString = intProp.ToString(_cultureInfo);
                            }
                            else
                                valueString = null;
                        }
                        else
                            valueString = null;
                    }
                    break;

                case "real":
                    {
                        if (valueString.Length > 0)
                        {
                            switch (valueString.ToLower())
                            {
                                case "\u221E":
                                case "infinity":
                                    {
                                        valueString = MaxSqlFloat.ToString();
                                    }
                                    break;

                                case "-\u221E":
                                case "-infinity":
                                    {
                                        valueString = (MinSqlFloat).ToString();
                                    }
                                    break;

                                case "nan":
                                    {
                                        valueString = null;
                                    }
                                    break;

                                default:
                                    {
                                        if (Double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleProp))
                                        {
                                            valueString = doubleProp.ToString(_cultureInfo);
                                        }
                                        else
                                            valueString = null;
                                    }
                                    break;
                            }
                        }
                        else
                            valueString = null;
                    }
                    break;

                case "datetime":
                    {
                        if (valueString.Length > 0)
                        {
                            if (DateTime.TryParse(valueString, out DateTime dateProp))
                            {
                                valueString = dateProp.ToString(_cultureInfo);
                                if (dateProp.Year < 1753 || dateProp.Year > 9999) // SQL range
                                {
                                    valueString = null;
                                }
                            }
                            else
                                valueString = null;

                        }
                        else
                            valueString = null;
                    }
                    break;
                case "bit":
                    {
                        if (valueString.Length > 0)
                        {
                            if (Boolean.TryParse(valueString, out bool boolProp))
                            {
                                valueString = boolProp.ToString(_cultureInfo);
                            }
                            else
                                valueString = null;
                        }
                        else
                            valueString = null;
                    }
                    break;

                default:
                    break;
            }
            return valueString;

        }

        /// <summary>
        /// Get the column type of a Simio table column.
        /// There are more types, but we are only dealing with real, int, datetime, and bit.
        /// Anything else is nvarchar(1000)
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private static string GetSimioTableColumnType(ITableColumn col)
        {

            switch (col)
            {
                case IRealTableColumn cc:
                    return "float";
                case IIntegerTableColumn cc:
                    return "int";
                case IDateTimeTableColumn cc:
                    return "datetime";
                case IBooleanTableColumn cc:
                    return "bit";
                default:
                    return "nvarchar(1000)";
            }

        }

        /// <summary>
        /// Return the SQL type for the State type.
        /// Only simple types (real, int, datetime, bit) are converted,
        /// and anything else is set the nvarchar(1000)
        /// </summary>
        /// <param name="stateCol"></param>
        /// <returns></returns>
        private static string GetSimioTableStateColumnType(ITableStateColumn stateCol)
        {

            switch (stateCol)
            {
                case IRealTableStateColumn cc:
                    return "float";
                case IIntegerTableStateColumn cc:
                    return "int";
                case IDateTimeTableStateColumn cc:
                    return "datetime";
                case IBooleanTableStateColumn cc:
                    return "bit";
                case IStringTableStateColumn cc:
                    return "nvarchar(1000)";
                default:
                    return "nvarchar(1000)";
            }
        }

    }
}
