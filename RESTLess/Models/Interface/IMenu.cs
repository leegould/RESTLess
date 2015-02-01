using Caliburn.Micro;

namespace RESTLess.Models.Interface
{
    /// <summary>
    ///   Interface used to define a menu.
    /// </summary>
    /// <see cref="http://caliburnmicro.codeplex.com/discussions/254102"/>
    public interface IMenu : IParent<IMenuItem>, IChild
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
