namespace PCG_FDF.Data.ComponentDI
{
    public class BaseUriDI
    {
        public string Current_Uri { get; private set; }

        public BaseUriDI(string current_uri)
        {
            Current_Uri = new Uri(current_uri).Host;
        }
    }
}
