namespace DevExtreme.Demo.Models
{
    public class SimpleTable
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}