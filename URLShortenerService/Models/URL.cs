using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace URLShortenerService.Models;

public class Url : IUrl
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int? Id { get; set; }

    public string? LongUrl { get; set; }

    public string? ShortUrl { get; set; }

    public Url() { }
}

public class UrlContext: DbContext
{
    public DbSet<Url> Urls { get; set; }
    public UrlContext(DbContextOptions options) : base(options) { }
}

