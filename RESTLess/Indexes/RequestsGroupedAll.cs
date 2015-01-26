using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace RESTLess.Indexes
{
	public class RequestsGroupedAll : AbstractIndexCreationTask
	{
		public override string IndexName
		{
			get
			{
				return "Requests/Grouped/All";
			}
		}
		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinition
			{
				Map = @"from request in docs.Requests
let path = request.Path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
from p in path

				select new {
					Id = request.Id,
					Url = request.Url,
					Path = p,
					PathIndex = Array.IndexOf(path, p)
				}",
				Reduce = @"from result in results
				group result by new {
					Url = result.Url,
					Path = result.Path,
					PathIndex = result.PathIndex
				} into g
				select new {
					Id = g.Max(x => x.Id),
					Url = g.Key.Url,
					Path = g.Key.Path,
					PathIndex = g.Key.PathIndex
				}"
			};
		}
	}
}
