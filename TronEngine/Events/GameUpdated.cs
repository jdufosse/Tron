using System;

namespace TronEngine.Events
{
    public class GameUpdatedEventArgs : EventArgs
    {
        #region PROPERTIES

        public bool IsFinished { get; internal set; }

        public bool IsCreated { get; internal set; }

        public string PlayerLeftConnectionId { get; internal set; }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        internal GameUpdatedEventArgs(bool isFinished, bool isCreated, string playerLeftConnectionId = null)
        {
            this.IsCreated = isCreated;
            this.IsFinished = isFinished;
            this.PlayerLeftConnectionId = playerLeftConnectionId;
        }

        #endregion CONSTRUCTORS
    }

    public delegate void GameUpdatedEventHandler(Object sender, GameUpdatedEventArgs e);
}
