using RestSharp;
using SoowGoodWeb.Domain.Service.Repositories.RestApiCallService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Insfracture.Service.ImplementRepositories.RestApiCallService
{
    public class BaseRestClientApiService : IBaseRestClientApiService
    {
        public BaseRestClientApiService()
        {

        }

        public async Task<RestResponse> MakeApiCall<T>(
    string baseURL,
    string endPoint,
    Method method,
    T? model = null,
    string? authToken = null,
    int? maxRetries = 3,
    int? delayMilliseconds = 1000
) where T : class
        {
            var url = $"{baseURL}{endPoint}";

            var options = new RestClientOptions(url)
            {
                Timeout = TimeSpan.FromSeconds(30),
                ThrowOnAnyError = true,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };

            var clientRest = new RestClient(options);
            var request = new RestRequest { Method = method };

            if (!string.IsNullOrEmpty(authToken))
            {
                request.AddHeader("Authorization", $"bearer {authToken}");
            }

            if (model != null)
            {
                request.AddJsonBody(model);
            }

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"Making API call to: {url}, Attempt: {attempt}");

                    var responseJson = await clientRest.ExecuteAsync(request);

                    if (responseJson != null && responseJson.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return responseJson;
                    }

                    Console.WriteLine($"API call failed with status code {responseJson?.StatusCode}. Attempt: {attempt}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception on attempt {attempt}: {ex.Message}");

                    if (attempt == maxRetries)
                    {
                        throw; // Re-throw the exception if it's the last attempt
                    }
                }

                await Task.Delay((int)delayMilliseconds); // Wait before retrying
            }

            throw new Exception($"API call failed after {maxRetries} attempts.");
        }

    }
}
