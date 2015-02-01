using Caliburn.Micro;

namespace RESTLess.Models.Interface
{
    /// <summary>
    ///   Interface used to identify a menu item.
    /// </summary>
    public interface IMenuItem : IChild, IHaveDisplayName, IParent<IMenuItem>
    {
        #region Properties
        /// <summary>
        ///   Gets the collection of menu items.
        /// </summary>
        /// <value>The collection of menu items.</value>
        BindableCollection<IMenuItem> Items { get; }
        #endregion
    }
}
