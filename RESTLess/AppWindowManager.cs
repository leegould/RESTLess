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
                        var appsettings = conn.Query<AppSettings>().FirstOrDefault();

                        window.SizeToContent = SizeToContent.Manual;

                        if (appsettings != null)
                        {
                            window.Width = appsettings.Width;
                            window.Height = appsettings.Height;
                        }
                        else
                        {
                            window.Width = 800;
                            window.Height = 600;
                        }
                    }
                }
            }

            window.Title = "RESTLess";

            return window;
        }
    }
}
