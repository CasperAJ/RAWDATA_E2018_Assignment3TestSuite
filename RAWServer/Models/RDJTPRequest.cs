using System;
namespace RAWServer.Models
{
    public class RDJTPRequest
    {
        public RDJTPRequest()
        {
        }



        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }
    }
}