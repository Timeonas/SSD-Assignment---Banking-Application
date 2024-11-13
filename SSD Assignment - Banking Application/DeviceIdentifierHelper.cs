using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class DeviceIdentifierHelper
    {
        public static string GetDeviceIdentifier()
        {
            try
            {
                // Option 1: Get IP Address
                string ipAddress = Dns.GetHostAddresses(Dns.GetHostName())
                                      .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                                      .ToString();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return ipAddress;
                }

                // Option 2: Get MAC Address if IP is not available or preferred
                string macAddress = NetworkInterface.GetAllNetworkInterfaces()
                                   .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up
                                                       && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)?
                                   .GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(macAddress))
                {
                    return macAddress;
                }

                // Option 3: Get User SID if MAC and IP are not preferred
                string userSID = WindowsIdentity.GetCurrent().User?.Value;
                if (!string.IsNullOrEmpty(userSID))
                {
                    return userSID;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving device identifier: " + ex.Message);
            }

            // Default if no identifier found
            return "UnknownDevice";
        }
    }
}
