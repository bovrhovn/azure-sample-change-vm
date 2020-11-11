using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Interfaces;
using VM.Web.Options;

namespace VM.Web.Services
{
    public class AzureVmService : IAzureVmService
    {
        private readonly ILogger<AzureVmService> logger;
        private readonly IAzure azure;

        public AzureVmService(IOptions<AzureAdOptions> azureAdOptionsValue, ILogger<AzureVmService> logger)
        {
            this.logger = logger;
            var adOptions = azureAdOptionsValue.Value;

            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(adOptions.ClientId, adOptions.ClientSecret,
                    adOptions.TenantId, AzureEnvironment.AzureGlobalCloud);

            azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithSubscription(adOptions.SubscriptionId);
        }

        public async Task<List<IVirtualMachine>> GetVirtualMachinesAsync()
        {
            logger.LogInformation("Getting machines");
            var virtualMachines = await azure.VirtualMachines.ListAsync();
            return virtualMachines.ToList();
        }

        public async Task<IVirtualMachine> GetMachineByIdAsync(string id)
        {
            var virtualMachines = await azure.VirtualMachines.ListAsync(true, CancellationToken.None);
            var virtualMachine = virtualMachines.FirstOrDefault(d => d.VMId == id);
            return virtualMachine;
        }

        public async Task<bool> ChangeStateAsync(string id, bool start = false)
        {
            try
            {
                var machine = await GetMachineByIdAsync(id);
                logger.LogInformation($"Starting to change the state at {DateTime.Now}");
                if (start)
                    await azure.VirtualMachines.StartAsync(machine.ResourceGroupName, machine.Name,
                        CancellationToken.None);
                else
                    await azure.VirtualMachines.PowerOffAsync(machine.ResourceGroupName, machine.Name,
                        CancellationToken.None);
                logger.LogInformation($"State changed at {DateTime.Now}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> ResizeMachineAsync(string id, string size)
        {
            try
            {
                var virtualMachine = await GetMachineByIdAsync(id);

                logger.LogInformation($"Doing resizing with size {size}");

                var startWatch = Stopwatch.StartNew();
                startWatch.Start();

                //stop the VM
                await azure.VirtualMachines.PowerOffAsync(virtualMachine.ResourceGroupName, virtualMachine.Name,
                    CancellationToken.None);

                //do the change
                var newSize = ExpandableStringEnum<VirtualMachineSizeTypes>.Parse(size);
                virtualMachine.Inner.HardwareProfile.VmSize = newSize;

                //update the status
                virtualMachine.Update();

                //start the machine
                await azure.VirtualMachines.StartAsync(virtualMachine.ResourceGroupName, virtualMachine.Name,
                    CancellationToken.None);

                startWatch.Stop();
                
                logger.LogInformation(
                    $"Machine size changing took {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds / 1000} s)");

                startWatch.Start();

                if (virtualMachine.PowerState == PowerState.Stopped ||
                    virtualMachine.PowerState == PowerState.Deallocated)
                {
                    logger.LogInformation("Starting the machine as it has stopped or been deallocated.");
                    await virtualMachine.StartAsync();
                }

                startWatch.Stop();

                logger.LogInformation(
                    $"Machine updated with new size {size} and started machine in {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds / 1000} s).");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return false;
            }

            return true;
        }
    }
}