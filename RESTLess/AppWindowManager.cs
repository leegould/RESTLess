using System.Linq;
using System.Windows;
using Caliburn.Micro;
using RESTLess.Models;

namespace RESTLess
{
    public class AppWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = base.EnsureWindow(model, view, isDialog);

            var m = model as AppViewModel;

            if (m != null)
            {
                var docstore = m.DocumentStore;
                if (docstore != null)
                {
                    using (var conn = docstore.OpenSession())
                    {
                        var appsettings = conn.Query<AppSettings>().FirstOrDefault() ?? AppSettings.CreateDefault();

                        window.SizeToContent = SizeToContent.Manual;
                        
                        window.Top = appsettings.Top;
                        window.Left = appsettings.Left;
                        window.Width = appsettings.Width;
                        window.Height = appsettings.Height;
                    }
                }
            }

            window.Title = "RESTLess";

            return window;
        }
    }
}
