using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Banking_Application
{
    public class Program
    {
        //Variable
        public static string currentUserRole = "";

        public static void Main(string[] args)
        {
            //SetupDatabase();


            // Step 1: Authenticate User
            if (!AuthenticateUser())
            {
                // Exit the application if authentication fails
                return;
            }






            Console.WriteLine("Enter Teller Name:");
            string tellerName = Console.ReadLine();
            string deviceIdentifier = DeviceIdentifierHelper.GetDeviceIdentifier();


            //Test if it encryptes
            //Console.WriteLine("Run encryption tests? (y/n): ");
            //string input = Console.ReadLine();
            //if (input?.ToLower() == "y")
            //{
            //    EncriptionTest.RunEncryptionTests();
            //}

            //Test To decrpyt
            //string encryptedBase64 = "nvE1Kj1114m3Xep/sg/qLY1V9ZyyLwqz0vwfEIcU/Ol50W8yf9gWLSvlyI8H8U/kQRVpsKY="; // Replace with your actual encrypted Base64 string

            //try
            //{
            //    // Convert the Base64-encoded string back to a byte array
            //    byte[] encryptedData = Convert.FromBase64String(encryptedBase64);

            //    // Decrypt the data (using the correct mode, e.g., CipherMode.CFB)
            //    string decryptedText = EncryptionMaker.Decrypt(encryptedData, CipherMode.CFB); // Replace with the correct mode if different

            //    Console.WriteLine("Decrypted text: " + decryptedText);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Decryption failed: " + ex.Message);
            //}


            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            dal.loadBankAccounts();
            bool running = true;

            do
            {

                Console.WriteLine("");
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("CHOOSE OPTION:");
                String option = Console.ReadLine();
                
                switch(option)
                {
                    case "1":
                        String accountType = "";
                        int loopCount = 0;
                        
                        do
                        {

                           if(loopCount > 0)
                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            Console.WriteLine("CHOOSE OPTION:");
                            accountType = Console.ReadLine();

                            loopCount++;

                        } while (!(accountType.Equals("1") || accountType.Equals("2")));

                        String name = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Name: ");
                            name = Console.ReadLine();

                            loopCount++;

                        } while (name.Equals(""));

                        String addressLine1 = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID ÀDDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            addressLine1 = Console.ReadLine();

                            loopCount++;

                        } while (addressLine1.Equals(""));

                        Console.WriteLine("Enter Address Line 2: ");
                        String addressLine2 = Console.ReadLine();
                        
                        Console.WriteLine("Enter Address Line 3: ");
                        String addressLine3 = Console.ReadLine();

                        String town = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            town = Console.ReadLine();

                            loopCount++;

                        } while (town.Equals(""));

                        double balance = -1;
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Opening Balance: ");
                            String balanceString = Console.ReadLine();

                            try
                            {
                                balance = Convert.ToDouble(balanceString);
                            }

                            catch 
                            {
                                loopCount++;
                            }

                        } while (balance < 0);

                        Bank_Account ba;

                        if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                        {
                            double overdraftAmount = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Overdraft Amount: ");
                                String overdraftAmountString = Console.ReadLine();

                                try
                                {
                                    overdraftAmount = Convert.ToDouble(overdraftAmountString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (overdraftAmount < 0);

                            ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                        }

                        else
                        {

                            double interestRate = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Interest Rate: ");
                                String interestRateString = Console.ReadLine();

                                try
                                {
                                    interestRate = Convert.ToDouble(interestRateString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (interestRate < 0);

                            ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                        }

                        String accNo = dal.addBankAccount(ba, tellerName, deviceIdentifier);

                        Console.WriteLine("New Account Number Is: " + accNo);

                        break;
                    case "2":

                        //Check for proper admin perms
                        if (currentUserRole != "Admin")
                        {
                            Console.WriteLine("Access Denied: Only admins can close accounts.");
                            break;
                        }

                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo, tellerName, deviceIdentifier);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());

                            String ans = "";

                            do
                            {

                                Console.WriteLine("Proceed With Delection (Y/N)?"); 
                                ans = Console.ReadLine();

                                switch (ans)
                                {
                                    case "Y":
                                    case "y": dal.closeBankAccount(accNo, tellerName, deviceIdentifier);
                                        break;
                                    case "N":
                                    case "n":
                                        break;
                                    default:
                                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                        break;
                                }
                            } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                        }

                        break;
                    case "3":
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo, tellerName, deviceIdentifier);

                        if(ba is null) 
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());
                        }

                        break;
                    case "4": //Lodge
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo, tellerName, deviceIdentifier);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToLodge = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Lodge: ");
                                String amountToLodgeString = Console.ReadLine();

                                try
                                {
                                    amountToLodge = Convert.ToDouble(amountToLodgeString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (amountToLodge < 0);

                            dal.lodge(accNo, amountToLodge, tellerName, deviceIdentifier);
                        }
                        break;
                    case "5": //Withdraw
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo, tellerName, deviceIdentifier);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToWithdraw = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                String amountToWithdrawString = Console.ReadLine();

                                try
                                {
                                    amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (amountToWithdraw < 0);

                            bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw, tellerName, deviceIdentifier);

                            if(withdrawalOK == false)
                            {

                                Console.WriteLine("Insufficient Funds Available.");
                            }
                        }
                        break;
                    case "6":
                        running = false;
                        break;
                    default:    
                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                        break;
                }
                
                
            } while (running != false);

        }

        //Security Access
        public static bool AuthenticateUser()
        {
            Console.WriteLine("Enter Username: ");
            string username = Console.ReadLine();

            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();

            using (var connection = new SqliteConnection("Data Source=Banking Database.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT role FROM Users
            WHERE username = @username AND password = @password;
        ";

                //Use parameterized queries to prevent SQL injection
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password); 

                //Execute the query
                var role = command.ExecuteScalar() as string;

                if (role != null)
                {
                    //Set the user's role if the query finds a match
                    currentUserRole = role;
                    Console.WriteLine($"Login Successful! Role: {role}");
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid credentials. Access Denied.");
                    return false;
                }
            }
        }



        /*
        public static void SetupDatabase()
        {
            using (var connection = new SqliteConnection("Data Source=Banking Database.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();

                // Add schema creation SQL
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                userId INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password TEXT NOT NULL,
                role TEXT NOT NULL
            );
            
            INSERT OR IGNORE INTO Users (username, password, role)
            VALUES 
                ('Adam', 'password123', 'Admin'),
                ('Tim', 'teller123', 'Teller');
        ";

                command.ExecuteNonQuery();

                Console.WriteLine("Database setup complete!");
            }
        }
        */

    }
}