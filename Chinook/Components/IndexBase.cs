using Chinook.ClientModels;
using Chinook.Migrations;
using Chinook.Models;
using Chinook.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NuGet.DependencyResolver;
using System.Security.Claims;

namespace Chinook.Components
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        IArtistService ArtistService { get; set; }

        [Parameter]
        public string? SearchText { get; set; }

        [CascadingParameter] protected Task<AuthenticationState> authenticationState { get; set; }

        protected List<Artist> Artists;

        protected List<ClientModels.Playlist> UserPlayLists;
        protected string CurrentUserId;
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
            UserPlayLists = await ArtistService.GetUserPlayLists(CurrentUserId);
            Artists = await GetArtists(string.Empty);
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
        public async Task<List<Artist>> GetArtists(string name)
        {
            try
            {
                return await ArtistService.GetArtists(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            try
            {
                return await ArtistService.GetAlbumsForArtist(artistId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        protected async Task SearchArtists(ChangeEventArgs e)
        {
            try
            {
                SearchText = e?.Value?.ToString();

                Artists = await GetArtists(SearchText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
