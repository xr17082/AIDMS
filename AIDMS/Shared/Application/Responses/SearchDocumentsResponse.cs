namespace AIDMS.Shared.Application.Responses
{
    public class SearchDocumentsResponse
    {
        public string FileName { get; set; }

        public int PageNumber { get; set; }

        public string Text { get; set; }

        public double Score { get; set; }
    }
}
