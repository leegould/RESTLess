using System.ComponentModel.Composition;

using Caliburn.Micro;

namespace RESTLess.Controls
{
    [Export(typeof(AboutViewModel))]
    public class AboutViewModel : Screen
    {
        public void OkButton()
        {
            TryClose();
        }
    }
}
