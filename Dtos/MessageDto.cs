using WebPortfolioApi.Models;

namespace WebPortfolioApi.Dtos;

//Data transfer object to ensure only certain fields are exposed through the API. 
public record MessageDto(string? Name, string? EmailAddress, string? Content);

public static class MessageToDto
{
    //Map a Message object to a MessageDto object
    //This is usually used when we want to include a Message object without the ID in the response 
    public static MessageDto asMessageDto(this Message m)
    {
        return new MessageDto(m.Name, m.EmailAddress, m.Content);
    }
}