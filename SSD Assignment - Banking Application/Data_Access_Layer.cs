using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;

namespace Banking_Application
{



    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        public void loadBankAccounts()
        {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();

                            //Decrypt each encrypted field
                            ca.accountNo = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(0)), CipherMode.CFB);
                            ca.name = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(1)), CipherMode.CFB);
                            ca.address_line_1 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(2)), CipherMode.CFB);
                            ca.address_line_2 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(3)), CipherMode.CFB);
                            ca.address_line_3 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(4)), CipherMode.CFB);
                            ca.town = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(5)), CipherMode.CFB);

                            // Balance and overdraft amount may not be encrypted
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);

                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();

                            //Decrypt each encrypted field
                            sa.accountNo = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(0)), CipherMode.CFB);
                            sa.name = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(1)), CipherMode.CFB);
                            sa.address_line_1 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(2)), CipherMode.CFB);
                            sa.address_line_2 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(3)), CipherMode.CFB);
                            sa.address_line_3 = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(4)), CipherMode.CFB);
                            sa.town = EncryptionMaker.Decrypt(Convert.FromBase64String(dr.GetString(5)), CipherMode.CFB);

                            //Balance and interest rate may not be encrypted
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);

                            accounts.Add(sa);
                        }
                    }
                }
            }

        public String addBankAccount(Bank_Account ba, string tellerName, string deviceIdentifier)
        {
            //Encrypt PII fields
            string encryptedName = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.name, CipherMode.CFB));
            string encryptedAddressLine1 = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.address_line_1, CipherMode.CFB));
            string encryptedAddressLine2 = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.address_line_2, CipherMode.CFB));
            string encryptedAddressLine3 = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.address_line_3, CipherMode.CFB));
            string encryptedTown = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.town, CipherMode.CFB));
            string encryptedAccountNo = Convert.ToBase64String(EncryptionMaker.Encrypt(ba.accountNo, CipherMode.CFB));

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            INSERT INTO Bank_Accounts 
            (accountNo, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate)
            VALUES (@accountNo, @name, @addressLine1, @addressLine2, @addressLine3, @town, @balance, @accountType, @overdraftAmount, @interestRate)
        ";

                //Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@accountNo", encryptedAccountNo);
                command.Parameters.AddWithValue("@name", encryptedName);
                command.Parameters.AddWithValue("@addressLine1", encryptedAddressLine1);
                command.Parameters.AddWithValue("@addressLine2", encryptedAddressLine2);
                command.Parameters.AddWithValue("@addressLine3", encryptedAddressLine3);
                command.Parameters.AddWithValue("@town", encryptedTown);
                command.Parameters.AddWithValue("@balance", ba.balance);
                command.Parameters.AddWithValue("@accountType", ba.GetType() == typeof(Current_Account) ? 1 : 2);

                if (ba.GetType() == typeof(Current_Account))
                {
                    //Current Account specific field
                    Current_Account ca = (Current_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", ca.overdraftAmount);
                    command.Parameters.AddWithValue("@interestRate", DBNull.Value);
                }
                else
                {
                    //Savings Account specific field
                    Savings_Account sa = (Savings_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", DBNull.Value);
                    command.Parameters.AddWithValue("@interestRate", sa.interestRate);
                }

                command.ExecuteNonQuery();
            }

            // Call Logger to log transaction
            Logger.LogTransaction(tellerName, ba.accountNo, ba.name, "Account Creation", deviceIdentifier);

            return ba.accountNo;
        }


        public Bank_Account findBankAccountByAccNo(String accNo, string tellerName, string deviceIdentifier) 
        { 
        
            foreach(Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    //Log the balance qeury or account information retrieval
                    Logger.LogTransaction(tellerName, accNo, ba.name, "Balance Query", deviceIdentifier);
                    return ba;
                }

            }

            return null; 
        }

        public bool closeBankAccount(String accNo, string tellerName, string deviceIdentifier) 
        {

            Bank_Account toRemove = null;
            
            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                //Call Log
                Logger.LogTransaction(tellerName, accNo, toRemove.name, "Account Closure", deviceIdentifier);
                return true;
            }

        }

        //Method to lodge moeny into account
        public bool lodge(String accNo, double amountToLodge, string tellerName, string deviceIdentifier)
        {
            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }
            }

            if (toLodgeTo == null)
                return false;
            else
            {
                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE Bank_Accounts SET balance = balance + @amount WHERE accountNo = @accNo";
                        command.Parameters.AddWithValue("@amount", amountToLodge);
                        command.Parameters.AddWithValue("@accNo", accNo);
                        command.ExecuteNonQuery();
                    }

                    //Ask the user for the reason
                    string reason = string.Empty;
                    if (amountToLodge > 10000)
                    {
                        do
                        {
                            Console.WriteLine("Amount exceeds €10,000. Please provide a reason for the lodgement:");
                            reason = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 5)
                            {
                                Console.WriteLine("Reason is too short. Please provide a valid reason.");
                            }

                        } while (string.IsNullOrWhiteSpace(reason) || reason.Length < 5);
                    }
                    else
                    {
                        reason = "Standard transaction.";
                    }

                    //Log the transaction
                    Logger.LogTransaction(tellerName, accNo, toLodgeTo.name, "Lodgement", deviceIdentifier, $"{reason} (Amount: {amountToLodge})");

                    //Confirm the transaction to the user
                    Console.WriteLine($"Successfully lodged €{amountToLodge} into account {accNo}.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during lodgement: {ex.Message}");
                    return false;
                }
            }
        }


        //Method for withdrawing the money
        public bool withdraw(string accNo, double amountToWithdraw, string tellerName, string deviceIdentifier)
        {
            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }
            }

            if (toWithdrawFrom == null || result == false)
            {
                Console.WriteLine("Withdrawal failed: Account not found or insufficient funds.");
                return false;
            }
            else
            {
                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE Bank_Accounts SET balance = balance - @amount WHERE accountNo = @accNo";
                        command.Parameters.AddWithValue("@amount", amountToWithdraw);
                        command.Parameters.AddWithValue("@accNo", accNo);
                        command.ExecuteNonQuery();
                    }

                    // Prompt the user for the reason
                    string reason = string.Empty;
                    if (amountToWithdraw > 10000)
                    {
                        do
                        {
                            Console.WriteLine("Amount exceeds €10,000. Please provide a reason for the withdrawal:");
                            reason = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 5)
                            {
                                Console.WriteLine("Reason is too short. Please provide a valid reason.");
                            }
                        } while (string.IsNullOrWhiteSpace(reason) || reason.Length < 5);
                    }
                    else
                    {
                        reason = "Standard transaction.";
                    }

                    //Log the transaction
                    Logger.LogTransaction(tellerName, accNo, toWithdrawFrom.name, "Withdrawal", deviceIdentifier, $"{reason} (Amount: {amountToWithdraw})");

                    //Confirm the transaction to the user
                    Console.WriteLine($"Successfully withdrew €{amountToWithdraw} from account {accNo}.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during withdrawal: {ex.Message}");
                    return false;
                }
            }
        }


    }
}
