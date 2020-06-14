using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetbook.Contracts.V1.Requests
{
    public class CreatePostRequest
    {
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
