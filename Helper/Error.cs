using System.Net;
using System.Text.Json.Serialization;

namespace Helper
{
    public record class Error(string Code, string Discription, [property: JsonIgnore] HttpStatusCode StatusCode)
    {
        public static readonly Error None = new("00", "Success", HttpStatusCode.OK);
        public static readonly Error GlobalError = new("-1", "InternalServerError Please try again", HttpStatusCode.InternalServerError);
    }
}
