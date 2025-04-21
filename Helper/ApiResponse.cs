using System.Net;

namespace Helper
{
    public class ApiResponse
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Exception Exception { get; set; }
        public string Url { get; set; }
        public bool IsSuccessStatusCode { get; set; }
    }
}
