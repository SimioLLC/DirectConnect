using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectConnect
{
    /// <summary>
    /// Each log entry has Flags to indicate type.
    /// </summary>
    [Flags]
    public enum EnumLogFlags
    {
        None = 0,
        Information = 1,
        Event = 2,
        Warning = 4,
        Error = 8,
        All = 0xffff
    }


    /// <summary>
    /// A single entry into the log.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// A way to categorize logs
        /// </summary>
        public EnumLogFlags Flags { get; set; }

        /// <summary>
        /// A DateTimeOffset for when the log was entered
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// The actual log message.
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// What is the index of this entry? 0 based.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Logs are grouped into pages. 
        /// This concept is used to remove old entries (See also PageSize)
        /// </summary>
        public int PageNumber {  get { return Index / Loggerton.PageSize; } }

        /// <summary>
        /// Has this message been excluded by regex excludes?
        /// </summary>
        public bool IsExcluded { get; set; }

        public LogEntry(EnumLogFlags flags, String msg)
        {
            Flags = flags;
            TimeStamp = DateTime.UtcNow;
            Message = msg;
        }

        public override string ToString()
        {
            string ss = "";
            if (Flags == EnumLogFlags.None)
                ss = $"{TimeStamp.ToString("HH:mm:ss.ff")} {Message}";
            else
                ss = $"{Flags}:{TimeStamp.ToString("HH:mm:ss.ff")} {Message}";

            return ss;
        }
    }

    /// <summary>
    /// A singleton (in memory logger)
    /// </summary>
    public sealed class Loggerton
    {
        private static readonly Loggerton instance = new Loggerton();
        public static Loggerton Instance { get { return instance; } }

        static Loggerton()
        {
            
        }
        private Loggerton()
        {
            MaxEntries = 3000;
            PageSize = 30;
        }

        /// <summary>
        /// How many log entries on a page
        /// </summary>
        public static int PageSize = 30;

        /// <summary>
        /// How many entries to store before removal/writing
        /// </summary>
        public int MaxEntries { get; set; }

        public bool StoreRemovedPages { get; set; } = false;

        /// <summary>
        /// Set to write to Documents in constructor
        /// </summary>
        public string PathForStoring { get; set; }

        /// <summary>
        /// Set this to false to stop all logging.
        /// </summary>
        public bool IsEnabled = true;

        /// <summary>
        /// The logs, in a FIFO format
        /// </summary>
        private Queue<LogEntry> LogBook = new Queue<LogEntry>();

        /// <summary>
        /// A list of regular expressions for marking logs
        /// as 'excluded'
        /// </summary>
        private List<string> ExcludesList = new List<string>();

        /// <summary>
        /// Add the regex excludes as a delimited list.
        /// You pick your own character as the delimiter.
        /// </summary>
        /// <param name="commalist"></param>
        public void SetExcludes(string commalist, char delimiter)
        {
            ExcludesList.Clear();
            ExcludesList = commalist.Split(delimiter).ToList();

            // Reevaluate all the logs
            foreach (LogEntry le in LogBook)
            {
                le.IsExcluded = false;
                foreach (string expr in ExcludesList)
                {
                    if (Regex.IsMatch(le.Message, expr, RegexOptions.IgnoreCase))
                    {
                        le.IsExcluded = true;
                        goto GetNextLogEntry;
                    }
                }
                GetNextLogEntry:;
            }
        }

        /// <summary>
        /// Add a new log entry. If resulting count now exceeds MaxEntries,
        /// then remove the oldest Page.
        /// </summary>
        /// <param name="entry"></param>
        private void AddLogEntry(LogEntry entry)
        {
            LogBook.Enqueue(entry);
            if (LogBook.Count > MaxEntries)
                RemoveLastPage();
        }

        /// <summary>
        /// Remove the oldest log entry
        /// </summary>
        /// <param name="entry"></param>
        private void RemoveLogEntry(LogEntry entry)
        {
            LogBook.Enqueue(entry);
        }

        /// <summary>
        /// Remove the last (oldest) page from the logbook.
        /// If the total count is less than PageSize, then simply return.
        /// </summary>
        public void RemoveLastPage()
        {
            if ( LogBook.Count() <= PageSize)
                return;

            LogEntry oldestEntry = LogBook.Last();

            List<LogEntry> entryList = LogBook
                .TakeWhile(ee => ee.PageNumber == oldestEntry.PageNumber)
                .OrderBy(ee => ee.TimeStamp)
                .ToList();

            // Todo: AppendToFile(entryList)

            foreach ( LogEntry entry in entryList)
            {
                LogBook.Dequeue();
            }

        }

        /// <summary>
        /// Get all logs that should be displayed.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public string GetLogs(EnumLogFlags flags)
        {
            List<LogEntry> filteredLogs = LogBook
                .Where(rr => (rr.Flags | flags) != 0)
                .Where(rr => !rr.IsExcluded)
                .OrderByDescending(rr => rr.TimeStamp)
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (LogEntry le in filteredLogs)
            {
                sb.AppendLine($"{le}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Completely clear the logs without writing.
        /// </summary>
        public void ClearLogs()
        {
            LogBook.Clear();
        }

        /// <summary>
        /// A good method to convert the logs to a string.
        /// Just call and display.
        /// </summary>
        /// <returns></returns>
        public string ShowLogs()
        {
            return LogBook.ToString();
        }

        /// <summary>
        /// Push all the logs out to a file.
        /// </summary>
        /// <param name="path"></param>
        public void WriteLogs(string path)
        {
            File.WriteAllText(path, LogBook.ToString());
        }

        /// <summary>
        /// The simplest logging routine. Just call this with a message.
        /// </summary>
        /// <param name="message"></param>
        public void LogIt( string message)
        {
            if (!IsEnabled)
                return;

            bool isExcluded = false;
            foreach (string expr in ExcludesList)
            {
                if (Regex.IsMatch(message, expr, RegexOptions.IgnoreCase))
                {
                    isExcluded = true;
                    break;
                }
            }

            LogEntry entry = new LogEntry(EnumLogFlags.None, message);
            entry.IsExcluded = isExcluded;
            AddLogEntry(entry);
        }

        /// <summary>
        /// Logging and specifying a type of log (EnumLogFlags)
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        public void LogIt(EnumLogFlags logType, string message)
        {
            if (!IsEnabled)
                return;

            bool isExcluded = false;
            foreach (string expr in ExcludesList)
            {
                if (Regex.IsMatch(message, expr, RegexOptions.IgnoreCase))
                {
                    isExcluded = true;
                    break;
                }
            }

            LogEntry entry = new LogEntry(logType, message);
            entry.IsExcluded = isExcluded;
            AddLogEntry(entry);
        }

    }
}

