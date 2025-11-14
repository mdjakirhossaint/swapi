using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SoowGoodWeb.Core.Service.GenericModels
{
    public static class JsonHelper
    {
        public static List<T> DeserializeJsonToList<T>(RestResponse? response)
        {
            try
            {
                if (response == null || string.IsNullOrWhiteSpace(response.Content))
                    throw new ArgumentNullException(nameof(response), "Response content cannot be null or empty.");

                dynamic responseConvertToJson = JsonConvert.DeserializeObject(response.Content);
                dynamic tokenDeserialized = JObject.Parse(responseConvertToJson.ToString());

                var resultsToken = tokenDeserialized["results"];

                if (resultsToken is JArray)
                {
                    return JsonConvert.DeserializeObject<List<T>>(resultsToken.ToString());
                }
                else if (resultsToken is JObject)
                {
                    var singleItem = JsonConvert.DeserializeObject<T>(resultsToken.ToString());
                    return new List<T> { singleItem };
                }

                throw new InvalidOperationException("'results' property is neither a JSON array nor a JSON object.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON into a list.", ex);
            }
        }
        public static T DeserializeJsonToSignle<T>(RestResponse response)
        {
            try
            {
                var responseObject = JObject.Parse(response.Content);
                var resultsToken = responseObject["results"];

                if (resultsToken == null)
                {
                    throw new InvalidOperationException("'results' key is missing in the JSON response.");
                }
                return resultsToken.ToObject<T>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON.", ex);
            }
        }

    }
}
