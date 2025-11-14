using Nancy.Responses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Domain.Service.Repositories.RestApiCallService
{
    public interface IBaseRestClientApiService
    {
        Task<RestResponse> MakeApiCall<T>(string baseURL, string endPoint, Method method, T? model = null, string? authToken = null, int? maxRetries = null, int? delayMilliseconds = null) where T : class;
    }
}
