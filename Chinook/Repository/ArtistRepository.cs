using Chinook.Models;
using Chinook.Repository.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Repository
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly ChinookContext _DbContext;
        public ArtistRepository(ChinookContext DbContext) 
        { 
            _DbContext = DbContext;
        }
        //private async Task<string> GetUserId()
        //{
        //    var user = (await authenticationState).User;
        //    var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
        //    return userId;
        //}

        public async Task<List<Artist>> GetArtists()
        {
            var users = _DbContext.Users.Include(a => a.UserPlaylists).ToList();

            return _DbContext.Artists.ToList();
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            return _DbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
