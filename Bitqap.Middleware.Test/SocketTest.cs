using Bitqap.Middleware.Entity.ApiEntity;
using NUnit.Framework;

namespace Bitqap.Middleware.Test
{
    public class SocketTest
    {
        private string apiSettings1 = "{\"SocketHostUrl\": \"ws://.com:8003\"}";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConnectionTest()
        {
            var settings = System.Text.Json.JsonSerializer.Deserialize<ApiSettings>(apiSettings1);

            Assert.Pass();
        }
    }
}