using SoowGoodWeb.Core.Service.GenericModels;

namespace SoowGoodWeb.Core.GenericModels
{
    /// <summary>
    /// When signle object return at time use.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T> : ApiBaseResponse
    {
        public T? results { get; set; }
    }
    /// <summary>
    /// When List object return at time use.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponseList<T> : ApiBaseResponse
    {
        public List<T>? results { get; set; }
    }
}
