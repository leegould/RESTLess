using System.Linq;
using Raven.Client;

namespace RESTLess.Extensions
{
    public static class IDocumentSessionExtensions
    {
        public static void ClearDocuments<T>(this IDocumentSession session)
        {
            var objects = session.Query<T>().ToList();
            while (objects.Any())
            {
                foreach (var obj in objects)
                {
                    session.Delete(obj);
                }

                session.SaveChanges();
                //objects = session.Query<T>().ToList();
            }
        }

        public static void ClearDocumentsAsync<T>(this IAsyncDocumentSession session)
        {
            var objects = session.Query<T>().ToList();
            while (objects.Any())
            {
                foreach (var obj in objects)
                {
                    session.Delete(obj);
                }

                session.SaveChangesAsync();
                //objects = session.Query<T>().ToList();
            }
        }
    }
}
