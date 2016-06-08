using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicStore.Models;
using System.Net.Http;

namespace MusicStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;

        public StoreController(MusicStoreContext dbContext, IOptions<AppSettings> options)
        {
            DbContext = dbContext;
            _appSettings = options.Value;
            _httpClient = new HttpClient();
        }

        public MusicStoreContext DbContext { get; }

        //
        // GET: /Store/
        public async Task<IActionResult> Index()
        {
            var genres = await DbContext.Genres.ToListAsync();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco
        public async Task<IActionResult> Browse(string genre)
        {
            // Retrieve Genre genre and its Associated associated Albums albums from database
            var genreModel = await DbContext.Genres
                .Include(g => g.Albums)
                .Where(g => g.Name == genre)
                .FirstOrDefaultAsync();

            if (genreModel == null)
            {
                return NotFound();
            }

            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache,
            int id)
        {
            // default to true, change it if it wasn't
            bool wasCached = true;

            var cacheKey = string.Format("album_{0}", id);
            Album album;
            if (!cache.TryGetValue(cacheKey, out album))
            {
                wasCached = false;
                album = await DbContext.Albums
                                .Where(a => a.AlbumId == id)
                                .Include(a => a.Artist)
                                .Include(a => a.Genre)
                                .FirstOrDefaultAsync();

                if (album != null)
                {
                    if (_appSettings.CacheDbResults)
                    {
                        //Remove it from cache if not retrieved in last 10 minutes
                        cache.Set(
                            cacheKey,
                            album,
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                    }
                }
            }

            if (album == null)
            {
                return NotFound();
            }

            var analyticsUrl = $"http://localhost:52119/api/send?id={id}&name={album.Title}&cached={wasCached}&time={DateTimeOffset.Now.ToString()}";


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // It's okay to not await this, since it's just sending data to another service
            // Also, there's no content.  We can embed everything in the URI.
            _httpClient.PutAsync(analyticsUrl, null);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            
            return View(album);
        }
    }
}