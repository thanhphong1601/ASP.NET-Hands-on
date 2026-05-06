using System.Text.Json.Serialization;

namespace ASP.NET_Hands_on.Domain.Model
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public ApiResponse(int statusCode = 200, string message = "Request successful")
        {
            Success = statusCode >= 200 && statusCode < 300;
            StatusCode = statusCode;
            Message = message;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public ApiResponse(T? data, int statusCode = 200, string message = "Request successful") : base(statusCode, message)
        {
            Data = data;
        }
    }
}
