using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TheWorld.Models;

namespace TheWorld.Services
{
    public class GeoCoordsGoogleService : IGeoCoordsService
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<GeoCoordsBingService> _logger;

        public GeoCoordsGoogleService(ILogger<GeoCoordsBingService> logger, IConfigurationRoot config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<GeoCoordsResult> GetCoordsAsync(string name)
        {
            GeoCoordsResult result = new GeoCoordsResult()
            {
                Success = false,
                Message = "Failed to get coordinates"
            };

            name = name.Replace(" ", "+");
            string encodedName = WebUtility.UrlEncode(name);

            string url = $"http://maps.googleapis.com/maps/api/geocode/json?address={encodedName}&sensor=true_or_false";

            HttpClient client = new HttpClient();

            string json = await client.GetStringAsync(url);

            JObject results = JObject.Parse(json);

            JToken geometry = results["results"][0]["geometry"];
            if (!geometry.HasValues)
            {
                result.Message = $"Could not find '{name}' as a location";
            }
            else
            {
                JToken coords = geometry["location"];
                result.Latitude = (double)coords["lat"];
                result.Longitude = (double)coords["lng"];
                result.Success = true;
                result.Message = "Success";
            }

            return result;
        }
    }
}
