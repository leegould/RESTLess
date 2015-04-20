using System;
using System.Linq;
using System.Linq.Expressions;

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

        public static void ClearDocumentsWhere<T>(this IDocumentSession session, Expression<Func<T, bool>> expression)
        {
            var objects = session.Query<T>().Where(expression).ToList();

            foreach (var obj in objects)
            {
                session.Delete(obj);
            }

            session.SaveChanges();
        }
    }
}
