using System;
using TronEngine.Models;

namespace Tron.ViewModels.Tron
{
    public class MapViewModel : ViewModelBase
    {
        #region PROPERTIES

        public Guid GameId { get; protected set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public MapViewModel(Game game)
        {
            if (game != null)
            {
                this.GameId = game.Id;
                this.Width = game.MapWidth;
                this.Height = game.MapHeight;
            }
            else
            {
                this.HasError = true;
                this.ErrorMessage = "No game";
            }
        }

        #endregion CONSTRUCTORS
    }
}
