namespace BirdsiteLive.Models.WellKnownModels
{
    public class NodeInfoV21
    {
        public string version { get; set; }
        public string[] protocols { get; set; }
        public Usage usage { get; set; }
        public bool openRegistrations { get; set; }
        public SoftwareV21 software { get; set; }
        public Services services { get; set; }
        public Metadata metadata { get; set; }
    }
}