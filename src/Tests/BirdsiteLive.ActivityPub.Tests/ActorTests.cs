using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Tests
{
    [TestClass]
    public class ActorTests
    {
        [TestMethod]
        public void Deserialize()
        {
            var json =
                "{\"@context\":[\"https://www.w3.org/ns/activitystreams\",\"https://w3id.org/security/v1\",{\"manuallyApprovesFollowers\":\"as:manuallyApprovesFollowers\",\"toot\":\"http://joinmastodon.org/ns#\",\"featured\":{\"@id\":\"toot:featured\",\"@type\":\"@id\"},\"alsoKnownAs\":{\"@id\":\"as:alsoKnownAs\",\"@type\":\"@id\"},\"movedTo\":{\"@id\":\"as:movedTo\",\"@type\":\"@id\"},\"schema\":\"http://schema.org#\",\"PropertyValue\":\"schema:PropertyValue\",\"value\":\"schema:value\",\"IdentityProof\":\"toot:IdentityProof\",\"discoverable\":\"toot:discoverable\"}],\"id\":\"https://mastodon.technology/users/testtest\",\"type\":\"Person\",\"following\":\"https://mastodon.technology/users/testtest/following\",\"followers\":\"https://mastodon.technology/users/testtest/followers\",\"inbox\":\"https://mastodon.technology/users/testtest/inbox\",\"outbox\":\"https://mastodon.technology/users/testtest/outbox\",\"featured\":\"https://mastodon.technology/users/testtest/collections/featured\",\"preferredUsername\":\"testtest\",\"name\":\"TESTEST\",\"summary\":\"\u003cp\u003etest \u003cbr /\u003edsqdq65d4sq56d456q4d8zd4q685d45qd4sqd2q1d5zq56d465qsd4q65sd21qsd23q1s5d64qsd8q465d4s5q1d6qsd35qs4dq6sd84q\u003c/p\u003e\",\"url\":\"https://mastodon.technology/@testtest\",\"manuallyApprovesFollowers\":false,\"discoverable\":false,\"publicKey\":{\"id\":\"https://mastodon.technology/users/testtest#main-key\",\"owner\":\"https://mastodon.technology/users/testtest\",\"publicKeyPem\":\"-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAm7BlbWI/UD/YJj288h/5\nFB0gXZj0BjYVaK28uzTvb4w6eMu4qpbE9NI0bFqrloXzL3z6PaOCL4Myz9uJYolE\nZ9uNVi2OeZmHigNEOT3hkJWzddtrhkg8MLXKPdOETjhVWV3n+na7QWDDIXP7Fuvi\n+osA5LOoqtD1rYs87xUcWQPLCtVHs928FXsCdLO11ofXiNrancSzY17nkuufjWO+\ndLtvz1kx4Mt2V4Fu+DHskQAzPKU2tzGBrtlVQrk+1R63psIuZYDB6e4i7L6/d1Xl\nIQGmBeJfyxiuNIlbfZIbJ3xPYBQaVAnRKtyGVEFMWwZCqMySwc2LBX+rxI20zJ0R\n7wIDAQAB\n-----END PUBLIC KEY-----\n\"},\"tag\":[],\"attachment\":[],\"endpoints\":{\"sharedInbox\":\"https://mastodon.technology/inbox\"}}";


            var actor = JsonConvert.DeserializeObject<Actor>(json);

            
        }

        [TestMethod]
        public void Serialize()
        {
            var obj = new Actor
            {
                type = "Person",
                id = "id"
            };

            var json = JsonConvert.SerializeObject(obj);


        }
    }
}