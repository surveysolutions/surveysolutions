using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Client.Properties;

namespace Client
{
    class WifiFileArchive: ISaveFileArchive
    {
        private string network;
        private List<string> targetList;

        public WifiFileArchive()
        {

        }

        public void SaveArchive(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadArchive()
        {
            throw new NotImplementedException();
        }

        public string getTargetName()
        {
            throw new NotImplementedException();
        }
 
        public void FlushTargetList()
        {
            List<string> networks = new List<string>();
            NetworkInterface[] networkCards = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkCards)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    networks.Add(networkInterface.Name);
                }

            }
            this.targetList = networks;
        }
    }
}
