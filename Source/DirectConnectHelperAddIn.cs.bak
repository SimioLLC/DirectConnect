using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using SimioAPI;
using SimioAPI.Extensions;


namespace DirectConnect
{
    public class DirectConnectHelperAddIn : IModelHelperAddIn
    {
        public string Name => "Direct Connect";

        public string Description => "Direct Connect to SQL Server database.";

        public Image Icon => Properties.Resources.Icon;

        // If this example is used as a the basis of another addin make, sure to change this id, so that Simio can disambiguate the 
        //  new addin from this existing example
        public Guid UniqueID => new Guid("1f3e8b78-aa69-4f78-adf8-3d3ed11fc93c");

        public ModelAddInEnvironment AddInEnvironment => ModelAddInEnvironment.InteractiveDesktop | ModelAddInEnvironment.SimioProjectFactoryAPI;

        /// <summary>
        /// Called at the time the model is loaded.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDisposable CreateInstance(IModelHelperContext context)
        {
            Logit( $"Creating ModelHelper Instance. Model={context.Model.Name} #Tables={context.Model.Tables.Count}" );
            Logit( $"Creating ModelHelper Instance. API Name={Name} Description={Description} Guid={UniqueID}");

            // new model setup
            DirectConnectUtils.NewModelSetup(context.Model);

            // get connect string
            var connectionString = context.PropertyValues.FirstOrDefault(p => p.Name == "ConnectionString");
            if (connectionString?.Value != null)
            {
                Logit($"CreateInstance. ConnectionString={connectionString.Value}");
                try
                {
                    if ( string.IsNullOrEmpty(connectionString.Value.ToString()) )
                    {
                        throw new Exception("ConnectionString Is Blank.  Define connection string and re-enable model helper.");
                    }
                    DirectConnectUtils.SetConnectionAndConnectionString(connectionString.Value.ToString());
                }
                catch (Exception ex)
                {
                    Logit(ex.Message);
                }
            }
            // get connection TimeOut
            var connectionTimeOut = context.PropertyValues.FirstOrDefault(p => p.Name == "ConnectionTimeOut");
            if (connectionTimeOut?.Value != null)
            {
                Logit($"CreateInstance. ConnectionTimeOut={connectionTimeOut.Value}");
                try
                {
                    if ( string.IsNullOrEmpty(connectionTimeOut.Value.ToString()) )
                    {
                        throw new Exception("Connection TimeOut Is Blank.  Define connection timeout and re-enable model helper.");
                    }

                    Int32 connectionTimeOutInt = 0;
                    if (Int32.TryParse(connectionTimeOut.Value.ToString(), out connectionTimeOutInt))
                    {                        
                        DirectConnectUtils.SetConnectionTimeOut(connectionTimeOutInt);
                    }
                    else
                    {
                        throw new Exception("Connection TimeOut Is Not An Integer.  Redefine connection timeout and re-enable model helper.");
                    }
                }
                catch (Exception ex)
                {
                    Logit(ex.Message);
                }
            }

            // get date time format
            var dateTimeFormatProp = context.PropertyValues.FirstOrDefault(p => p.Name == "DateTimeFormat");
            if ( dateTimeFormatProp?.Value != null)
            {
                Logit($"CreateInstance. DateTimeFormat={dateTimeFormatProp.Value}");
                try
                {
                    if ( string.IsNullOrEmpty(dateTimeFormatProp.Value.ToString()) )
                    {
                        throw new Exception("DateTimeFormat string Is Blank.  Define tihe DateTime Format String and re-enable model helper.");
                    }
                    DirectConnectUtils.SetDateTimeFormatString(dateTimeFormatProp.Value.ToString());

                }
                catch (Exception ex)
                {
                    Logit(ex.Message);
                }
            }
            return new DirectConnectOnSaveInstance(context);
        }

        /// <summary>
        /// The below is executed as soon as the Model is loaded.
        /// It is used for doing things like setting up properties to be used later.
        /// </summary>
        /// <param name="schema"></param>
        public void DefineSchema(IModelHelperAddInSchema schema)
        {
            Logit("Defining ModelHelper Schema with default Properties");

            var connectionStringProp = schema.Properties.AddStringProperty("ConnectionString");
            connectionStringProp.DisplayName = "Connection String";
            connectionStringProp.Description = "Microsoft SQL Server Connection String.";
            connectionStringProp.DefaultValue = String.Empty;

            var connectionTimeOutProp = schema.Properties.AddStringProperty("ConnectionTimeOut");
            connectionTimeOutProp.DisplayName = "Connection TimeOut (seconds)";
            connectionTimeOutProp.Description = "Connection TimeOut in Seconds.";
            connectionTimeOutProp.DefaultValue = "600";

            var dateTimeFormatProp = schema.Properties.AddStringProperty("DateTimeFormat");
            dateTimeFormatProp.DisplayName = "DateTime Format String";
            dateTimeFormatProp.Description = "DateTime Format String Used To Save To Database (e.g. yyyy-MM-dd HH:mm:ss).  String value need to be defined.";
            dateTimeFormatProp.DefaultValue = "yyyy-MM-dd HH:mm:ss";
        }

        private void Logit(string message)
        {
            Loggerton.Instance.LogIt(message);
        }

    }

    class DirectConnectOnSaveInstance : IDisposable
    {
        readonly IModelHelperContext _context;

        public DirectConnectOnSaveInstance(IModelHelperContext context)
        {
            _context = context;

            // The context contains various model events you can subscribe to. You will usually
            //  want to unsubscribe from the event in the implementation of Dispose()
            _context.ModelSaved += _context_ModelSaved;
            _context.ModelTablesImporting += _context_ModelTablesImporting;
            _context.ModelTablesImported += _context_ModelTablesImported;
        }

        private void _context_ModelTablesImported(IModelTablesImportedArgs args)
        {
            Alert("ModelTablesImported Event");
        }

        private void _context_ModelTablesImporting(IModelTablesImportingArgs args)
        {
            Alert("ModelTablesImporting Event");
        }

        private void SaveDataToSql(string message, string caption, out bool saveTables, out bool saveLogs)
        {
            saveTables = true;
            saveLogs = true;

            SaveToDatabaseWindow saveWindow = new SaveToDatabaseWindow(caption, message);

            DialogResult result = saveWindow.ShowDialog();
            Logit(message);

            if ( result == DialogResult.OK )
            {
                saveTables = saveWindow.SaveTables;
                saveLogs = saveWindow.SaveLogs;
            }
        }

        private void _context_ModelSaved(IModelSavedArgs args)
        {
            //
            // This is the handler for the "Model Saved" event, this is the bulk of the "helping" logic
            //
            bool isInteractive = _context.AddInEnvironment == ModelAddInEnvironment.InteractiveDesktop;

            try
            {

                // First the tables
                Boolean saveTables = true;
                Boolean saveLogs = true;

                if (isInteractive)
                {
                    SaveDataToSql("Export Simio Tables/Logs Back To SQL Server?", "Export Tables/Logs To SQL Server",
                    out saveTables, out saveLogs);
                }

                StringBuilder sbSaveMessage = new StringBuilder();

                if ( saveTables )
                {
                    int tablesSaved = 0;
                    DirectConnectUtils.SaveSimioTablesToDB(_context.Model, String.Empty, ref tablesSaved);
                    sbSaveMessage.AppendLine($"Done. Number Of Tables Exported: {tablesSaved}");
                } // non-interactive, or user says ok to save

                if ( saveLogs )
                {
                    int logsSaved = 0;
                    DirectConnectUtils.SaveSimioLogsToDB(_context.Model, ref logsSaved );
                    sbSaveMessage.AppendLine($"Done. Number Of Logs Exported: {logsSaved}");
                } // non-interactive, or user says ok to save

                ShowStatus(isInteractive, "Exported", sbSaveMessage.ToString() );

            }
            catch (Exception ex)
            {
                ShowStatus(isInteractive, "Export Error", $"Error Saving: {ex.Message}");
            }
        }

        /// <summary>
        /// Show the status window, and wait for the user to respond.
        /// </summary>
        /// <param name="message"></param>
        private void ShowStatus(bool isInteractive, string title, string message)
        {
            if (isInteractive == false)
            {
                Logit(message);
            }
            else
            {
                using (StatusWindow statWin = new StatusWindow(title, message))
                {
                    statWin.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Show the status window, and wait for the user to respond.
        /// </summary>
        /// <param name="message"></param>
        private DialogResult ShowYesNo(bool isInteractive, string title, string message)
        {
            if (isInteractive == false)
            {
                Logit(message);
                return DialogResult.Ignore;
            }
            else
            {
                using (SaveToDatabaseWindow queryWindow = new SaveToDatabaseWindow(title, message))
                {
                    DialogResult result = queryWindow.ShowDialog();
                    return result;
                }
            }
        }

        internal void Logit(string message)
        {
            Loggerton.Instance.LogIt(message);
        }

        internal void Alert(string message)
        {
            SaveToDatabaseWindow saveWindow = new SaveToDatabaseWindow("Alert", message);

            saveWindow.Show();
            Logit(message);
        }

        public void Dispose()
        {
            DirectConnectUtils.ClearConnectionAndConnectionString();

            // Unsubscribing from the events here, as we no longer need to listen to them,
            _context.ModelSaved -= _context_ModelSaved;
            _context.ModelTablesImporting -= _context_ModelTablesImporting;
            _context.ModelTablesImported -= _context_ModelTablesImported;
        }

    }
}
