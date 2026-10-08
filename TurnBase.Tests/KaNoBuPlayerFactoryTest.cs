using NUnit.Framework;
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
}
