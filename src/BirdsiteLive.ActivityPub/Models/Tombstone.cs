namespace BirdsiteLive.ActivityPub.Models
{
    public class Tombstone
    {
        public string id { get; set; }
        public readonly string type = "Tombstone";
        public string atomUrl { get; set; }
    }
}