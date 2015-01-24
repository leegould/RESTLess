using System.Collections.Generic;

namespace RESTLess.Models
{
    public class ResultGrouped
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public IList<string> PathSplit
        {
            get
            {
                if (!string.IsNullOrEmpty(Path))
                {
                    return Path.Split('/');
                }
                return null;
            }
        }
    }
}
