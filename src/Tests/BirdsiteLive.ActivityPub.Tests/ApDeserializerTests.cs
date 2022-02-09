using BirdsiteLive.ActivityPub.Models;
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

        [TestMethod]
        public void DeleteDeserializationTest()
        {
            var json =
                "{\"@context\": \"https://www.w3.org/ns/activitystreams\", \"id\": \"https://mastodon.technology/users/deleteduser#delete\", \"type\": \"Delete\", \"actor\": \"https://mastodon.technology/users/deleteduser\", \"to\": [\"https://www.w3.org/ns/activitystreams#Public\"],\"object\": \"https://mastodon.technology/users/deleteduser\",\"signature\": {\"type\": \"RsaSignature2017\",\"creator\": \"https://mastodon.technology/users/deleteduser#main-key\",\"created\": \"2020-11-19T22:43:01Z\",\"signatureValue\": \"peksQao4v5N+sMZgHXZ6xZnGaZrd0s+LqZimu63cnp7O5NBJM6gY9AAu/vKUgrh4C50r66f9OQdHg5yChQhc4ViE+yLR/3/e59YQimelmXJPpcC99Nt0YLU/iTRLsBehY3cDdC6+ogJKgpkToQvB6tG2KrPdrkreYh4Il4eXLKMfiQhgdKluOvenLnl2erPWfE02hIu/jpuljyxSuvJunMdU4yQVSZHTtk/I8q3jjzIzhgyb7ICWU5Hkx0H/47Q24ztsvOgiTWNgO+v6l9vA7qIhztENiRPhzGP5RCCzUKRAe6bcSu1Wfa3NKWqB9BeJ7s+2y2bD7ubPbiEE1MQV7Q==\"}}";

            var data = ApDeserializer.ProcessActivity(json) as ActivityDelete;

            Assert.AreEqual("https://mastodon.technology/users/deleteduser#delete", data.id);
            Assert.AreEqual("Delete", data.type);
            Assert.AreEqual("https://mastodon.technology/users/deleteduser", data.actor);
            Assert.AreEqual("https://mastodon.technology/users/deleteduser", data.apObject);
        }

        //[TestMethod]
        //public void NoteDeserializationTest()
        //{
        //    var json =
        //        "{\"@context\":[\"https://www.w3.org/ns/activitystreams\",{\"ostatus\":\"http://ostatus.org#\",\"atomUri\":\"ostatus:atomUri\",\"inReplyToAtomUri\":\"ostatus:inReplyToAtomUri\",\"conversation\":\"ostatus:conversation\",\"sensitive\":\"as:sensitive\",\"toot\":\"http://joinmastodon.org/ns#\",\"votersCount\":\"toot:votersCount\"}],\"id\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182/activity\",\"type\":\"Create\",\"actor\":\"https://mastodon.technology/users/testtest\",\"published\":\"2020-06-29T02:10:04Z\",\"to\":[\"https://mastodon.technology/users/testtest/followers\"],\"cc\":[],\"object\":{\"id\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182\",\"type\":\"Note\",\"summary\":null,\"inReplyTo\":null,\"published\":\"2020-06-29T02:10:04Z\",\"url\":\"https://mastodon.technology/@testtest/104424839893177182\",\"attributedTo\":\"https://mastodon.technology/users/testtest\",\"to\":[\"https://mastodon.technology/users/testtest/followers\"],\"cc\":[],\"sensitive\":false,\"atomUri\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182\",\"inReplyToAtomUri\":null,\"conversation\":\"tag:mastodon.technology,2020-06-29:objectId=34900058:objectType=Conversation\",\"content\":\"<p>test</p>\",\"contentMap\":{\"en\":\"<p>test</p>\"},\"attachment\":[],\"tag\":[],\"replies\":{\"id\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182/replies\",\"type\":\"Collection\",\"first\":{\"type\":\"CollectionPage\",\"next\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182/replies?only_other_accounts=true&page=true\",\"partOf\":\"https://mastodon.technology/users/testtest/statuses/104424839893177182/replies\",\"items\":[]}}}}";

        //    var data = ApDeserializer.ProcessActivity(json) as ActivityAcceptFollow;
        //}
    }
}