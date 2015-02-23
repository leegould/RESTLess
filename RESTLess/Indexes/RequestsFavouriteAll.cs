using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace RESTLess.Indexes
{
	public class RequestsFavouriteAll : AbstractIndexCreationTask
	{
		public override string IndexName
		{
			get
			{
				return "Requests/Favourite/All";
			}
		}

		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinition { Map = @"from request in docs.Requests
						where request.Favourite == true
						select new {
							Id = request.Id,
							Url = request.Url,
							Path = request.Path
						}" };
		}
	}
}
