using System.Text.Json.Serialization;

namespace ASP.NET_Hands_on.Model
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;//*
        public T? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(T? data, int statusCode = 200, string message = "Request successful")
        {
            Success = statusCode >= 200 && statusCode < 300;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            ObjectType = data == null ? "null" : data.GetType().Name;
        }
    }
}
