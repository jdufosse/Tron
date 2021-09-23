using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TronEngine.Enums;
using TronEngine.Exceptions;

namespace TronEngine.Models
{
    public class Game
    {
        #region CONSTANTES

        public const int START_ANIMATION_DURATION = 3000;

        private const string _PREFIX_PRIAVTE_ROOM = "PR_";
        private const string _PREFIX_MATCH_MAKING = "MM_";
        private const int _MAP_WIDTH_MIN = 600;
        private const int _MAP_HEIGHT_MIN = 400;
        private const int _MOVE_INTERVAL = 250;
        private const int _MOVE_STEP = 3;
        private const short _MAP_VALUE_PLAYER1 = 1;
        private const short _MAP_VALUE_PLAYER2 = 2;

        #endregion CONSTANTES

        #region FIELDS

        private static int _name_index = 1;
        private bool _isInitialized = false;
        private short[,] _map = null;

        #endregion FIELDS

        #region PROPERTIES

        public int MapWidth { get; protected set; } = -1;

        public int MapHeight { get; protected set; } = -1;

        public Guid Id { get; private set; }

        public string Name { get; protected set; }

        public Player[] Players { get; protected set; } = new Player[2];

        public BikeInformation[] BikeInformations { get; protected set; } = new BikeInformation[2];

        public GameStatus Status { get; protected set; }

        public GameType GameType { get; protected set; }

        public int[] Scores { get; protected set; } = new int[2];

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public Game(Player player1, Player player2, GameType gameType) : this()
        {
            if (player2 == null && gameType != GameType.PRIVATE_ROOM)
            {
                throw new TronException("Player two is null");
            }

            this.Players[0] = player1 ?? throw new TronException("Player one is null");
            this.Players[1] = player2;
            this.GameType = gameType;
            this.Name = Game.GenerateName(gameType);
        }

        protected Game()
        {
            this.Id = Guid.NewGuid();
            this.Scores[0] = 0;
            this.Scores[1] = 0;
            this.Status = GameStatus.PREPARATION;
        }

        #endregion CONSTRUCTORS

        #region INTERNAL METHODS

        internal void AddPlayer(Player player)
        {
            if (player != null
                && (this.Status == GameStatus.NONE
                    || this.Status == GameStatus.PREPARATION))
            {
                if (this.Players[0] == null)
                {
                    this.Players[0] = player;
                    this.Status = GameStatus.PREPARATION;
                }
                else if (this.Players[1] == null)
                {
                    this.Players[1] = player;
                    this.Status = GameStatus.PREPARATION;
                }

                if (this.Status == GameStatus.PREPARATION)
                {
                    foreach (Player p in this.Players)
                    {
                        if (p != null)
                        {
                            p.Status = PlayerStatus.IN_ROOM;
                        }
                    }
                }
            }
        }

        internal Player GetOpponentPlayer(Player player)
        {
            return (player != null && !string.IsNullOrEmpty(player.ConnnectionId)) ? this.Players.FirstOrDefault(p => p != null && player.ConnnectionId != p.ConnnectionId) : null;
        }

        internal void Initialize()
        {
            // Check width map
            if (this.MapWidth < _MAP_WIDTH_MIN)
            {
                this.MapWidth = _MAP_WIDTH_MIN;
            }

            // Check height map
            if (this.MapHeight < _MAP_WIDTH_MIN)
            {
                this.MapHeight = _MAP_HEIGHT_MIN;
            }

            this._map = new short[this.MapWidth, this.MapHeight];

            // Initialize intial bike position
            this.BikeInformations[0] = new BikeInformation
            {
                X = this.MapWidth / 4,
                Y = this.MapHeight / 2,
                Direction = Direction.RIGHT,
                Traces = new List<Trace>()
            };
            this.BikeInformations[1] = new BikeInformation
            {
                X = this.MapWidth * 3 / 4,
                Y = this.MapHeight / 2,
                Direction = Direction.LEFT,
                Traces = new List<Trace>()
            };

            this._isInitialized = true;
        }

        internal void PlayerLeft(Player player)
        {
            if (player == null)
            {
                return;
            }

            switch (Status)
            {
                case GameStatus.NONE:
                case GameStatus.PREPARATION:
                    this.RemovePlayer(player);
                    break;
                case GameStatus.RUNNING:
                case GameStatus.PAUSED:
                    this.Status = GameStatus.FINISHED;
                    this.OnFinished(EventArgs.Empty);
                    break;
                default:
                    break;
            }
        }

        internal bool SetMapInformation(int width, int height)
        {
            bool result = false;
            if (width > _MAP_WIDTH_MIN && (this.MapWidth == -1 || this.MapWidth > width))
            {
                this.MapWidth = width;
                result = true;
            }

            if (height > _MAP_HEIGHT_MIN && (this.MapHeight == -1 || this.MapHeight > height))
            {
                this.MapHeight = height;
                result = true;
            }
            return result;
        }

        internal void SetOpponentPlayer(Player player)
        {
            if (this.Players[1] == null && this.GameType == GameType.PRIVATE_ROOM)
            {
                this.Players[1] = player;
            }
        }

        internal bool SetPlayerDirection(Player player, Direction direction)
        {
            if (player != null && player.Status == PlayerStatus.IN_GAME && this.Status == GameStatus.RUNNING && direction != Direction.NONE)
            {
                // Found index of player
                if (this.Players[0] != null && this.Players[0].ConnnectionId == player.ConnnectionId && this.BikeInformations[0] != null)
                {
                    this.BikeInformations[0].Direction = direction;
                    return true;
                }
                else if (this.Players[1] != null && this.Players[1].ConnnectionId == player.ConnnectionId && this.BikeInformations[1] != null)
                {
                    this.BikeInformations[1].Direction = direction;
                    return true;
                }
            }
            return false;
        }

        internal async Task Start()
        {
            if (!this._isInitialized)
            {
                return;
            }

            // Wait strating game animation is done to launch game
            await Task.Delay(Game.START_ANIMATION_DURATION);

            foreach (Player player in this.Players)
            {
                if (player != null)
                {
                    player.Status = Enums.PlayerStatus.IN_GAME;
                }
            }

            this.Initialize();

            DateTime dateTime;
            TimeSpan executionTime = new TimeSpan();
            int sleepingTime;
            bool doNothingInFirstLoop = true;
            this.Status = GameStatus.RUNNING;

            while (this.Status == GameStatus.RUNNING)
            {
                if (doNothingInFirstLoop)
                {
                    // First loop
                    doNothingInFirstLoop = false;
                    this.OnInformationUpdated(EventArgs.Empty);
                }
                else
                {
                    sleepingTime = (executionTime.TotalSeconds > 0) ? _MOVE_INTERVAL - Convert.ToInt32(Math.Floor(executionTime.TotalSeconds)) : _MOVE_INTERVAL;

                    if (sleepingTime > 0)
                    {
                        await Task.Delay(sleepingTime);
                    }

                    dateTime = DateTime.Now;
                    Console.WriteLine("Timer : " + dateTime.ToString("o"));


                    // Apply direction of both players
                    this.ApplyMove(0, out bool hasDetectedConflictPLayer1);
                    this.ApplyMove(1, out bool hasDetectedConflictPLayer2);

                    if (hasDetectedConflictPLayer1)
                    {
                        this.Scores[1]++;
                        this.Status = GameStatus.PAUSED;
                    }
                    if (hasDetectedConflictPLayer2)
                    {
                        this.Scores[0]++;
                        this.Status = GameStatus.PAUSED;
                    }
                    executionTime = DateTime.Now - dateTime;
                    Console.WriteLine("Timer execution : " + executionTime.ToString("G"));
                    this.OnInformationUpdated(EventArgs.Empty);
                }
            }
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private static string GenerateName(GameType gameType)
        {
            return string.Concat((gameType == GameType.PRIVATE_ROOM) ? _PREFIX_PRIAVTE_ROOM : _PREFIX_MATCH_MAKING, (_name_index++).ToString("D4"));
        }

        private void ApplyMove(int index, out bool hasDetectConflict)
        {
            hasDetectConflict = false;
            BikeInformation bikeInformation = this.BikeInformations[index];
            Trace lastTrace = (bikeInformation.Traces.Count > 0) ? bikeInformation.Traces[bikeInformation.Traces.Count - 1] : null;
            int lastPlayerX = bikeInformation.X;
            int lastPlayerY = bikeInformation.Y;

            Trace nextTrace = null;
            switch (bikeInformation.Direction)
            {
                case Direction.RIGHT:
                    bikeInformation.X += _MOVE_STEP;
                    break;
                case Direction.LEFT:
                    bikeInformation.X -= _MOVE_STEP;
                    break;
                case Direction.UP:
                    bikeInformation.Y -= _MOVE_STEP;
                    break;
                case Direction.DOWN:
                    bikeInformation.Y += _MOVE_STEP;
                    break;
                default:
                    return;
            }

            // Out of map
            if (bikeInformation.X <= 0 || bikeInformation.X > MapWidth
                || bikeInformation.Y <= 0 || bikeInformation.Y > MapHeight)
            {
                hasDetectConflict = true;
                return;
            }

            nextTrace = new Trace(lastPlayerX, lastPlayerY, bikeInformation.X, bikeInformation.Y, bikeInformation.Direction);
            hasDetectConflict |= this.FillMap(nextTrace, this.GetMapValue(index), lastTrace != null);
            if (lastTrace != null && lastTrace.Direction == bikeInformation.Direction)
            {
                lastTrace.ContinueTo(_MOVE_STEP);
            }
            else
            {
                bikeInformation.Traces.Add(nextTrace);
            }
        }

        private bool FillMap(Trace trace, short playerValue, bool isCutTrace = false)
        {
            if (trace != null)
            {
                if (isCutTrace)
                {
                    return this.FillMap((trace.Direction == Direction.RIGHT) ? trace.TopLeftX + Trace.BIKE_WIDTH_SIDE + 1 : trace.TopLeftX,
                        (trace.Direction == Direction.DOWN) ? trace.TopLeftY + Trace.BIKE_WIDTH_SIDE + 1 : trace.TopLeftY,
                        (trace.Direction == Direction.LEFT) ? trace.BottomRightX - Trace.BIKE_WIDTH_SIDE - 1 : trace.BottomRightX,
                        (trace.Direction == Direction.UP) ? trace.BottomRightY - Trace.BIKE_WIDTH_SIDE - 1 : trace.BottomRightY,
                        playerValue);
                }
                else
                {
                    return this.FillMap((trace.Direction == Direction.RIGHT) ? trace.TopLeftX + 1 : trace.TopLeftX,
                        (trace.Direction == Direction.DOWN) ? trace.TopLeftY + 1 : trace.TopLeftY,
                        (trace.Direction == Direction.LEFT) ? trace.BottomRightX - 1 : trace.BottomRightX,
                        (trace.Direction == Direction.UP) ? trace.BottomRightY - 1 : trace.BottomRightY,
                        playerValue);
                }
            }
            return false;
        }

        private bool FillMap(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, short playerValue)
        {
            // All coordonnees must be substract by 1 to give index of array.
            for (int i = topLeftX - 1; i < bottomRightX - 1; i++)
            {
                for (int j = topLeftY - 1; j < bottomRightY - 1; j++)
                {
                    if (this._map[i, j] == 0)
                    {
                        this._map[i, j] = playerValue;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private short GetMapValue(int index)
        {
            return (index == 0) ? _MAP_VALUE_PLAYER1 : _MAP_VALUE_PLAYER2;
        }

        private void RemovePlayer(Player player)
        {
            if (player != null)
            {
                for (int i = 0; i < this.Players.Length; i++)
                {
                    if (this.Players[i] != null && this.Players[i].ConnnectionId == player.ConnnectionId)
                    {
                        this.Players[i] = null;
                        break;
                    }
                }
            }
        }

        #endregion PRIVATE METHODS

        #region EVENTS

        public event EventHandler InformationUpdated;

        protected virtual void OnInformationUpdated(EventArgs e)
        {
            this.InformationUpdated?.Invoke(this, e);
        }

        public event EventHandler Finished;

        protected virtual void OnFinished(EventArgs e)
        {
            this.Finished?.Invoke(this, e);
        }

        #endregion EVENTS
    }
}
