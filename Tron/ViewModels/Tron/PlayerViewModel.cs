using System;
using TronEngine.Models;

namespace Tron.ViewModels.Tron
{
    public class PlayerViewModel : ViewModelBase
    {
        #region PROPERTIES

        public string Name { get; protected set; }

        public string ConnectionId { get; protected set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public PlayerViewModel(Player player)
        {
            if(player  == null || String.IsNullOrEmpty(player.ConnnectionId))
            {
                this.HasError = true;
                this.ErrorMessage = "Unspecified player";
                return;
            }

            this.Name = player.UserName;
            this.ConnectionId = player.ConnnectionId;
        }

        #endregion CONSTRUCTORS
    }
}
