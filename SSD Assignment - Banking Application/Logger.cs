using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class Logger
    {
        private static readonly string AppName = "SSD Banking Application";
        private static readonly string AppVersion = System.Reflection.Assembly.GetExecutingAssembly()
                                                      .GetName()
                                                      .Version
                                                      .ToString();
        private static readonly string AppHash = GetApplicationHash();

        public static void LogTransaction(
            string tellerName,
            string accountNo,
            string accountHolderName,
            string transactionType,
            string deviceIdentifier,
            string reason = "",
            double amount = 0.0,
            EventLogEntryType logType = EventLogEntryType.Information // New parameter for log level
        )
        {
            try
            {
                string logEntry = $@"
                WHO-1: {tellerName},
                WHO-2: {accountNo}, {accountHolderName},
                WHAT: {transactionType},
                WHERE: {deviceIdentifier},
                WHEN: {DateTime.Now},
                WHY: {reason},
                HOW: {AppName}, Version: {AppVersion}, Hash: {AppHash}
            ";

                // Write log to Windows Event Log
                if (!EventLog.SourceExists(AppName))
                {
                    EventLog.CreateEventSource(AppName, "Application");
                }
                EventLog.WriteEntry(AppName, logEntry, logType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging transaction: {ex.Message}");
            }
        }

        public static void LogEvent(string message, EventLogEntryType logType = EventLogEntryType.Information)
        {
            try
            {
                if (!EventLog.SourceExists(AppName))
                {
                    EventLog.CreateEventSource(AppName, "Application");
                }
                EventLog.WriteEntry(AppName, message, logType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging event: {ex.Message}");
            }
        }

        private static string GetApplicationHash()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(File.ReadAllBytes(exePath));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}