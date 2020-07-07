using System.Collections.Generic;

namespace BirdsiteLive.Models.WellKnownModels
{
    public class WebFingerResult
    {
        public string subject { get; set; }
        public string[] aliases { get; set; }
        public List<WebFingerLink> links { get; set; } = new List<WebFingerLink>();
    }
}