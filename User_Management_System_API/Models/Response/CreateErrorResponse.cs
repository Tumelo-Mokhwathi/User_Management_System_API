using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace User_Management_System_API.Models.Response
{
    public class CreateErrorResponse
    {
        public HttpStatusCode code { get; set; }
        public string message { get; set; }
        public string source { get; set; }

        public CreateErrorResponse(HttpStatusCode Code, string Message, string Source)
        {
            code = Code;
            message = Message;
            source = Source;
        }
    }
}
