namespace WebPortfolioApi.Models
{
    public class Message
    {   
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? Content { get; set; }
    }
}