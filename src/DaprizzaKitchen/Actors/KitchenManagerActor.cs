using Dapr.Actors.Runtime;
using DaprizzaInterfaces;
using DaprizzaModels;

namespace DaprizzaKitchen.Actors;

public class KitchenManagerActor(ActorHost host) : Actor(host), IKitchenManagerActor, IRemindable
{
    private const string chefsKey = "Chefs";

    public async Task RegisterChefs(IEnumerable<Chef> chefs)
    {
        await StateManager.SetStateAsync(chefsKey, chefs);

        await this.RegisterReminderAsync("CheckOnKitchen", null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public async Task<IEnumerable<Chef>> ListChefs()
    {
        return await StateManager.GetStateAsync<IEnumerable<Chef>>(chefsKey);
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        return Task.CompletedTask;
    }
}

