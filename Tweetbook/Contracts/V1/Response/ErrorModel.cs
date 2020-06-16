using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetbook.Contracts.V1.Response
{
    public class ErrorModel
    {
        public string FieldName { get; set; }
        public String Message { get; set; }
    }
}
