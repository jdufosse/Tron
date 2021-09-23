using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TronEngine.Enums;
using TronEngine.Events;
using TronEngine.Models;

namespace TronEngine.Helpers
{
    public static class TronHelper
    {
        #region FIELDS

        private static ConcurrentDictionary<Guid, Game> _games = new ConcurrentDictionary<Guid, Game>();

        private static ConcurrentDictionary<Guid, Game> _gameHistory = new ConcurrentDictionary<Guid, Game>();

        private static ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();

        private static ConcurrentQueue<Game> _waitingGames = new ConcurrentQueue<Game>();

        private static ConcurrentQueue<Player> _waitingPlayers = new ConcurrentQueue<Player>();

        #endregion FIELDS

        #region PUBLIC METHODS

        public static bool CreatePlayer(string connectionId)
        {
            if (!string.IsNullOrEmpty(connectionId) && !_players.ContainsKey(connectionId))
            {
                return _players.TryAdd(connectionId, new Player(connectionId));
            }
            return false;
        }

        public static Game CreatePrivateRoom(string connectionId, string userName)
        {
            Player player = GetPlayer(connectionId);
            if (player != null)
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    player.UserName = userName;
                }
                player.Status = Enums.PlayerStatus.IN_ROOM;
                return CreateGame(new Game(player, null, Enums.GameType.PRIVATE_ROOM));
            }
            return null;
        }

        public static Game JoinMatchMaking(string connectionId, string userName)
        {
            Game game = null;
            Player player = GetPlayer(connectionId);
            if (player != null)
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    player.UserName = userName;
                }

                player.Status = Enums.PlayerStatus.SEARCHING_MATCH;

                // Try to get a match 
                game = CheckMatchMaking(player);
                if (game == null)
                {
                    _waitingPlayers.Enqueue(player);
                }
            }

            return game;
        }

        public static Game JoinPrivateRoom(string connectionId, string userName, string roomName)
        {
            Game game = GetGame(roomName);
            Player player = GetPlayer(connectionId);
            if (player != null && game != null)
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    player.UserName = userName;
                }
                player.Status = Enums.PlayerStatus.IN_ROOM;

                game.AddPlayer(player);
            }
            return game;
        }

        public static void PlayerDisconnected(string connectionId, Exception exception)
        {
            Player player = GetPlayer(connectionId);
            Game game = GetLastGamePlayed(player);
            ManagePlayerLeft(player, game);
        }

        public static void PlayerLeft(string connectionId, Guid matchId)
        {
            Player player = GetPlayer(connectionId);
            Game game = GetGame(matchId, player);
            ManagePlayerLeft(player, game);
        }

        public static Game SetMapInformation(string connectionId, Guid matchId, int width, int height)
        {
            Game game = GetGame(matchId, GetPlayer(connectionId));
            if (game != null)
            {
                game.SetMapInformation(width, height);
                return game;
            }
            return null;
        }

        public static Game SetPlayerDirection(string connectionId, Guid matchId, Direction direction)
        {
            Player player = GetPlayer(connectionId);
            Game game = GetGame(matchId, player);

            if (game != null && game.SetPlayerDirection(player, direction))
            {
                return game;
            }
            return null;
        }

        public static Game SetPlayerReady(string connectionId, Guid matchId, bool isReady)
        {
            Player player = GetPlayer(connectionId);
            Game game = GetGame(matchId, player);

            if (player != null && game != null)
            {
                if (isReady)
                {
                    player.Status = Enums.PlayerStatus.READY;
                }
                else
                {
                    player.Status = Enums.PlayerStatus.IN_ROOM;
                }

                if (game.Players.All(p => p.Status == Enums.PlayerStatus.READY))
                {
                    // Start game asynchronously
                    Task.Factory.StartNew(async () =>
                    {
                        await LaunchGame(game);
                    });

                    return game;
                }
            }
            return null;
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private static Game CheckMatchMaking(Player player)
        {
            Game game = null;

            if (player == null)
            {
                return game;
            }

            if (_waitingGames.TryDequeue(out Game waitingGame) && waitingGame != null)
            {
                if (waitingGame.Players[0] == null)
                {
                    waitingGame.Players[0] = player;
                    return waitingGame;
                }
                else if (waitingGame.Players[1] == null)
                {
                    waitingGame.Players[1] = player;
                    return waitingGame;
                }
            }

            if (_waitingPlayers.TryDequeue(out Player opponentPlayer) && opponentPlayer != null)
            {
                game = CreateGame(new Game(opponentPlayer, player, Enums.GameType.MATCH_MAKING));
                opponentPlayer.Status = Enums.PlayerStatus.IN_ROOM;
                player.Status = Enums.PlayerStatus.IN_ROOM;
            }
            return game;
        }

        private static Game CreateGame(Game game)
        {
            if (game != null)
            {
                _games.TryAdd(game.Id, game);
                game.Finished += Game_Finished;
                game.InformationUpdated += Game_InformationUpdated;
            }
            return game;
        }

        private static void Game_InformationUpdated(object sender, EventArgs e)
        {
            if (sender is Game game)
            {
                OnInformationUpdated(game, EventArgs.Empty);
            }
        }

        private static void Game_Finished(object sender, EventArgs e)
        {
            if (sender is Game game)
            {
                OnGameUpdated(game, new GameUpdatedEventArgs(true, false));

                game.Finished -= Game_Finished;
                game.InformationUpdated -= Game_InformationUpdated;

                Task.Factory.StartNew(() =>
                {
                    if (_games.TryRemove(game.Id, out Game gameToArchive) && gameToArchive != null)
                    {
                        _gameHistory.TryAdd(gameToArchive.Id, gameToArchive);
                    }
                });
            }
        }

        private static Game GetGame(string roomName)
        {
            return (!string.IsNullOrEmpty(roomName)) ? _games.Values.FirstOrDefault(g => g != null && g.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase)) : null;
        }

        private static Game GetGame(Guid matchId, Player player)
        {
            Game game = null;
            if (_games.ContainsKey(matchId) && player != null)
            {
                game = _games[matchId];
                if (game.Players.All(p => p == null || p.ConnnectionId != player.ConnnectionId))
                {
                    game = null;
                }
            }
            return game;
        }

        private static Game GetLastGamePlayed(Player player)
        {
            if (player != null)
            {
                return _games.Values.FirstOrDefault(g => g != null && g.Players.Any(p => p != null && p.ConnnectionId == player.ConnnectionId));
            }
            return null;
        }

        private static Player GetPlayer(string connectionId)
        {
            if (!string.IsNullOrEmpty(connectionId))
            {
                if (!_players.ContainsKey(connectionId))
                {
                    _players.TryAdd(connectionId, new Player(connectionId));
                }
                return _players[connectionId];
            }
            return null;
        }

        private static async Task LaunchGame(Game game)
        {
            if (game == null)
            {
                return;
            }

            game.Initialize();
            await game.Start();
        }

        private static void ManagePlayerLeft(Player player, Game game)
        {
            if (player == null)
            {
                return;
            }

            // Recreate queue without player
            List<Player> playersInMatchMaking = _waitingPlayers.ToList();
            playersInMatchMaking.RemoveAll(p => p != null && p.ConnnectionId == player.ConnnectionId);
            _waitingPlayers = new ConcurrentQueue<Player>(playersInMatchMaking);

            // Manage game where the player is
            if (game != null)
            {
                game.PlayerLeft(player);

                if (game.Players.All(p => p == null))
                {
                    // No players left, the game is removed
                    _games.TryRemove(game.Id, out Game removedGame);
                }
                else if (game.Players.Any(p => p == null) && game.GameType == GameType.MATCH_MAKING)
                {
                    // Try to get opponent
                    if (_waitingPlayers.TryDequeue(out Player opponentPlayer) && opponentPlayer != null)
                    {
                        game.AddPlayer(opponentPlayer);
                        OnGameUpdated(game, new GameUpdatedEventArgs(false, true, player.ConnnectionId));
                    }
                    else
                    {
                        _waitingGames.Enqueue(game);
                        OnGameUpdated(game, new GameUpdatedEventArgs(false, false, player.ConnnectionId));
                    }
                }
                else if (game.Players.Any(p => p == null) && game.GameType == GameType.PRIVATE_ROOM)
                {
                    OnGameUpdated(game, new GameUpdatedEventArgs(false, false, player.ConnnectionId));
                }
            }
        }

        #endregion PRIVATE METHODS

        #region EVENTS

        public static event EventHandler InformationUpdated;

        private static void OnInformationUpdated(object sender, EventArgs e)
        {
            EventHandler eventHandler = InformationUpdated;
            eventHandler?.Invoke(sender, e);
        }

        public static event GameUpdatedEventHandler GameUpdated;

        private static void OnGameUpdated(object sender, GameUpdatedEventArgs e)
        {
            GameUpdatedEventHandler eventHandler = GameUpdated;
            eventHandler?.Invoke(sender, e);
        }

        #endregion EVENTS
    }
}
