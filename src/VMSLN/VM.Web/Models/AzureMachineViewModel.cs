namespace VM.Web.Models
{
    public class AzureMachineViewModel
    {
        public string ResourceGroup { get; set; }
        public string Size { get; set; }
        public string Powerstate { get; set; }
        public string Os { get; set; }
        public string Name { get; set; }
    }
}