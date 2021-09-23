using TronEngine.Events;
using TronEngine.Models;

namespace Tron.ViewModels.Tron
{
    public class GameUpdatedViewModel : GameViewModelBase
    {
        #region PROPERTIES

        public bool IsFinished { get; protected set; }

        public int ScorePlayer1 { get; protected set; }

        public int ScorePlayer2 { get; protected set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public GameUpdatedViewModel(Game game, GameUpdatedEventArgs eventArgs) : base(game)
        {
            if (eventArgs != null)
            {

                IsFinished = eventArgs.IsFinished;
            }

            if (game != null)
            {
                ScorePlayer1 = game.Scores[0];
                ScorePlayer2 = game.Scores[1];
            }
        }

        #endregion CONSTRUCTORS
    }
}
