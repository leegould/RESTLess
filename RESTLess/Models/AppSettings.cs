namespace RESTLess.Models
{
    public class AppSettings
    {
        public static AppSettings CreateDefault()
        {
            return new AppSettings
                   {
                       Height = 800,
                       Width = 600,
                       Left = 0,
                       Top = 0,
                       LoadResponses = true,
                       RequestSettings = RequestSettings.CreateDefault()
                   };
        }

        public double Left { get; set; }

        public double Top { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
        
        public RequestSettings RequestSettings { get; set; }

        public bool LoadResponses { get; set; }
    }
}
