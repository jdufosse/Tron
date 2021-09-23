namespace Tron.ViewModels
{
    /// <summary>
    /// View model base
    /// </summary>
    public abstract class ViewModelBase
    {
        /// <summary>
        /// Has error
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
