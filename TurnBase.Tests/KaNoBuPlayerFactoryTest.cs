using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

public class KaNoBuPlayerFactoryTest
{
    [Test]
    public void NormalizeServerAddressAddsDefaultRemotePort()
    {
        Assert.AreEqual("http://localhost:8080", KaNoBuPlayerFactory.NormalizeServerAddress("localhost"));
        Assert.AreEqual("http://localhost:8080", KaNoBuPlayerFactory.NormalizeServerAddress("http://localhost"));
        Assert.AreEqual("http://localhost:9000", KaNoBuPlayerFactory.NormalizeServerAddress("http://localhost:9000"));
    }

    [Test]
    public void CreateGameAllowsOneHumanPlayer()
    {
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            FieldSize = 8,
            PlayerTypes = new List<KaNoBuPlayerType> { KaNoBuPlayerType.Human }
        };

        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            new KaNoBuPlayerEasy(),
            null,
            new TestLogger(),
            null,
            null);

        Assert.IsNotNull(game);
    }

    [Test]
    public void CreateGameAllowsZeroHumanPlayer()
    {
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            FieldSize = 8,
            PlayerTypes = new List<KaNoBuPlayerType> { KaNoBuPlayerType.Easy }
        };

        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            new KaNoBuPlayerEasy(),
            null,
            new TestLogger(),
            null,
            null);

        Assert.IsNotNull(game);
    }

    [Test]
    public void CreateGameRejectsMoreThanOneHumanPlayer()
    {
        var logger = new TestLogger();
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            FieldSize = 8,
            PlayerTypes = new List<KaNoBuPlayerType>
            {
                KaNoBuPlayerType.Human,
                KaNoBuPlayerType.Human
            }
        };

        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            new KaNoBuPlayerEasy(),
            null,
            logger,
            null,
            null);

        Assert.IsNull(game);
        Assert.AreEqual("Max one human player is allowed.", logger.LastMessage);
    }

    private sealed class TestLogger : ILogger
    {
        public string LastMessage { get; private set; }

        public void Log(string text)
        {
            this.LastMessage = text;
        }
    }
}
