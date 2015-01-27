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
				select new {
					Id = request.Id,
					Url = request.Url,
					Path = request.Path
				}",
				Reduce = @"from result in results
				group result by new {
					Url = result.Url,
					Path = result.Path
				} into g
				select new {
					Id = g.Max(x => x.Id),
					Url = g.Key.Url,
					Path = g.Key.Path
				}"
			};
		}
	}
}
