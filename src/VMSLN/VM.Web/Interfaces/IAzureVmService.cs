using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;

namespace VM.Web.Interfaces
{
    public interface IAzureVmService
    {
        Task<List<IVirtualMachine>> GetVirtualMachinesAsync();
        Task<IVirtualMachine> GetMachineByIdAsync(string id);
        Task<bool> ChangeStateAsync(string id,bool start = false);
        Task<bool> ResizeMachineAsync(string id, string size);
    }
}