using System;
using TronEngine.Enums;
using TronEngine.Models;

namespace Tron.ViewModels.Tron
{
    public abstract class GameViewModelBase : ViewModelBase
    {
        #region PROPERTIES

        public Guid Id { get; protected set; }

        public string Name { get; protected set; }

        public PlayerViewModel Player1 { get; protected set; }

        public PlayerViewModel Player2 { get; protected set; }

        public GameType Type { get; set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        protected GameViewModelBase(Game game)
        {
            if (game == null || String.IsNullOrEmpty(game.Name))
            {
                this.HasError = true;
                this.ErrorMessage = "Unspecified game";
                return;
            }

            this.Id = game.Id;
            this.Name = game.Name;
            this.Type = game.GameType;

            this.Player1 = this.FillPlayerViewModel(game.Players[0]);

            if (game.GameType != GameType.PRIVATE_ROOM || game.Players[1] != null)
            {
                this.Player2 = this.FillPlayerViewModel(game.Players[1]);
            }
        }

        #endregion CONSTRUCTORS

        #region PRIVATE METHODS

        private PlayerViewModel FillPlayerViewModel(Player player)
        {
            PlayerViewModel result = new PlayerViewModel(player);
            if (result.HasError)
            {
                this.HasError = true;
                this.ErrorMessage = "An error has occured while loading player";
            }
            return result;
        }

        #endregion PRIVATE METHODS
    }
}
