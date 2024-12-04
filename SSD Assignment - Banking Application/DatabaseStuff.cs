using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Banking_Application.Program;

namespace SSD_Assignment___Banking_Application
{
    public  class DatabaseStuff
    {
        private static readonly string ConnectionString = "Data Source=Banking Database.db";

        //Method to get a database connection
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        //Method to execute raw SQL commands
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

        //method to update and add databases
        public static void SetupDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                //Create the Users table if it doesn't exist
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                userId INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password TEXT NOT NULL,
                salt TEXT,
                role TEXT NOT NULL
            );
            
            INSERT OR IGNORE INTO Users (username, password, role)
            VALUES 
                ('Adam', 'password123', 'Admin'),
                ('Tim', 'teller123', 'Teller');
        ";
                command.ExecuteNonQuery();

                Console.WriteLine("Users table ensured!");
            }
        }

        //Existing data can be hashed
        public static void HashExistingPasswords()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT userId, password FROM Users WHERE salt IS NULL;";

                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string currentPassword = reader.GetString(1);

                        Console.WriteLine($"Hashing password for userId: {userId}");

                        //Generate a salt and hash the password
                        string salt = SecurityHelper.GenerateSalt();
                        string hashedPassword = SecurityHelper.HashWithSalt(currentPassword, salt);

                        //Update the database
                        var updateCommand = connection.CreateCommand();
                        updateCommand.CommandText = @"
                    UPDATE Users
                    SET password = @password, salt = @salt
                    WHERE userId = @userId;
                ";
                        updateCommand.Parameters.AddWithValue("@password", hashedPassword);

                        updateCommand.Parameters.AddWithValue("@salt", salt);
                        updateCommand.Parameters.AddWithValue("@userId", userId);

                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        Console.WriteLine($"Updated userId: {userId}, Rows affected: {rowsAffected}");
                    }
                }
            }

            Console.WriteLine("Existing passwords hashed and salts added!");
        }

    }
}
