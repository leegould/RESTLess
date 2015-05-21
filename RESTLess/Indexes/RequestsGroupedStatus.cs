using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace RESTLess.Indexes
{
	class RequestsGroupedStatus: AbstractIndexCreationTask
	{
		public override string IndexName
		{
			get
			{
				return "Requests/Grouped/Status";
			}
		}
		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinition {
						Map = @"from response in docs.Responses
								 select new {
									 Id = response.Id,
									 RequestId = response.RequestId,
									 Url = LoadDocument(response.RequestId).Url,
									 Path = LoadDocument(response.RequestId).Path,
									 StatusCode = response.StatusCode
								 }",
						Reduce = @" from result in results
								 group result by new {
									StatusCode = result.StatusCode,
									Url = result.Url,
									Path = result.Path
								 } into g
								 select new {
									 Id = g.Max(x => x.Id),
									 RequestId = g.Max(x => x.RequestId),
									 Url = g.Key.Url,
									 Path = g.Key.Path,
									 StatusCode = g.Key.StatusCode
								 }
			 "};
		}
	}
}
