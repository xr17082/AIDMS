namespace AIDMS.Client.Infrastructure.Routes
{
    public static class DocumentRoute
    {
        public static string SearchDocuments(string query) => $"api/docs/search-documents?query={query}";
    }
}
