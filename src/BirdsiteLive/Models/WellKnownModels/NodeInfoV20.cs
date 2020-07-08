using System.ComponentModel.DataAnnotations;

namespace BirdsiteLive.Models.WellKnownModels
{
    public class NodeInfoV20
    {
        public string version { get; set; }
        public string[] protocols { get; set; }
        public Software software { get; set; }
        public Usage usage { get; set; }
        public bool openRegistrations { get; set; }
        public Services services { get; set; }
        public Metadata metadata { get; set; }
    }
}