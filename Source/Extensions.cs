using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectConnect
{
    /// <summary>
    /// Use to quelch an OLE DB error, where NaN values were
    /// causing BulkCopy to fail.
    /// See SO:
    /// https://stackoverflow.com/questions/23229862/weird-ole-db-provider-stream-for-linked-server-null-returned-invalid-data
    /// </summary>
    public static class MyToNullIfNanExtension
    {
        public static double? MyToNullIfNan(this double? result)
        {
            if (result.HasValue)
            {
                if (double.IsNaN(result.Value) || double.IsInfinity(result.Value))
                {
                    return null;
                }
            }
            return result;
        }

        public static double? MyToNullIfNan(this double result)
        {
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                return null;
            }
            return result;
        }

        public static float? MyToNullIfNan(this float? result)
        {
            if (result.HasValue)
            {
                if (float.IsNaN(result.Value) || float.IsInfinity(result.Value))
                {
                    return null;
                }
            }
            return result;
        }

        public static float? MyToNullIfNan(this float result)
        {
            if (float.IsNaN(result) || float.IsInfinity(result))
            {
                return null;
            }
            return result;
        }

        public static float MyToMinValueIfNan(this float result)
        {
            if (float.IsNaN(result) || float.IsInfinity(result))
            {
                return float.MinValue;
            }

            return result;
        }
    }
}
