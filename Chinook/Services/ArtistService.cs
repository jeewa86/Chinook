using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Reflection.Metadata.BlobBuilder;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private readonly ChinookContext _DbContext;
        public ArtistService(ChinookContext DbContext)
        {
            _DbContext = DbContext;
        }

        public async Task<List<Artist>> GetArtists(string name)
        {
            var users = _DbContext.Users.Include(a => a.UserPlaylists).ToList();

            return _DbContext.Artists.Where(a => a.Name.Contains(name)).ToList();
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            return _DbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

        public async Task<Artist> GetArtist(long artistId)
        {
            return _DbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
        }

        public async Task<List<PlaylistTrack>> GetTracks(long artistId, string userId)
        {
            var tracks = _DbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "Favorites")).Any()
                })
                .ToList();

            return tracks;
        }

        public async Task<ClientModels.Playlist> GetPlayList(long playListId, string userId)
        {
            var playlist = _DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == playListId)
                .Select(p => new ClientModels.Playlist()
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "Favorites")).Any()
                    }).ToList()
                })
                .FirstOrDefault();

            return playlist;
        }

        public async Task<List<ClientModels.Playlist>> GetUserPlayLists(string userId)
        {
            var ups = _DbContext.Playlists.ToList();
            var userPlaylists = from p in _DbContext.Playlists
                                      join up in _DbContext.UserPlaylists on p.PlaylistId equals up.PlaylistId
                                      where up.UserId == userId
                                      select new 
                                      {
                                        p.PlaylistId,
                                        p.Name
                                      };
            List<ClientModels.Playlist> playlists = new List<ClientModels.Playlist>();
            if (userPlaylists.Any())
            {
                foreach (var item in userPlaylists)
                {
                    var playlist = new ClientModels.Playlist();
                    playlist.PlaylistId = item.PlaylistId;
                    playlist.Name = item.Name;
                    playlists.Add(playlist);
                }
            }
            return playlists;
        }

        public void AddTrackToPlaylist(long playListId, string playListName, string userId, Track track)
        {
            if (playListId == -1)
            {
                //insert
                var id = _DbContext.Playlists.OrderByDescending(o => o.PlaylistId).Select(c => c.PlaylistId).FirstOrDefault();
                var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == track.TrackId).ToList();

                var playlist = new Models.Playlist() { PlaylistId = id + 1, Name = playListName, Tracks = selectedtrack };
                _DbContext.Playlists.Add(playlist);
                _DbContext.SaveChanges();
                
                var userPlaylist = new Models.UserPlaylist() { PlaylistId = id + 1, UserId = userId };
                _DbContext.UserPlaylists.Add(userPlaylist);
                _DbContext.SaveChanges();
            }
            else
            {
                //update
                var playlist = _DbContext.Playlists.Where(p => p.PlaylistId == playListId).FirstOrDefault();
                var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == track.TrackId).FirstOrDefault();
                playlist.Tracks.Add(selectedtrack);

                _DbContext.Playlists.Update(playlist);
                _DbContext.SaveChanges();
            }
        }

        public void RemoveTrackFromPlaylist(long playListId, long trackId)
        {
            var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == trackId).FirstOrDefault();
            var playlist = _DbContext.Playlists.Where(p => p.PlaylistId == playListId).FirstOrDefault();
            playlist.Tracks.Remove(selectedtrack);
            _DbContext.SaveChanges();
        }

        public void AddTrackToFavorite(long trackId, string userId)
        {
            long playListId = (from p in _DbContext.Playlists
                                join up in _DbContext.UserPlaylists on p.PlaylistId equals up.PlaylistId
                                where up.UserId == userId && p.Name == "Favorites"
                                select p.PlaylistId).FirstOrDefault();

            if (playListId == 0)
            {
                //insert
                var id = _DbContext.Playlists.OrderByDescending(o => o.PlaylistId).Select(c => c.PlaylistId).FirstOrDefault();
                var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == trackId).ToList();

                var playlist = new Models.Playlist() { PlaylistId = id + 1, Name = "Favorites", Tracks = selectedtrack };
                _DbContext.Playlists.Add(playlist);
                _DbContext.SaveChanges();

                var userPlaylist = new Models.UserPlaylist() { PlaylistId = id + 1, UserId = userId };
                _DbContext.UserPlaylists.Add(userPlaylist);
                _DbContext.SaveChanges();
            }
            else
            {
                //update
                var playlist = _DbContext.Playlists.Where(p => p.PlaylistId == playListId).FirstOrDefault();
                var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == trackId).FirstOrDefault();
                playlist.Tracks.Add(selectedtrack);

                _DbContext.Playlists.Update(playlist);
                _DbContext.SaveChanges();
            }
        }

        public void RemoveTrackToFavorite(long trackId, string userId)
        {

            long playListId = (from p in _DbContext.Playlists
                               join up in _DbContext.UserPlaylists on p.PlaylistId equals up.PlaylistId
                               where up.UserId == userId && p.Name == "Favorites"
                               select p.PlaylistId).FirstOrDefault();

            if (playListId > 0)
            {
                //delete
                var selectedtrack = _DbContext.Tracks.Where(a => a.TrackId == trackId).FirstOrDefault();
                var playlist = _DbContext.Playlists.Where(p => p.PlaylistId == playListId).FirstOrDefault();
                playlist.Tracks.Remove(selectedtrack);
                _DbContext.SaveChanges();
            }
        }
    }
}
