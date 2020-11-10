using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VM.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Local Console API to test out the connectivity between migration of the VM");

            var url = Environment.GetEnvironmentVariable("TEST_URL"); 
            
            //initial call from local machine to test out the API
            var client = new HttpClient();
            while (true) //TODO: background process for checking out functionality
            {
                var receivedInput = await client.GetStringAsync(url);
                System.Console.WriteLine(receivedInput);
                await Task.Delay(2000); //delay for 2 seconds and call again
            }
        }
    }
}