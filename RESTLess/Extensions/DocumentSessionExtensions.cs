using System.Linq;

using Raven.Client;

namespace RESTLess.Extensions
{
    public static class DocumentSessionExtensions
    {
        public static void ClearDocuments<T>(this IDocumentSession session)
        {
            var objects = session.Query<T>().ToList();
            
            foreach (var obj in objects)
            {
                session.Delete(obj);
            }

            session.SaveChanges();
        }
    }
}
