using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Repository.Interface
{
    public interface IArtistRepository
    {
        public Task<List<Artist>> GetArtists();

        public Task<List<Album>> GetAlbumsForArtist(int artistId);
    }
}
