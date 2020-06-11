using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Tweetbook.Contracts;
using Xunit;

namespace Tweetbook.IntegrationTests
{
    // this is for test our Routes and try how does HttpClient work
    public class UnitTest1
    {
        private readonly HttpClient _client;

        public UnitTest1()
        {
            var appFactory = new WebApplicationFactory<Startup>();
            _client = appFactory.CreateClient();
        }

        [Fact]
        public async Task Test1()
        {
            var response = await _client.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", "1"));
        }
    }
}
