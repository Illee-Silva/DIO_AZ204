using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class fnTempo
{
    private static readonly HttpClient httpClient = new HttpClient();

    [FunctionName("GetWeather")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string city = req.Query["city"];
        if (string.IsNullOrEmpty(city))
        {
            return new BadRequestObjectResult("Please pass a city on the query string");
        }

        string apiKey = Environment.GetEnvironmentVariable("OpenWeatherMapApiKey");
        string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return new BadRequestObjectResult("Error fetching weather data");
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        dynamic weatherData = JsonConvert.DeserializeObject(responseBody);

        double tempCelsius = weatherData.main.temp;
        double tempFahrenheit = (tempCelsius * 9 / 5) + 32;
        double tempKelvin = tempCelsius + 273.15;

        var result = new
        {
            City = city,
            TemperatureCelsius = tempCelsius,
            TemperatureFahrenheit = tempFahrenheit,
            TemperatureKelvin = tempKelvin
        };

        return new OkObjectResult(result);
    }
}