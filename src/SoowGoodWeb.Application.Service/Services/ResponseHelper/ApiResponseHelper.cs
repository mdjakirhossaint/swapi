
using Microsoft.AspNetCore.Http;
using SoowGoodWeb.Core.GenericModels;
using SoowGoodWeb.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Application.Service.Services.ResponseHelper
{
    public static class ApiResponseHelper
    {
        public static void SetSuccessResponse<T>(ApiResponse<T> apiResponse, T? result = default, string? message = null, string? status = null, int? statusCode = null)
        {
            if (apiResponse == null)
            {
                // If apiResponse is null, throw an exception or handle it accordingly.
                throw new ArgumentNullException(nameof(apiResponse), "ApiResponse cannot be null.");
            }
            // If no message is provided, use the default common success message
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(apiResponse.message))
            {
                message = "Success : The completion of actions like retrieval, insertion, updating, or deletion of data";
            }
            apiResponse.results = result;
            apiResponse.status = status ?? StatusResponseMessage.success;
            apiResponse.message = message ?? apiResponse.message;
            apiResponse.status_code = statusCode ?? StatusCodes.Status200OK;
            apiResponse.is_success = true;
        }
        public static void SetSuccessResponse<T>(ApiResponseList<T> apiResponse, List<T>? result = default, string? message = null, string? status = null, int? statusCode = null)
        {
            if (apiResponse == null)
            {
                // If apiResponse is null, throw an exception or handle it accordingly.
                throw new ArgumentNullException(nameof(apiResponse), "ApiResponse cannot be null.");
            }
            // If no message is provided, use the default common success message
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(apiResponse.message))
            {
                message = "Success : The completion of actions like retrieval, insertion, updating, or deletion of data";
            }
            apiResponse.results = result;
            apiResponse.status = status ?? StatusResponseMessage.success;
            apiResponse.message = message ?? apiResponse.message;
            apiResponse.status_code = statusCode ?? StatusCodes.Status200OK;
            apiResponse.is_success = true;
        }

        public static void SetFailedResponse<T>(ApiResponse<T> apiResponse, T? result = default, string? message = null, string? status = null, int? statusCode = null)
        {
            if (apiResponse == null)
            {
                // If apiResponse is null, throw an exception or handle it accordingly.
                throw new ArgumentNullException(nameof(apiResponse), "ApiResponse cannot be null.");
            }

            // If no message is provided, use a default failure message.
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(apiResponse.message))
            {
                message = "Failed : The completion of actions like retrieval, insertion, updating, or deletion of data";
            }
            apiResponse.results = result;
            apiResponse.status = status ?? StatusResponseMessage.failed;
            apiResponse.message = message ?? apiResponse.message;
            apiResponse.status_code = statusCode ?? StatusCodes.Status400BadRequest;
            apiResponse.is_success = false;
        }
        public static void SetFailedResponse<T>(ApiResponseList<T> apiResponse, List<T>? result = default, string? message = null, string? status = null, int? statusCode = null)
        {
            if (apiResponse == null)
            {
                // If apiResponse is null, throw an exception or handle it accordingly.
                throw new ArgumentNullException(nameof(apiResponse), "ApiResponse cannot be null.");
            }

            // If no message is provided, use a default failure message.
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(apiResponse.message))
            {
                message = "Failed : The completion of actions like retrieval, insertion, updating, or deletion of data";
            }
            apiResponse.results = result;
            apiResponse.status = status ?? StatusResponseMessage.failed;
            apiResponse.message = message ?? apiResponse.message;
            apiResponse.status_code = statusCode ?? StatusCodes.Status400BadRequest;
            apiResponse.is_success = false;
        }

    }
}
