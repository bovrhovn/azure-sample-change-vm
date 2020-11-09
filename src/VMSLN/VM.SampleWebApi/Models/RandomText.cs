using System;

namespace VM.SampleWebApi.Models
{
    public class RandomText
    {
        public string Text { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}