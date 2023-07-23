using Chinook.Models;
using Chinook.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Components
{
    public class PlayListBase : ComponentBase
    {
        [Inject]
        IArtistService ArtistService { get; set; }

        [Parameter] public long PlaylistId { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

        protected ClientModels.Playlist Playlist;
        protected string CurrentUserId;
        protected string InfoMessage;

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetUserId();

            await InvokeAsync(StateHasChanged);

            Playlist = await ArtistService.GetPlayList(PlaylistId, CurrentUserId);
        }

        protected async Task<string> GetUserId()
        {
            try
            {
                var user = (await authenticationState).User;
                var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
                return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        protected async Task<string> FavoriteTrack(long trackId)
        {
            try
            {
                var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                ArtistService.AddTrackToFavorite(trackId, CurrentUserId);
                Playlist = await ArtistService.GetPlayList(PlaylistId, CurrentUserId);
                InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
                return InfoMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }

        protected async Task<string> UnfavoriteTrack(long trackId)
        {
            try
            {
                var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                ArtistService.RemoveTrackToFavorite(trackId, CurrentUserId);
                Playlist = await ArtistService.GetPlayList(PlaylistId, CurrentUserId);
                InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
                return InfoMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        protected async Task<string> RemoveTrack(long trackId)
        {
            try
            {
                ArtistService.RemoveTrackFromPlaylist(PlaylistId, trackId);
                Playlist = await ArtistService.GetPlayList(PlaylistId, CurrentUserId);
                CloseInfoMessage();
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        protected void CloseInfoMessage()
        {
            InfoMessage = "";
        }
    }
}
