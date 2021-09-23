using TronEngine.Enums;

namespace TronEngine.Models
{
    public class Player
    {
        #region PROPERTIES

        public string ConnnectionId { get; }

        public PlayerStatus Status { get; set; }

        public string UserName { get; set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public Player(string connectionId)
        {
            this.ConnnectionId = connectionId;
        }

        #endregion CONSTRUCTORS
    }
}
