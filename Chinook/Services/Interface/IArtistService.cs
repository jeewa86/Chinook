using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services.Interface
{
    public interface IArtistService
    {
        public Task<List<Artist>> GetArtists(string name);

        public Task<List<Album>> GetAlbumsForArtist(int artistId);

        public Task<Artist> GetArtist(long artistId);

        public Task<List<PlaylistTrack>> GetTracks(long artistId, string userId);

        public Task<ClientModels.Playlist> GetPlayList(long playListId, string userId);

        public Task<List<ClientModels.Playlist>> GetUserPlayLists(string userId);

        public void AddTrackToPlaylist(long playListId, string playListName, string userId, Track track);

        void RemoveTrackFromPlaylist(long playListId, long trackId);

        void AddTrackToFavorite(long trackId, string userId);

        void RemoveTrackToFavorite(long trackId, string userId);
    }
}
