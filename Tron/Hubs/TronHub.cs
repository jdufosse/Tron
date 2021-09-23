using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Tron.ViewModels.Tron;
using TronEngine.Enums;
using TronEngine.Helpers;
using TronEngine.Models;

namespace Tron.Hubs
{
    public class TronHub : Hub
    {
        #region OVERRIDE METHODS
        public override Task OnConnectedAsync()
        {
            TronHelper.CreatePlayer(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            TronHelper.PlayerDisconnected(Context.ConnectionId, exception);
            return base.OnDisconnectedAsync(exception);
        }

        #endregion OVERRIDE METHODS

        #region PUBLIC METHODS

        public async Task GetConnectionInformation()
        {
            await Clients.Caller.SendAsync("connected", Context.ConnectionId);
        }

        public async Task DoMatchMaking(string userName)
        {
            Game game = TronHelper.JoinMatchMaking(Context.ConnectionId, userName);

            if (game != null)
            {
                GameViewModel viewModel = new GameViewModel(game);
                await this.CreateOrUpdateGroup(game);
                await Clients.Group(game.Id.ToString()).SendAsync("match_making_found", viewModel);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task CreatePrivateRoom(string userName)
        {
            Game game = TronHelper.CreatePrivateRoom(Context.ConnectionId, userName);

            if (game != null)
            {
                GameViewModel viewModel = new GameViewModel(game);
                await this.CreateOrUpdateGroup(game);
                await Clients.Group(game.Id.ToString()).SendAsync("private_room_created", viewModel);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task JoinPrivateRoom(string userName, string roomName)
        {
            Game game = TronHelper.JoinPrivateRoom(Context.ConnectionId, userName, roomName);

            if (game != null)
            {
                GameViewModel viewModel = new GameViewModel(game);
                await this.CreateOrUpdateGroup(game);
                await Clients.Group(game.Id.ToString()).SendAsync("private_room_completed", viewModel);
            }
            else
            {
                await Task.CompletedTask;
            }
        }
        public async Task PlayerReady(Guid matchId, bool isReady)
        {
            Game game = TronHelper.SetPlayerReady(Context.ConnectionId, matchId, isReady);

            if (game != null)
            {
                GameViewModel viewModel = new GameViewModel(game);
                await Clients.Group(game.Id.ToString()).SendAsync("match_started", viewModel);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task MapInformation(Guid matchId, int width, int height)
        {
            Game game = TronHelper.SetMapInformation(Context.ConnectionId, matchId, width, height);

            if (game != null)
            {
                MapViewModel viewModel = new MapViewModel(game);
                await Clients.Group(game.Id.ToString()).SendAsync("map_information_received", viewModel);
                Console.WriteLine("MapInformation response : " + DateTime.Now.ToString("o"));
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task BikeDirection(Guid matchId, Direction direction)
        {
            Game game = TronHelper.SetPlayerDirection(Context.ConnectionId, matchId, direction);

            if (game != null)
            {
                MapViewModel viewModel = new MapViewModel(game);
                await Clients.Caller.SendAsync("bike_direction_received", viewModel);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task LeaveGame(Guid matchId)
        {
            TronHelper.PlayerLeft(Context.ConnectionId, matchId);
            await Task.CompletedTask;
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private async Task CreateOrUpdateGroup(Game game)
        {
            if (game != null)
            {
                foreach (Player player in game.Players)
                {
                    if (player == null) continue;
                    await Groups.AddToGroupAsync(player.ConnnectionId, game.Id.ToString());
                }
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        #endregion PRIVATE METHODS
    }
}
