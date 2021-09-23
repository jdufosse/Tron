using TronEngine.Enums;
using TronEngine.Models;

namespace Tron.ViewModels.Tron
{
    /// <summary>
    /// 
    /// </summary>
    public class DataGameViewModel : GameViewModelBase
    {
        #region PROPERTIES

        public bool HasScoreChanged { get; protected set; }

        public int ScorePlayer1 { get; protected set; }

        public int ScorePlayer2 { get; protected set; }

        public BikeInformation BikeInformationPlayer1 { get; protected set; }

        public BikeInformation BikeInformationPlayer2 { get; protected set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public DataGameViewModel(Game game) : base(game)
        {
            this.ScorePlayer1 = game.Scores[0];
            this.ScorePlayer2 = game.Scores[1];
            this.BikeInformationPlayer1 = game.BikeInformations[0];
            this.BikeInformationPlayer2 = game.BikeInformations[1];
            this.Width = game.MapWidth;
            this.Height = game.MapHeight;
            this.HasScoreChanged = game.Status != GameStatus.RUNNING;
        }

        #endregion CONSTRUCTORS
    }
}
