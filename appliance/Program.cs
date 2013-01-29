using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace appliance
{
    public class Program
    {
        public static readonly byte[] MAC = new byte[] { 0x90, 0xA2, 0xDA, 0x0D, 0x32, 0xBF };
        public static readonly string ADDRESS = "192.168.1.101";
        public static readonly string NETMASK = "255.255.255.0";
        public static readonly string GATEWAY = "192.168.1.1";

        public static void Main()
        {
            SecretLabs.NETMF.Net.Wiznet5100 wiznet = new SecretLabs.NETMF.Net.Wiznet5100(SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D2);
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[0];
            networkInterface.PhysicalAddress = MAC;
            networkInterface.EnableStaticIP(ADDRESS, NETMASK, GATEWAY);
            Appliance app = new Appliance();
            UdpListener listener = new UdpListener(app);
        }

    }
}
