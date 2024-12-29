using Microsoft.Data.Sqlite;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Banking_Application.Program;

namespace SSD_Assignment___Banking_Application
{
    public class DatabaseStuff
    {
        private static readonly string ConnectionString = "Data Source=Banking Database.db";

        // Method to get a database connection
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        // Method to execute raw SQL commands
        public static void ExecuteSql(string sql)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;

                try
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("SQL executed successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing SQL: {ex.Message}");
                }
            }
        }

        // Method to set up the BankAccounts table
        public static void SetupDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // Create the BankAccounts table if it doesn't exist
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Bank_Accounts (
                accountNo TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                address_line_1 TEXT NOT NULL,
                address_line_2 TEXT,
                address_line_3 TEXT,
                town TEXT NOT NULL,
                balance REAL NOT NULL,
                accountType INTEGER NOT NULL,
                overdraftAmount REAL,
                interestRate REAL
            );
        ";
                command.ExecuteNonQuery();

                Console.WriteLine("BankAccounts table ensured!");
            }
        }


        

    }
}
