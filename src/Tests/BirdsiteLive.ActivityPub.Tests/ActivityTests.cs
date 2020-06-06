using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Tests
{
    //[TestClass]
    //public class ActivityTests
    //{
    //    [TestMethod]
    //    public void FollowDeserializationTest()
    //    {
    //        var json = "{ \"@context\":\"https://www.w3.org/ns/activitystreams\",\"id\":\"https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe\",\"type\":\"Follow\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":\"https://4a120ca2680e.ngrok.io/users/manu\"}";

    //        var data = JsonConvert.DeserializeObject<Activity>(json);

    //        Assert.AreEqual("https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe", data.id);
    //        Assert.AreEqual("Follow", data.type);
    //        Assert.AreEqual("https://4a120ca2680e.ngrok.io/users/manu", data.apObject);
    //    }

    //    [TestMethod]
    //    public void UndoDeserializationTest()
    //    {
    //        var json =
    //            "{\"@context\":\"https://www.w3.org/ns/activitystreams\",\"id\":\"https://mastodon.technology/users/testtest#follows/225982/undo\",\"type\":\"Undo\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":{\"id\":\"https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe\",\"type\":\"Follow\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":\"https://4a120ca2680e.ngrok.io/users/manu\"}}";

    //        var data = JsonConvert.DeserializeObject<Activity>(json);
    //        Assert.AreEqual("https://mastodon.technology/users/testtest#follows/225982/undo", data.id);
    //        Assert.AreEqual("Undo", data.type);
    //        Assert.AreEqual("https://4a120ca2680e.ngrok.io/users/manu", data.apObject);
    //    }
    //}
}
