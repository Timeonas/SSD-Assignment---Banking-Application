using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SSD_Assignment___Banking_Application
{
    public static class Logger
    {
        public static void LogTransaction(string tellerName, string accountNo, string accountHolderName, string transactionType, string deviceIdentifier, string reason = "")
        {
            string logEntry = $"WHO-1: {tellerName}, WHO-2: {accountNo}, {accountHolderName}, WHAT: {transactionType}, WHERE: {deviceIdentifier}, WHEN: {DateTime.Now}, WHY: {reason}";
            EventLog.WriteEntry("SSD Banking Application", logEntry, EventLogEntryType.Information);
        }
    }
}
