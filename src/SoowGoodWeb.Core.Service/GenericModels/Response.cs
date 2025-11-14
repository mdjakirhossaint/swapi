namespace SoowGoodWeb.Core.Service.GenericModels
{
    public class Response<T>
    {
        public T? Result { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public bool IsSuccess { get; set; } = false;
        public int StatusCode { get; set; }
    }
}
