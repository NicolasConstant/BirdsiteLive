using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Tests
{
    [TestClass]
    public class ApDeserializerTests
    {
        [TestMethod]
        public void FollowDeserializationTest()
        {
            var json = "{ \"@context\":\"https://www.w3.org/ns/activitystreams\",\"id\":\"https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe\",\"type\":\"Follow\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":\"https://4a120ca2680e.ngrok.io/users/manu\"}";

            var data = ApDeserializer.ProcessActivity(json) as ActivityFollow;

            Assert.AreEqual("https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe", data.id);
            Assert.AreEqual("Follow", data.type);
            Assert.AreEqual("https://4a120ca2680e.ngrok.io/users/manu", data.apObject);
        }

        [TestMethod]
        public void UndoDeserializationTest()
        {
            var json =
                "{\"@context\":\"https://www.w3.org/ns/activitystreams\",\"id\":\"https://mastodon.technology/users/testtest#follows/225982/undo\",\"type\":\"Undo\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":{\"id\":\"https://mastodon.technology/c94567cf-1fda-42ba-82fc-a0f82f63ccbe\",\"type\":\"Follow\",\"actor\":\"https://mastodon.technology/users/testtest\",\"object\":\"https://4a120ca2680e.ngrok.io/users/manu\"}}";

            var data = ApDeserializer.ProcessActivity(json) as ActivityUndoFollow;
            Assert.AreEqual("https://mastodon.technology/users/testtest#follows/225982/undo", data.id);
            Assert.AreEqual("Undo", data.type);
            Assert.AreEqual("Follow", data.apObject.type);
            Assert.AreEqual("https://mastodon.technology/users/testtest", data.apObject.actor);
            Assert.AreEqual("https://4a120ca2680e.ngrok.io/users/manu", data.apObject.apObject);
        }

        [TestMethod]
        public void AcceptDeserializationTest()
        {
            var json = "{\"@context\":\"https://www.w3.org/ns/activitystreams\",\"id\":\"https://mamot.fr/users/testtest#accepts/follows/333879\",\"type\":\"Accept\",\"actor\":\"https://mamot.fr/users/testtest\",\"object\":{\"id\":\"https://85da1577f778.ngrok.io/f89dfd87-f5ce-4603-83d9-405c0e229989\",\"type\":\"Follow\",\"actor\":\"https://85da1577f778.ngrok.io/users/gra\",\"object\":\"https://mamot.fr/users/testtest\"}}";


            var data = ApDeserializer.ProcessActivity(json) as ActivityAcceptFollow;
            Assert.AreEqual("https://mamot.fr/users/testtest#accepts/follows/333879", data.id);
            Assert.AreEqual("Accept", data.type);
            Assert.AreEqual("https://mamot.fr/users/testtest", data.actor);
            Assert.AreEqual("https://85da1577f778.ngrok.io/f89dfd87-f5ce-4603-83d9-405c0e229989", data.apObject.id);
            Assert.AreEqual("https://85da1577f778.ngrok.io/users/gra", data.apObject.actor);
            Assert.AreEqual("Follow", data.apObject.type);
            Assert.AreEqual("https://mamot.fr/users/testtest", data.apObject.apObject);
        }
    }
}