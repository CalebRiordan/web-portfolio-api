using WebPortfolioApi.Models;

namespace WebPortfolioApi.Dtos;

public record MessageDto(string? Name, string? EmailAddress, string? Content);

public static class MessageToDto
{
    public static MessageDto asMessageDto(this Message m)
    {
        return new MessageDto(m.Name, m.EmailAddress, m.Content);
    }
}
