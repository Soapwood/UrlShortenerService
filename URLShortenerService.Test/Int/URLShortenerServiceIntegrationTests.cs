using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace URLShortenerService.Test;

/// <summary>
/// Integration Tests for URLShortenerService
/// Service containers should be running i.e. docker-compose -f docker-compose.yml up
/// </summary>
public class UrlShortenerServiceIntegrationTests
{
    private static readonly string _serviceIp = "127.0.0.1";
    private static readonly string _servicePort = "5000";
    private static readonly string _testUrlBase = "https://ea.gr8people.com/jobs/";
    private static string _serviceUrl = $"http://{_serviceIp}:{_servicePort}";
    private static string _shortenApiCall = $"/shorten";

    private static HttpClient? _client;

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient();
    }

    /// <summary>
    /// Add URL with random six digit code to DB
    /// </summary>
    /// <returns></returns>
    [Test]
    [Category("Integration")]
    public async Task ValidateNewIdCreation()
    {
        Random generator = new Random();
        String randomNum = generator.Next(0, 1000000).ToString("D6");

        string shortenTestUrl = _testUrlBase + randomNum;

        var responses = await ValidateNewEntry(shortenTestUrl);
        string putResponseText = responses.Item1;
        string getResponseText = responses.Item2;

        Assert.IsNotNull(putResponseText);

        Console.WriteLine($"Testing {getResponseText} == {shortenTestUrl}");
        Assert.AreEqual(getResponseText, shortenTestUrl);
    }

    /// <summary>
    /// Add URL with random six digit code to DB
    /// </summary>
    /// <returns></returns>
    [Test]
    [Category("Integration")]
    public async Task TestNewIdCreation500()
    {
        for(int i = 0; i < 500; i++)
        {
            Random generator = new Random();
            String randomNum = generator.Next(0, 100000000).ToString("D10");

            string shortenTestUrl = _testUrlBase + randomNum;

            var responses = await ValidateNewEntry(shortenTestUrl);
            string putResponseText = responses.Item1;
            string getResponseText = responses.Item2;

            Assert.IsNotNull(putResponseText);

            Console.WriteLine($"Testing {getResponseText} == {shortenTestUrl}");
            Assert.AreEqual(getResponseText, shortenTestUrl);
        }
    }

    /// <summary>
    /// Create new entry and get back response for validation
    /// </summary>
    /// <param name="shortenTestUrl"></param>
    /// <returns>Responses from APIs</returns>
    public async static Task<Tuple<string, string>> ValidateNewEntry(string shortenTestUrl)
    {
        Console.WriteLine($"Testing: {_serviceUrl}{_shortenApiCall}");

        var httpContent = new StringContent($"{{\"longUrl\": \"{shortenTestUrl}\"}}", Encoding.UTF8, "application/json");
        HttpResponseMessage putResponse = await _client.PutAsync(new Uri(_serviceUrl + _shortenApiCall), httpContent);
        putResponse.EnsureSuccessStatusCode();

        var putResponseText = await putResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Response: {putResponseText}");
        Assert.IsNotNull(putResponseText);

        HttpResponseMessage getResponse = await _client.GetAsync(new Uri(_serviceUrl + $"/{putResponseText}"));
        getResponse.EnsureSuccessStatusCode();
        var getResponseText = await getResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Response: {getResponseText}");

        return Tuple.Create(putResponseText, getResponseText);
    }

    /// <summary>
    /// Add URL with random six digit code to DB
    /// </summary>
    /// <returns></returns>
    [Test]
    [Category("Integration")]
    public async Task ValidateDbCollisionReturns409AndShortUrl()
    {
        Random generator = new Random();
        String randomNum = generator.Next(0, 1000000).ToString("D6");

        string shortenTestUrl = _testUrlBase + randomNum;

        var responses = await ValidateNewEntry(shortenTestUrl);
        string putResponseText = responses.Item1;
        string getResponseText = responses.Item2;

        Assert.IsNotNull(putResponseText);

        Console.WriteLine($"Testing {getResponseText} == {shortenTestUrl}");
        Assert.AreEqual(getResponseText, shortenTestUrl);

        Console.WriteLine($"Testing: {_serviceUrl}{_shortenApiCall}");

        // Add same URL again
        var httpContent = new StringContent($"{{\"longUrl\": \"{shortenTestUrl}\"}}", Encoding.UTF8, "application/json");
        HttpResponseMessage repeatedPutResponse = await _client.PutAsync(new Uri(_serviceUrl + _shortenApiCall), httpContent);

        var repeatedPutResponseText = await repeatedPutResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"Response: {repeatedPutResponseText}");
        Assert.AreEqual(repeatedPutResponse.StatusCode, System.Net.HttpStatusCode.Conflict);
        
        // Verify original generated code is in the 409 response.
        StringAssert.Contains(putResponseText, repeatedPutResponseText);
    }
}