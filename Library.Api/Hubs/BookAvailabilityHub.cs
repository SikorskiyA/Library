using Microsoft.AspNetCore.SignalR;

namespace Library.Api.Hubs;

public class BookAvailabilityHub : Hub
{
    public async Task SubscribeToBook(string bookId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, bookId);
    }

    public async Task UnsubscribeFromBook(string bookId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, bookId);
    }
}