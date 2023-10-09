using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

public class DatabaseChannelTest
{
    private readonly IConfiguration _configuration = new TestConfiguration();
    private readonly TestClock _clock;
    private readonly IDatabase _sut;

    readonly Database.AsyncMessageReceiver _ignoringReceiver = (_, _) => { };

    public DatabaseChannelTest()
    {
        _clock = new TestClock();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);
    }

    [Fact]
    public void Subscribe_Returns_No_Previous_Subscribes_On_New_Channel()
    {
        int subscribers = _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        Equal(1, subscribers);
    }

    [Fact]
    public void Subscribe_Returns_Correct_Number_Of_Subscribers_On_Existing_Channel()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        int s2 = _sut.Subscribe("client-2", "channel", _ignoringReceiver);
        Equal(2, s2);
    }

    [Fact]
    public void Subscribe_Twice_Returns_Only_One_Subscription()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        int subscribers = _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        Equal(1, subscribers);
    }

    [Fact]
    public void Unsubscribe_For_Non_Existing_Channel_Succeeds()
    {
        _sut.Unsubscribe("client-1", "does-not-exit");
    }

    [Fact]
    public void Unsubscribe_For_Existing_Subscription_Succeeds()
    {
        _sut.Subscribe("client-1", "channel", _ignoringReceiver);
        _sut.Unsubscribe("client-1", "channel");

        int subscriptions = _sut.Subscribe("client-2", "channel", _ignoringReceiver);
        Equal(1, subscriptions);
    }

    [Fact]
    public void Unsubscribe_For_All_Subscriptions_Succeeds()
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

    [Fact]
    public void Publish_Triggers_Delegate()
    {
        List<byte[]> messages = new();
        _sut.Subscribe("client-1", "channel", (channel, message) =>
        {
            Equal("channel", channel);
            messages.Add(message);
        });

        _sut.Publish("channel", new byte[]
        {
            1,
            2,
            3
        });

        Single(messages);
        Equal(new byte[]
        {
            1,
            2,
            3
        }, messages[0]);
    }

    [Fact]
    public void Publish_To_Another_Channel_Does_Not_Trigger_Delegate()
    {
        List<byte[]> messages = new();
        _sut.Subscribe("client-1", "channel", (channel, message) =>
        {
            Equal("channel", channel);
            messages.Add(message);
        });

        _sut.Publish("channel-2", new byte[]
        {
            1,
            2,
            3
        });

        Empty(messages);
    }

    [Fact]
    public void Publish_Removes_Client_If_Delegate_Throws_Exception()
    {
        List<byte[]> messages = new();
        _sut.Subscribe("client-1", "channel", (channel, message) =>
        {
            Equal("channel", channel);
            messages.Add(message);
            throw new InvalidOperationException("Stop handling this");
        });

        _sut.Publish("channel", new byte[]
        {
            1,
            2,
            3
        });
        // The second time should not trigger the handler, since
        // it got removed.
        _sut.Publish("channel", new byte[]
        {
            1,
            2,
            3
        });

        Single(messages);
    }
}
