using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Banking_Application
{
    public class Program
    {
        // Variable to store current user role
        public static string currentUserRole = "";

        public static void Main(string[] args)
        {
            DatabaseStuff.SetupDatabase();
            //DatabaseStuff.HashExistingPasswords();
        
    

            
            //Authenticate User
            //if (!AuthenticateUser())
            //{
            //    //Exit the application if authentication fails
            //    return;
            //}
            
            Console.WriteLine("Enter Teller Name:");
            string tellerName = Console.ReadLine();
            string deviceIdentifier = DeviceIdentifierHelper.GetDeviceIdentifier();

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

                switch (option)
                {
            case "1":
                        String accountType = "";
                        int loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
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
                        Console.WriteLine("Enter Account Number: ");
                        string accNoInput = Console.ReadLine();

                        var ba1 = dal.findBankAccountByAccNo(accNoInput, tellerName, deviceIdentifier);
                        if (ba1 is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba1.ToString());

                            // Commented out administrator approval
                            /*
                            Console.WriteLine("Administrator approval is required to delete this account.");
                            Console.WriteLine("Enter Administrator Username:");
                            string adminUsername = Console.ReadLine();

                            Console.WriteLine("Enter Administrator Password:");
                            string adminPassword = ReadPassword();

                            try
                            {
                                using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
                                {
                                    if (context.ValidateCredentials(adminUsername, adminPassword))
                                    {
                                        using (var adminUser = UserPrincipal.FindByIdentity(context, adminUsername))
                                        {
                                            if (adminUser != null && adminUser.IsMemberOf(context, IdentityType.SamAccountName, "Bank Teller Administrator User Group"))
                                            {
                                                Console.WriteLine("Approval Granted. Proceeding with deletion.");
                                                dal.closeBankAccount(accNoInput, tellerName, deviceIdentifier);
                                                Console.WriteLine("Account deleted successfully.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Access Denied: User is not an administrator.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid administrator credentials. Deletion aborted.");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error during administrator approval: {ex.Message}");
                            }
                            */

                            // Direct deletion logic without administrator approval
                            Console.WriteLine("Proceeding with deletion without administrator approval.");
                            dal.closeBankAccount(accNoInput, tellerName, deviceIdentifier);
                            Console.WriteLine("Account deleted successfully.");
                        }
                        break;
                    case "3":
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

                            if (withdrawalOK == false)
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

        public static bool AuthenticateUser()
        {
            Console.WriteLine("Enter Username: ");
            string username = Console.ReadLine();

            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();

            try
            {
                string domain = "ITSLIGO.LAN";
                string ldapServer = "ldap://ITSLIGO.LAN";

                using (var ldapConnection = new LdapConnection(ldapServer))
                {
                    ldapConnection.Credential = new NetworkCredential(username, password, domain);
                    ldapConnection.AuthType = AuthType.Negotiate;

                    // Authenticate by binding to the server
                    ldapConnection.Bind();

                    // If we reach this point, authentication succeeded
                    Console.WriteLine("Authentication successful!");
                    LogLoginAttempt(username, "Success", "User authenticated successfully.");

                    // Check roles (optional, based on your Active Directory setup)
                    currentUserRole = IsUserInGroup(username, domain, "Bank Teller Administrator User Group")
                        ? "Admin"
                        : "Teller";

                    Console.WriteLine($"Login Successful! Role: {currentUserRole}");
                    return true;
                }
            }
            catch (LdapException ex)
            {
                Console.WriteLine($"LDAP Authentication failed: {ex.Message}");
                LogLoginAttempt(username, "Failure", "Invalid credentials or insufficient permissions.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
                LogLoginAttempt(username, "Failure", ex.Message);
                return false;
            }
        }

        // Helper method to check group membership
        private static bool IsUserInGroup(string username, string domain, string groupName)
        {
            try
            {
                string ldapServer = "ldap://ITSLIGO.LAN";
                using (var ldapConnection = new LdapConnection(ldapServer))
                {
                    ldapConnection.AuthType = AuthType.Negotiate;

                    // Specify search filter
                    string filter = $"(&(objectClass=user)(sAMAccountName={username})(memberOf=CN={groupName},CN=Users,DC=ITSLIGO,DC=LAN))";

                    // Search in the directory
                    var searchRequest = new SearchRequest("DC=ITSLIGO,DC=LAN", filter, SearchScope.Subtree);
                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    return searchResponse.Entries.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        // Utility for masking password input
        public static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                password.Append(key.KeyChar);
            }
            return password.ToString();
        }

        // Event Logging Method
        public static void LogLoginAttempt(string username, string status, string details)
        {
            string logEntry = $"WHO: {username}, STATUS: {status}, DETAILS: {details}, WHEN: {DateTime.Now}";
            try
            {
                if (!EventLog.SourceExists("SSD Banking Application"))
                {
                    EventLog.CreateEventSource("SSD Banking Application", "Application");
                }
                EventLog.WriteEntry("SSD Banking Application", logEntry, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to event log: {ex.Message}");
            }
        }
    }

    public static class PrincipalExtensions
    {
        public static bool IsMemberOf(this Principal principal, PrincipalContext context, IdentityType identityType, string groupName)
        {
            using (var group = GroupPrincipal.FindByIdentity(context, identityType, groupName))
            {
                return group != null && group.Members.Contains(principal);
            }
        }
    }
}
            