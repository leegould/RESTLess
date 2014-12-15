using System.Windows;

using Caliburn.Micro;

namespace RESTLess
{
    public class AppWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = base.EnsureWindow(model, view, isDialog);

            window.SizeToContent = SizeToContent.Manual;

            window.Width = 400;
            window.Height = 400;

            return window;
        }
    }
}
