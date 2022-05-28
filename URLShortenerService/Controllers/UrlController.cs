using URLShortenerService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using URLShortenerService.Hash;
using System.Web;

/// <summary>
/// InjectedController to allow UrlContext DB to be instanciated via dependency injection
/// </summary>
public class InjectedController : ControllerBase
{
    protected readonly UrlContext db;
    
    public InjectedController(UrlContext context)
    {
        db = context;
    }
}

/// <summary>
/// URLController. Controller for MVC pattern to map URL POCO to database reference.
/// Extends InjectedController to allow DI injection of DB context.
/// </summary>
[ApiController]
[Route("")]
public class UrlController : InjectedController
{
    private readonly ILogger<UrlController> _logger;
    public UrlHashingService Hash;

    public UrlController(UrlContext context, ILogger<UrlController> logger) : base(context) {
        Hash = new UrlHashingService();
        _logger = logger;
    }

    /// <summary>
    /// HTTP PUT Request Handler
    /// PUT urlshortenerservice/shorten
    /// Takes in JSON blob of longUrl
    /// </summary>
    /// <param name="inputUrl"></param>
    /// <returns>HTTP Status Code</returns>
    /// <exception cref="DbUpdateException"></exception>
    [HttpPut("shorten")]
    public async Task<ActionResult<IUrl>> PutUrl([FromBody] Url inputUrl)
    {
        _logger.LogInformation($"[PUT REQUEST] SHORTENING URL: {inputUrl.LongUrl}");
        //Verify input
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Verify if input string is valid URI
        if (!Uri.IsWellFormedUriString(inputUrl.LongUrl, UriKind.Absolute)) return BadRequest("Input string is NOT VALID URL");

        // Check if long URL doesn't already exist
        var presentUrl = from url in db.Urls select url;
        presentUrl = presentUrl.Where(s => s.LongUrl!.Contains(inputUrl.LongUrl));
        var presentUrlsList = await presentUrl.ToListAsync();

        // If URL already present, return known shortened URL
        if (presentUrlsList.Any())
        {
            _logger.LogError($"[PUT REQUEST CONFLICT] URL ALREADY SHORTENED: {presentUrlsList.First().ShortUrl}");
            return Conflict($"Url already shortened: {presentUrlsList.First().ShortUrl}");
        }

        try
        {
            // Save changes to generate MySQL DB ID
            await db.Urls.AddAsync(inputUrl);
            await db.SaveChangesAsync();
        } catch (OperationCanceledException ex)
        {
            _logger.LogError($"[PUT REQUEST FAILURE] Operation Cancelled. Please check MySQL service health. PUT request will fail.");
            _logger.LogError(ex.Message);
        }

        if (inputUrl.Id == null)
        {
            _logger.LogError($"[DB ERROR] ID WAS NOT ASSIGNED TO URL");
            throw new DbUpdateException("ID was not assigned to URL from MySQLDB.");
        }

        // Encode the returned ID to shortened URL "hash"
        var encodedUrl = Hash.Encode((int) inputUrl.Id);
        inputUrl.ShortUrl = encodedUrl;

        await db.SaveChangesAsync();

        _logger.LogInformation($"[PUT REQUEST OK] Created Entry: [{inputUrl.Id}:{inputUrl.LongUrl}:{inputUrl.ShortUrl}]");

        // OK Response
        return Ok($"{inputUrl.ShortUrl}");
    }

    /// <summary>
    /// HTTP GET Request Handler
    /// GET urlshortenerservice/
    /// Reads in shortUrl from URI itself
    /// </summary>
    /// <param name="shortUrl"></param>
    /// <returns>HTTP Status Code</returns>
    [HttpGet("{shortUrl}")]
    public async Task<ActionResult<IUrl>> GetUrl(string shortUrl)
    {
        _logger.LogInformation($"[GET REQUEST] GETTING LONG URL FOR: {shortUrl}");
        int decodedHashId = Hash.Decode(shortUrl);

        Url? requestUrl = null;

        try
        {
            requestUrl = await db.Urls.FindAsync(decodedHashId);
        } catch (OperationCanceledException ex)
        {
            _logger.LogError($"[GET REQUEST FAILURE] Operation Cancelled. Please check MySQL service health. GET request will fail.");
            _logger.LogError(ex.Message);
        }

        if (requestUrl == default(Url))
        {
            _logger.LogError($"[GET REQUEST FAILURE] ID NOT FOUND IN URL DB");
            return NotFound("ID Not found in URL DB");
        }

        _logger.LogInformation($"[GET REQUEST OK] Retrieved Entry: [{requestUrl.Id}:{requestUrl.LongUrl}:{requestUrl.ShortUrl}]");
        return Ok(HttpUtility.UrlDecode(requestUrl.LongUrl));
    }
}
