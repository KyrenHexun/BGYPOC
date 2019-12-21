using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    public static class Logger
    {
        public static void WriteLog(string content, EventLogEntryType logType)
        {
            EventLog myLog = new EventLog();
            myLog.Source = "MSCRM";
            myLog.WriteEntry(content, logType);
        }
        public static void WriteLog(string sourceName, string content, EventLogEntryType logType)
        {
            EventLog myLog = new EventLog();
            myLog.Source = sourceName;
            myLog.WriteEntry(content, logType);
        }
    }
}
