using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Tron.Hubs;
using Tron.ViewModels.Tron;
using TronEngine.Events;
using TronEngine.Helpers;
using TronEngine.Models;

namespace Tron.Services
{
    public class TronService : IHostedService
    {
        #region FIELDS

        private readonly IHubContext<TronHub> _hubContext;

        #endregion FIELDS

        #region CONSTRUCTORS

        public TronService(IHubContext<TronHub> hubContext)
        {
            this._hubContext = hubContext;
        }

        #endregion CONSTRUCTORS

        #region IHOSTEDSERVICE IMPLEMENTATIONS

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TronHelper.GameUpdated += TronHelper_GameUpdated;
            TronHelper.InformationUpdated += TronHelper_InformationUpdated;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TronHelper.GameUpdated -= TronHelper_GameUpdated;
            TronHelper.InformationUpdated -= TronHelper_InformationUpdated;

            return Task.CompletedTask;
        }

        #endregion IHOSTEDSERVICE IMPLEMENTATIONS

        #region EVENT HANDLE METHODS

        private async void TronHelper_GameUpdated(object sender, GameUpdatedEventArgs e)
        {
            if (sender is Game game)
            {
                this.UpdateGroup(game, e);
                if (e.IsCreated)
                {
                    GameViewModel viewModel = new GameViewModel(game);
                    await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("match_started", viewModel);
                }
                else
                {
                    GameUpdatedViewModel viewModel = new GameUpdatedViewModel(game, e);
                    await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("game_updated", viewModel);
                }
            }
        }

        private async void TronHelper_InformationUpdated(object sender, EventArgs e)
        {
            if (sender is Game game)
            {
                DataGameViewModel viewModel = new DataGameViewModel(game);
                await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("game_information", viewModel);
            }
        }

        #endregion EVENT HANDLE METHODS

        #region PRIVATE METHODS

        private void UpdateGroup(Game game, GameUpdatedEventArgs e)
        {
            if (game != null)
            {
                if (e != null && !String.IsNullOrEmpty(e.PlayerLeftConnectionId))
                {
                    _hubContext.Groups.RemoveFromGroupAsync(e.PlayerLeftConnectionId, game.Id.ToString());
                }

                foreach (Player player in game.Players)
                {
                    if (player != null && !String.IsNullOrEmpty(player.ConnnectionId))
                    {
                        _hubContext.Groups.AddToGroupAsync(player.ConnnectionId, game.Id.ToString());
                    }
                }
            }
        }

        #endregion PRIVATE METHODS
    }
}
