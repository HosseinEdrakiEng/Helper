using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Helper
{
    public static class Extention
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = new();

        public static string GetClientId(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
        }
        public static string GeUsertLevel(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "level")?.Value;
        }
        public static string GetUserScore(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "score")?.Value;
        }
        public static string GetUserPhonenumber(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value;
        }
        public static string GetGroupId(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "groupId")?.Value;
        }
        public static string GetUserNationalCode(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(c => c.Type == "nationalCode")?.Value;
        }
        public static string GetUserId(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(r => r.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public static List<string> GetUserRoles(this HttpContext httpContext)
        {
            return [.. httpContext.User.Claims?.Where(r => r.Type == ClaimTypes.Role)?.Select(r => r.Value)];
        }

        public static string SerializeAsJson(this object model, JsonSerializerOptions option = null)
        {
            return JsonSerializer.Serialize(model, options: option ?? new JsonSerializerOptions());
        }

        public static T DeserializeObject<T>(this string data, JsonSerializerOptions option = null)
        {
            return JsonSerializer.Deserialize<T>(data, options: option ?? new JsonSerializerOptions());
        }

        public static async Task<ApiResponse> ApiCall<T>(this IHttpClientFactory httpClientFactory
            , string name
            , T model
            , HttpMethod httpMethod
            , string url
            , Dictionary<string, string> headers
            , CancellationToken cancellationToken)
        {
            var result = new ApiResponse();
            try
            {
                var requestContent = model.SerializeAsJson();
                result.Request = requestContent;
                result.Url = url;

                var httpClient = httpClientFactory.CreateClient(name);
                var requestMessage = new HttpRequestMessage(httpMethod, url);

                HttpContent content = null;
                string contentTypeValue = string.Empty;
                if (headers.TryGetValue("Content-Type", out contentTypeValue) && contentTypeValue == "application/json")
                {
                    content = new StringContent(requestContent, Encoding.UTF8, "application/json");
                }
                else if (headers.TryGetValue("Content-Type", out contentTypeValue) && contentTypeValue == "application/x-www-form-urlencoded")
                {
                    if (model is Dictionary<string, string> t)
                    {
                        var modelData = t.ToList();
                        content = new FormUrlEncodedContent(modelData);
                    }
                }
                else
                    throw new NotSupportedException();

                requestMessage.Content = content;

                foreach (var header in headers)
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

                var httpResponse = await httpClient.SendAsync(requestMessage, cancellationToken);
                result.StatusCode = httpResponse.StatusCode;
                result.Response = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                result.IsSuccessStatusCode = httpResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        public static string GenerateRandomCode(int length = 6)
        {
            Random random = new Random();
            int min = (int)Math.Pow(10, length - 1);
            int max = (int)Math.Pow(10, length) - 1;

            int randomNumber = random.Next(min, max + 1);

            return randomNumber.ToString();
        }

        public static async Task<List<dynamic>> SelectFieldsAsync<T>(this IQueryable<T> source, params string[] fields)
        {
            if (fields is null || fields.Length == 0)
                throw new ArgumentException("You must specify at least one field");

            var selector = "new (" + string.Join(", ", fields) + ")";
            var query = source.Select(selector);
            return await query.ToDynamicListAsync();

            /*Sample
             * 
             * var products = await _context.Products
                    .Where(p => p.Price > 100)
                    .SelectFieldsAsync("Name", "Price");

                foreach (var p in products)
                {
                    Console.WriteLine($"{p.Name} - {p.Price}");
                }

             */
        }

        public static async Task<List<T>> SelectDynamicAsTypeAsync<T>(this IQueryable<T> source, params string[] fields) where T : new()
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException("Fields must be specified", nameof(fields));

            var selector = "new (" + string.Join(", ", fields) + ")";
            var projected = await source.Select(selector).ToDynamicListAsync();
            var type = typeof(T);
            var properties = PropertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanWrite)
                 .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase)
            );

            var result = new List<T>();

            foreach (var item in projected)
            {
                var newObj = new T();

                foreach (var field in fields)
                {
                    if (properties.TryGetValue(field, out var targetProp))
                    {
                        var sourceProp = item.GetType().GetProperty(field);
                        if (sourceProp != null)
                        {
                            var value = sourceProp.GetValue(item);
                            targetProp.SetValue(newObj, value);
                        }
                    }
                }

                result.Add(newObj);
            }

            return result;
        }
    }
}
