using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace RESTLess.Controls
{
    [Export(typeof(ConfirmViewModel))]
    public class ConfirmViewModel: Screen
    {
        public ConfirmViewModel()
        {
        }

        public void OkButton()
        {
            TryClose(true);
        }

        public void CancelButton()
        {
            TryClose(false);
        }
    }
}
