using Chinook.ClientModels;
using Chinook.Migrations;
using Chinook.Models;
using Chinook.Services.Interface;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using System.Security.Claims;

namespace Chinook.Components
{
    public class ArtistBase : ComponentBase
    {
        [Inject]
        IArtistService ArtistService { get; set; }
        [Parameter] public long ArtistId { get; set; }
        [CascadingParameter] protected Task<AuthenticationState> authenticationState { get; set; }
        protected Modal PlaylistDialog { get; set; }
        protected string PlaylistName { get; set; } = "";

        protected Artist Artist;
        protected List<PlaylistTrack> Tracks;
        protected DbContext DbContext;
        protected PlaylistTrack SelectedTrack;
        protected List<ClientModels.Playlist> UserPlayLists;
        protected string InfoMessage;
        protected string CurrentUserId;
        protected int SelectedPlayListId;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();

            Artist = await ArtistService.GetArtist(ArtistId);
            UserPlayLists = await ArtistService.GetUserPlayLists(CurrentUserId);
            Tracks = await ArtistService.GetTracks(ArtistId, CurrentUserId);
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

        protected void FavoriteTrack(long trackId)
        {
            try
            {
                var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
                ArtistService.AddTrackToFavorite(trackId, CurrentUserId);
                InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected void UnfavoriteTrack(long trackId)
        {
            try 
            { 
                var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
                ArtistService.RemoveTrackToFavorite(trackId, CurrentUserId);
                InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected void OpenPlaylistDialog(long trackId)
        {
            CloseInfoMessage();
            SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            PlaylistDialog.Open();
        }

        protected void AddTrackToPlaylist()
        {   
            try
            {
                var track = new Track { TrackId = SelectedTrack.TrackId };
                ArtistService.AddTrackToPlaylist(SelectedPlayListId, PlaylistName, CurrentUserId, track);
                CloseInfoMessage();
                InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {PlaylistName}.";
                PlaylistDialog.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected void CloseInfoMessage()
        {
            InfoMessage = "";
        }

        protected void PlayListChange(ChangeEventArgs args)
        {
            SelectedPlayListId = Convert.ToInt32(args?.Value?.ToString());
        }
    }
}
