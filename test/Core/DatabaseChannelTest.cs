using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

public class DatabaseChannelTest
{
    private readonly IConfiguration _configuration = new TestConfiguration();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly IDatabase _sut;

    readonly Database.AsyncMessageReceiver _ignoringReceiver = (_, _) => { };

    public DatabaseChannelTest()
    {
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _dateTimeProvider);
    }

    [Fact]
    public void Subscribe_ReturnsNoPreviousSubscribes_OnNewChannel()
    {
        int subscribers = _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        Equal(1, subscribers);
    }

    [Fact]
    public void Subscribe_ReturnsCorrectNumberOfSubscribers_OnExistingChannel()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        int s2 = _sut.Subscribe("client-2", "channel", _ignoringReceiver);
        Equal(2, s2);
    }

    [Fact]
    public void Subscribe_Twice_ReturnsOnlyOneSubscription()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        int subscribers = _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        Equal(1, subscribers);
    }

    [Fact]
    public void Unsubscribe_ForNonExistingChannel_Succeeds()
    {
        _sut.Unsubscribe("client-1", "does-not-exit");
    }

    [Fact]
    public void Unsubscribe_For_ExistingSubscription_Succeeds()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        _sut.Unsubscribe("client-1", "channel");

        int subscriptions = _sut.Subscribe("client-2", "channel", _ignoringReceiver);
        Equal(1, subscriptions);
    }

    [Fact]
    public void Unsubscribe_ForAllSubscriptions_Succeeds()
    {
        _sut.Subscribe("client-1", "channel-1", _ignoringReceiver);
        _sut.Subscribe("client-1", "channel-2", _ignoringReceiver);
        _sut.Subscribe("client-1", "channel-3", _ignoringReceiver);

        var unsubscribedChannels = _sut.UnsubscribeAll("client-1");
        Equal(new[]
        {
            "channel-1",
            "channel-2",
            "channel-3"
        }, unsubscribedChannels.OrderBy(i => i));

        int subsChannel1 = _sut.Subscribe("client-2", "channel-1", _ignoringReceiver);
        Equal(1, subsChannel1);
        int subsChannel2 = _sut.Subscribe("client-2", "channel-2", _ignoringReceiver);
        Equal(1, subsChannel2);
        int subsChannel3 = _sut.Subscribe("client-2", "channel-3", _ignoringReceiver);
        Equal(1, subsChannel3);
    }

}