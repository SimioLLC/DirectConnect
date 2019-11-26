using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimioAPI.Extensions;
using System.Data;
using System.Data.Common;

namespace DirectConnect
{
    class DirectConnectGridDataProvider
    {
        public class DirectConnectGridDataProviderImpl : IGridDataProvider
        {
            #region IGridDataProvider Members

            public string Name
            {
                get { return "Direct Connect"; }
            }

            public string Description
            {
                get { return "Reads data from Direct Connect"; }
            }

            public System.Drawing.Image Icon
            {
                get { return Properties.Resources.Icon; }
            }

            public Guid UniqueID
            {
                get { return MY_ID; }
            }
            static readonly Guid MY_ID = new Guid("8785DD8D-1CA8-4DFE-8468-D29101469F79");

            public byte[] GetDataSettings(byte[] existingSettings)
            {
                DirectConnectGridDataSettings thesettings = DirectConnectGridDataSettings.FromBytes(existingSettings);
                if (thesettings == null)
                    thesettings = new DirectConnectGridDataSettings();

                DirectConnectSettingsDialog dlg = new DirectConnectSettingsDialog();
                dlg.SetSettings(thesettings);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    return thesettings.ToBytes();

                return existingSettings;
            }

            public IGridDataRecords OpenData(byte[] dataSettings, IGridDataOpenContext openContext)
            {
                DirectConnectGridDataSettings thesettings = DirectConnectGridDataSettings.FromBytes(dataSettings);
                if (thesettings == null || thesettings.TableOrViewName == null)
                    return null;

                return new DirectConnectGridDataRecords(thesettings);
            }

            public string GetDataSummary(byte[] dataSettings)
            {
                DirectConnectGridDataSettings thesettings = DirectConnectGridDataSettings.FromBytes(dataSettings);
                if (thesettings == null || thesettings.TableOrViewName == null) // and maybe check that file exists and can be opened, etc?
                    return null;

                return String.Format("Bound to Direct Connect : {0}", thesettings.TableOrViewName);
            }

            public string GetFileNameIfAny(byte[] dataSettings)
            {
                return null;
            }

            #endregion
        }
    }

    [Serializable]
    class DirectConnectGridDataSettings
    {
        string _tableOrViewName;
        public string TableOrViewName
        {
            get { return _tableOrViewName; }
            set { _tableOrViewName = value; }
        }

        bool _isStoredProcedure;
        public bool IsStoredProcedure
        {
            get { return _isStoredProcedure; }
            set { _isStoredProcedure = value; }
        }

        public static DirectConnectGridDataSettings FromBytes(byte[] settings)
        {
            if (settings == null)
                return null;

            System.IO.MemoryStream memstream = new System.IO.MemoryStream(settings);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            DirectConnectGridDataSettings messettings = (DirectConnectGridDataSettings)fmt.Deserialize(memstream);

            return messettings;
        }
        public byte[] ToBytes()
        {
            System.IO.MemoryStream memstream = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            fmt.Serialize(memstream, this);

            return memstream.ToArray();
        }
    }

    class DirectConnectGridDataRecords : IGridDataRecords
    {
        private DirectConnectGridDataSettings _settings;
        public DirectConnectGridDataRecords(DirectConnectGridDataSettings settings)
        {
            _settings = settings;
        }

        #region IGridDataRecords Members

        List<GridDataColumnInfo> _columnInfo;
        List<GridDataColumnInfo> ColumnInfo
        {
            get
            {
                if (_columnInfo == null)
                {
                    _columnInfo = new List<GridDataColumnInfo>();

                    foreach (var i in DirectConnectUtils.GetColumnInfoForTable(_settings.TableOrViewName, _settings.IsStoredProcedure))
                    {
                        _columnInfo.Add(new GridDataColumnInfo { Name = i.Name, Type = i.Type });
                    }
                }

                return _columnInfo;
            }
        }

        public IEnumerable<GridDataColumnInfo> Columns
        {
            get { return ColumnInfo; }
        }

        #endregion

        #region IEnumerable<IGridDataRecord> Members

        public IEnumerator<IGridDataRecord> GetEnumerator()
        {
            var ds = DirectConnectUtils.GetDataSet(_settings.TableOrViewName,_settings.IsStoredProcedure);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                yield return new DirectConnectGridDataRecord(dr, ds.Tables[0].Columns.Count);
            }
            ds.Clear();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }

    class DirectConnectGridDataRecord : IGridDataRecord
    {
        private readonly DataRow _dr;
        public DirectConnectGridDataRecord(DataRow dr, Int32 numberOfColumns)
        {
            _dr = dr; ;
        }

        #region IGridDataRecord Members

        public string this[int index]
        {
            get
            {
                var theValue = _dr[index];

                // Simio will first try to parse dates in the current culture
                if (theValue is DateTime)
                    return ((DateTime)theValue).ToString();

                return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", _dr[index]);
            }
        }

        #endregion
    }

}
