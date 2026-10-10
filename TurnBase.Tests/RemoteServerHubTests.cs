using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TurnBase;

[TestFixture]
public class RemoteServerHubTests
{
    [Test]
    public void JoinReturnsOnlyPlayersRegisteredForRequestedGame()
    {
        var hub = new RemoteServerHub();
        hub.RegisterPlayer("player-a", "game-a");
        hub.RegisterPlayer("player-b", "game-b");

        Assert.IsFalse(hub.TryJoin("missing-game", out _));
        Assert.IsTrue(hub.TryJoin("game-b", out var joinedPlayer));
        Assert.AreEqual("player-b", joinedPlayer);
        Assert.IsTrue(hub.TryJoin("game-a", out joinedPlayer));
        Assert.AreEqual("player-a", joinedPlayer);
        Assert.IsFalse(hub.TryJoin("game-a", out _));
    }

    [Test]
    public void JoinWithoutGameIdReturnsAPlayerFromAnyAvailableGame()
    {
        var hub = new RemoteServerHub();
        hub.RegisterPlayer("player-a", "game-a");
        hub.RegisterPlayer("player-b", "game-b");

        Assert.IsTrue(hub.TryJoin(null, out var joinedPlayer));
        Assert.That(joinedPlayer, Is.AnyOf("player-a", "player-b"));
        Assert.IsTrue(hub.TryJoin(string.Empty, out joinedPlayer));
        Assert.That(joinedPlayer, Is.AnyOf("player-a", "player-b"));
        Assert.IsFalse(hub.TryJoin(null, out _));
    }

    [Test]
    public void JoinWithUnknownGameIdDoesNotJoinAnotherGame()
    {
        var hub = new RemoteServerHub();
        hub.RegisterPlayer("player-a", "game-a");

        Assert.IsFalse(hub.TryJoin("missing-game", out _));
        Assert.IsTrue(hub.TryJoin("game-a", out var joinedPlayer));
        Assert.AreEqual("player-a", joinedPlayer);
    }

    [Test]
    public async Task RemoteRequestIsQueuedAndMatchingAnswerCompletesIt()
    {
        var hub = new RemoteServerHub();
        hub.RegisterPlayer("player-a", "game-a");
        var expectedResponse = new GamePlayersInitializedCommunicationModel();

        var responseTask = hub.SendRequest<ICommunicationModel>("player-a", new GameStartedCommunicationModel());
        var request = await hub.WaitForRequestAsync("player-a", TimeSpan.FromSeconds(1));

        Assert.IsInstanceOf<GameStartedCommunicationModel>(request);
        Assert.IsTrue(hub.SubmitResponse("player-a", expectedResponse));
        Assert.AreSame(expectedResponse, await responseTask);
    }

    [Test]
    public async Task WaitForRequestReturnsNullAfterTimeout()
    {
        var hub = new RemoteServerHub();
        var request = await hub.WaitForRequestAsync("unknown-player", TimeSpan.FromMilliseconds(1));

        Assert.IsNull(request);
    }
}
