/*******
 * A Few Notes:
 1.  Hit some syntax errors, The credential and SubscriptionId not working 
     due to outdated of Microsoft Azure Management Storage Library, after upgrading to 
     the 6.3.0-preview, fixed the syntax error in creating storage.  
     So, sync up all the Libraries to preview version. Fixed

 2. Crashed when creating the Storage. Found storageName must be all lowercase. Fixed.

 **/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//RL Added
using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Rest;


namespace CreateVM_V1._0
{
    class Program
    {


        private const string clientAppId = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        private const string clientAppsecret = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";


        private const string groupName = "RLResourceGroup0002";
        private const string subscriptionId = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
        private const string storageName = "rlstorage0002";   // lowercase only, otherwise all kinds of errors
        private const string ipName = "RLPublicIP0002";
        private const string subnetName = "RLSubnet0002";
        private const string vnetName = "RLVnet0002";
        private const string nicName = "RLNIC0002";
        private const string avSetName = "RLAVSet0002";
        private const string vmName = "RLVM0002";
        private const string location = "West US";
        private const string adminName = "XXXXXXXX";
        private const string adminPassword = "XXXXXXX";


        private const string NSGName = "RLNSG0002";
        private const string NSGRuleName = "RLRule0002";





        static void Main(string[] args)
        {

            //RL Added 
            /**************************************************************************
            Step 1: Create the credentials used to authenticate requests
            Before you start this step, make sure that you have access to an Active Directory service principal. 
                From the service principal, you acquire a token for authenticating requests to Azure Resource Manager.
            ****************************************************************************/

            // To get the token that's needed to create the credentials
            var token = GetAccessTokenAsync();
            var credential = new TokenCredentials(token.Result.AccessToken);




            /******************************************************************************
             Step 2: Create the resources
             Register the providers and create a resource group
             All resources must be contained in a resource group. Before you can add resources to a group, 
             your subscription must be registered with the resource providers.

            This Step includes the followings:
            1. Register the providers and create a resource group
            2. Create the resource group and register the providers
            3. Create a storage account
            4. Create a public IP address
            5. Create a virtual network
            6. Create a network interface
            7. Create an availability set
            8. Create a virtual machine


             *******************************************************************************/



            // Register the providers and create a resource group
            // To create the resource group and register the providers,
            // Tested and working
            var rgResult = CreateResourceGroupAsync(  credential,
                                                      groupName,
                                                      subscriptionId,
                                                      location);
            Console.WriteLine(rgResult.Result.Properties.ProvisioningState);
            Console.WriteLine(" ");

            //  Console.ReadLine();



            ////Create a storage account
            ////When using an unmanaged disk, a storage account is needed to store the virtual hard disk file that is created for the virtual machine.
            // Tested and working
            var stResult = CreateStorageAccountAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              storageName);
            Console.WriteLine(stResult.Result.ProvisioningState);
            Console.WriteLine(" ");
            //    Console.ReadLine();


            //Create a public IP address
            // Tested and working
            var ipResult = CreatePublicIPAddressAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              ipName);
            Console.WriteLine(ipResult.Result.ProvisioningState);
            Console.WriteLine(" ");
            //     Console.ReadLine();


            //// Create a virtual network
            //// A virtual machine that's created with the Resource Manager deployment model must be in a virtual network.
            // Tested and working
            var vnResult = CreateVirtualNetworkAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              vnetName,
              subnetName);
            Console.WriteLine(vnResult.Result.ProvisioningState);
            Console.WriteLine(" ");
            //    Console.ReadLine();


            //Creating a NSG 
            var NSGResult = CreateNetworkSecurityGroupAsync(
                              credential,
                              groupName,
                              subscriptionId,
                              location);

            Console.WriteLine(" ");
            Console.WriteLine("Creating NSG Completed .... Hit Enter to Continue...");
            Console.ReadLine();


            // Creating some NSG rules
            var RuleResult = CreatingRulesAsync(credential,
                              groupName,
                              subscriptionId);
            Console.WriteLine(" ");
            Console.WriteLine("Creating Rule Completed .... Hit Enter to Continue...");
            Console.ReadLine();







            ////Create a network interface
            ////A virtual machine needs a network interface to communicate on the virtual network.
            // Tested and working
            var ncResult = CreateNetworkInterfaceAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              subnetName,
              vnetName,
              ipName,
              nicName);
            Console.WriteLine(ncResult.Result.ProvisioningState);
            Console.WriteLine(" ");
            //     Console.ReadLine();






            ////Create an availability set
            ////Availability sets make it easier for you to manage the maintenance of the virtual machines used by your application.
            var avResult = CreateAvailabilitySetAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              avSetName);
            Console.WriteLine(" ");
            Console.WriteLine("Hit Enter to Continue...");
            Console.ReadLine();

            ////Create a virtual machine
            ////Now that you created all the supporting resources, you can create a virtual machine.
            var vmResult = CreateVirtualMachineAsync(
              credential,
              groupName,
              subscriptionId,
              location,
              nicName,
              avSetName,
              storageName,
              adminName,
              adminPassword,
              vmName);
            Console.WriteLine("In progress, it will take about 10 minutes...");
            Console.WriteLine(vmResult.Result.ProvisioningState);
            Console.WriteLine(" ");
            Console.WriteLine("Hit Enter to Delete the Resource Group and Finish...");
            Console.ReadLine();





            /*************************************************************************** 
            Step 3: Delete the resources
            Because you are charged for resources used in Azure, it is always good practice to delete resources that are no longer needed.
            If you want to delete the virtual machines and all the supporting resources, all you have to do is delete the resource group.
            ****************************************************************************/
            //DeleteResourceGroupAsync(
            //  credential,
            //  groupName,
            //  subscriptionId);
            //Console.WriteLine("Hit Enter to Close this Window...");
            //Console.ReadLine();




        }

        /****************************Methods****************************************************/
        ////RL Added
        //Step 1: Create the credentials used to authenticate requests
        //To get the token that's needed to create the credentials
        private static async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            var cc = new ClientCredential(clientAppId, clientAppsecret);
            var context = new AuthenticationContext("https://login.windows.net/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            var token = await context.AcquireTokenAsync("https://management.azure.com/", cc);
            if (token == null)
            {
                throw new InvalidOperationException("Could not get the token");
            }
            return token;
        }

            /******************************************************************************
             Step 2: Create the resources
             Register the providers and create a resource group
             All resources must be contained in a resource group. Before you can add resources to a group, 
             your subscription must be registered with the resource providers.

            This Step includes the followings:
            1. Register the providers and create a resource group
            2. Create the resource group and register the providers
            3. Create a storage account
            4. Create a public IP address
            5. Create a virtual network(and the Subnet)
            6. Create a network interface (with NSG group)
            7. Create an availability set
            8. Create a virtual machine
            *******************************************************************************/





        //To create the resource group and register the providers, add this method to the Program class:
        public static async Task<ResourceGroup> CreateResourceGroupAsync(
          TokenCredentials credential,
          string groupName,
          string subscriptionId,
          string location)
        {
            var resourceManagementClient = new ResourceManagementClient(credential)
            { SubscriptionId = subscriptionId };

            Console.WriteLine("Registering the providers...");
            var rpResult = resourceManagementClient.Providers.Register("Microsoft.Storage");
            Console.WriteLine(rpResult.RegistrationState);
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Network");
            Console.WriteLine(rpResult.RegistrationState);
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Compute");
            Console.WriteLine(rpResult.RegistrationState);

            Console.WriteLine("Creating the resource group...");
            var resourceGroup = new ResourceGroup { Location = location };
            return await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(
              groupName,
              resourceGroup);
        }

        //Create a storage account
        //When using an unmanaged disk, a storage account is needed to store the virtual hard disk file that is created for the virtual machine.
        public static async Task<StorageAccount> CreateStorageAccountAsync(TokenCredentials credential, string groupName, string subscriptionId, string location, string storageName)
        {
            try
            {
                var storageManagementClient = new StorageManagementClient(credential)
                { SubscriptionId = subscriptionId };

                Console.WriteLine("Creating the storage account...");
                return await storageManagementClient.StorageAccounts.CreateAsync(groupName, storageName,
                    new StorageAccountCreateParameters
                    {
                        Sku = new Microsoft.Azure.Management.Storage.Models.Sku { Name = SkuName.StandardLRS },
                        Kind = Kind.Storage,
                        Location = location
                    }
                );
            }
            catch (AggregateException exp)
            {
                throw new Exception(exp.ToString());
            }
        }




        //Create a public IP address
        //A public IP address is needed to communicate with the virtual machine.
        public static async Task<PublicIPAddress> CreatePublicIPAddressAsync(
                                                  TokenCredentials credential,
                                                  string groupName,
                                                  string subscriptionId,
                                                  string location,
                                                  string ipName)
        {
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };

            Console.WriteLine("Creating the public ip...");

            return await networkManagementClient.PublicIPAddresses.CreateOrUpdateAsync(
              groupName,
              ipName,
              new PublicIPAddress
              {
                  Location = location,
                  PublicIPAllocationMethod = "Dynamic"
              }
            );
        }


        // Create a virtual network
        // A virtual machine that's created with the Resource Manager deployment model must be in a virtual network.
        public static async Task<VirtualNetwork> CreateVirtualNetworkAsync(
          TokenCredentials credential,
          string groupName,
          string subscriptionId,
          string location,
          string vnetName,
          string subnetName)
        {
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };

            var subnet = new Subnet
            {
                Name = subnetName,
                AddressPrefix = "10.0.0.0/24"
            };

            var address = new AddressSpace
            {
                AddressPrefixes = new List<string> { "10.0.0.0/16" }
            };

            Console.WriteLine("Creating the virtual network...");
            return await networkManagementClient.VirtualNetworks.CreateOrUpdateAsync(
              groupName,
              vnetName,
              new VirtualNetwork
              {
                  Location = location,
                  AddressSpace = address,
                  Subnets = new List<Subnet> { subnet }
              }
            );
        }




        public static async Task<NetworkSecurityGroup> CreateNetworkSecurityGroupAsync(
                                    TokenCredentials credential,
                                              string groupName,
                                              string subscriptionId,
                                              string location)
        {

            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };


            Console.WriteLine("Creating the Network Security Group...");
            return await networkManagementClient.NetworkSecurityGroups.BeginCreateOrUpdateAsync(
                groupName,
                NSGName,
                new NetworkSecurityGroup
                { Location = location }
                );





        }



        public static async Task<SecurityRule> CreatingRulesAsync(
                            //        public static async Task<IList<SecurityRule>> CreatingRulesAsync(
                            TokenCredentials credential,
                                      string groupName,
                                      string subscriptionId)
        {

            var NetworkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };

            Console.WriteLine("Creating Rules for Network Security Group...");


            //http://windowsitpro.com/azure/manage-network-security-groups-powershell for example
            var a = await NetworkManagementClient.SecurityRules.BeginCreateOrUpdateAsync(
                groupName,
                NSGName,
                NSGRuleName,
                new SecurityRule()
                {
                    Priority = 101,
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "3389",
                    Protocol = "TCP",
                    SourceAddressPrefix = "INTERNET",
                    SourcePortRange = "*",
                    Direction = "Inbound",
                    Description = "Tryout of the Rules",
                    Access = "Allow"

                }
                );

            return (a);

        }








        //Create a network interface
        //A virtual machine needs a network interface to communicate on the virtual network.
        public static async Task<NetworkInterface> CreateNetworkInterfaceAsync(
                                                    TokenCredentials credential,
                                                              string groupName,
                                                              string subscriptionId,
                                                              string location,
                                                              string subnetName,
                                                              string vnetName,
                                                              string ipName,
                                                              string nicName)
        {
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };
            var subnet = await networkManagementClient.Subnets.GetAsync(
              groupName,
              vnetName,
              subnetName
            );
            var publicIP = await networkManagementClient.PublicIPAddresses.GetAsync(
              groupName,
              ipName
            );

            var nsg = await networkManagementClient.NetworkSecurityGroups.GetAsync(groupName, NSGName);

            Console.WriteLine("Creating the network interface...");
            return await networkManagementClient.NetworkInterfaces.CreateOrUpdateAsync(
              groupName,
              nicName,
              new NetworkInterface
              {
                  Location = location,
                  NetworkSecurityGroup =nsg ,
                  IpConfigurations = new List<NetworkInterfaceIPConfiguration>
                  {
                       new NetworkInterfaceIPConfiguration
                      {
                        Name = nicName,
                        PublicIPAddress = publicIP,
                        Subnet = subnet                        
                      }
                  }
              }
            );
        }




        //Create an availability set
        //Availability sets make it easier for you to manage the maintenance of the virtual machines used by your application.
        public static async Task<AvailabilitySet> CreateAvailabilitySetAsync(
          TokenCredentials credential,
          string groupName,
          string subscriptionId,
          string location,
          string avsetName)
        {
            var computeManagementClient = new ComputeManagementClient(credential)
            { SubscriptionId = subscriptionId };

            Console.WriteLine("Creating the availability set...");
            return await computeManagementClient.AvailabilitySets.CreateOrUpdateAsync(
              groupName,
              avsetName,
              new AvailabilitySet()
              { Location = location }
            );
        }


        //Create a virtual machine
        //Now that you created all the supporting resources, you can create a virtual machine.
        public static async Task<VirtualMachine> CreateVirtualMachineAsync(
          TokenCredentials credential,
          string groupName,
          string subscriptionId,
          string location,
          string nicName,
          string avsetName,
          string storageName,
          string adminName,
          string adminPassword,
          string vmName)
        {
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = subscriptionId };
            var computeManagementClient = new ComputeManagementClient(credential)
            { SubscriptionId = subscriptionId };
            var nic = await networkManagementClient.NetworkInterfaces.GetAsync(
              groupName,
              nicName);
            var avSet = await computeManagementClient.AvailabilitySets.GetAsync(
              groupName,
              avsetName);

            Console.WriteLine("Creating the virtual machine...");
            return await computeManagementClient.VirtualMachines.CreateOrUpdateAsync(
              groupName,
              vmName,
              new VirtualMachine
              {
                  Location = location,
                  AvailabilitySet = new Microsoft.Azure.Management.Compute.Models.SubResource
                  {
                      Id = avSet.Id
                  },
                  HardwareProfile = new HardwareProfile
                  {
                      VmSize = "Standard_A0"
                  },
                  OsProfile = new OSProfile
                  {
                      AdminUsername = adminName,
                      AdminPassword = adminPassword,
                      ComputerName = vmName,
                      WindowsConfiguration = new WindowsConfiguration
                      {
                          ProvisionVMAgent = true
                      }
                  },
                  NetworkProfile = new NetworkProfile
                  {
                      NetworkInterfaces = new List<NetworkInterfaceReference>
                        {
                new NetworkInterfaceReference { Id = nic.Id }
                        }
                  },
                  StorageProfile = new StorageProfile
                  {
                      ImageReference = new ImageReference
                      {
                          Publisher = "MicrosoftWindowsServer",
                          Offer = "WindowsServer",
                          Sku = "2012-R2-Datacenter",
                          Version = "latest"
                      },
                      OsDisk = new OSDisk
                      {
                          Name = "mytestod1",
                          CreateOption = DiskCreateOptionTypes.FromImage,
                          Vhd = new VirtualHardDisk
                          {
                              Uri = "http://" + storageName + ".blob.core.windows.net/vhds/mytestod1.vhd"
                          }
                      }
                  }
              }
            );
        }

        /***************************  
        Step 4: Delete the resources
        Because you are charged for resources used in Azure, it is always good practice to delete resources that are no longer needed. If you want to delete the virtual machines and all the supporting resources, all you have to do is delete the resource group.
        *************************/
        public static async void DeleteResourceGroupAsync(
          TokenCredentials credential,
          string groupName,
          string subscriptionId)
        {
            Console.WriteLine("Deleting resource group...");
            var resourceManagementClient = new ResourceManagementClient(credential)
            { SubscriptionId = subscriptionId };
            await resourceManagementClient.ResourceGroups.DeleteAsync(groupName);
        }











    }  //class
}
