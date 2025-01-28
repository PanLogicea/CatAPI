using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CatAPI.Models;
using CatAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;


namespace CatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatController : ControllerBase
    {
        private readonly CatContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatController> _logger;
        private readonly string _apiURI = string.Empty;
        public CatController(CatContext context, ILogger<CatController> logger, IConfiguration configuration)
        {
            _context = context;
            _httpClient = new HttpClient();
            var apiKey = configuration["CatApi:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _logger = logger;
            _apiURI = configuration["CatApi:ApiUrl"] ?? throw new ArgumentNullException(nameof(configuration), "CatApi:ApiUrl configuration is missing.");
          
        }

        [HttpPost("FetchCats")]
        public async Task<IActionResult> FetchCats()
        {
           
            try
            {
                var response = await _httpClient.GetStringAsync(_apiURI);
                var jsonResponse = JArray.Parse(response);
                foreach (var catJson in jsonResponse)
                {
                    var cat = await CreateCatAsync(catJson);
                    if (!_context.Cats.Any(c => c.CatId == cat.CatId))
                    {
                        _context.Cats.Add(cat);
                        await AddTagsToCatAsync(catJson, cat);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok("Cats fetched and saved successfully.");

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error fetching data from the external API.");
                return StatusCode(503, "Error fetching data from the external API.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        private async Task<Cat> CreateCatAsync(JToken catJson)
        {
            var newCat = new Cat
            {
                CatId = (string?)catJson["id"] ?? string.Empty,
                Width = (int?)catJson["width"] ?? 0,
                Height = (int?)catJson["height"] ?? 0,
                Image = await _httpClient.GetByteArrayAsync((string?)catJson["url"] ?? string.Empty),
                Created = DateTime.UtcNow
            };

            return newCat;
        }

        private async Task AddTagsToCatAsync(JToken catJson, Cat cat)
        {
            var breeds = catJson["breeds"];
            if (breeds != null)
            {
                foreach (var breed in breeds)
                {
                    var breedName = (string?)breed["name"];
                    var temperaments = ((string?)breed["temperament"])?.Split(',') ?? Array.Empty<string>();
                    // Uncomment this code fragment, if you need the breed name to be added as a tag.
                    //if (!string.IsNullOrEmpty(breedName))
                    //{
                    //    var breedTag = await GetOrCreateTagAsync(breedName.Trim());
                    //    _context.CatTags.Add(new CatTag { Cat = cat, Tag = breedTag });
                    //}

                    foreach (var temperament in temperaments)
                    {
                        if (!string.IsNullOrEmpty(temperament))
                        {
                            var temperamentTag = await GetOrCreateTagAsync(temperament.Trim());
                            _context.CatTags.Add(new CatTag { Cat = cat, Tag = temperamentTag });
                        }
                    }
                }
            }
        }

        private async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            var tag = _context.Tags.FirstOrDefault(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }
            return tag;
        }

        [HttpGet("GetCatBy/{id}")]
        public async Task<IActionResult> GetCat(int id)
        {
            try
            {
                var cat = await _context.Cats
                    .Include(c => c.CatTags)
                    .ThenInclude(ct => ct.Tag)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cat == null)
                {
                    return NotFound("Cat not found.");
                }

                return Ok(cat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the cat.");
                return StatusCode(500, "An error occurred while fetching the cat.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? tag = null)
        {
            try
            {
                var query = _context.Cats
                .Include(c => c.CatTags)
                .ThenInclude(ct => ct.Tag)
                .AsQueryable();

                if (!string.IsNullOrEmpty(tag))
                {
                    query = query.Where(c => c.CatTags.Any(ct => ct.Tag.Name == tag));
                }

                query = query.OrderBy(c => c.Id);

                var cats = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var catDtos = cats.Select(c => new CatDto
                {
                    Id = c.Id,
                    CatId = c.CatId,
                    Width = c.Width,
                    Height = c.Height,
                    Image = c.Image,
                    Created = c.Created,
                    Tags = c.CatTags.Select(ct => ct.Tag.Name ?? string.Empty).ToList()
                }).ToList();

                return Ok(catDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the cat.");
                return StatusCode(500, "An error occurred while fetching the cat.");
            }

        }
       
    }
}
