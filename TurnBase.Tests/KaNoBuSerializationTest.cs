using System;
using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuSerializationTest
{
    [Test]
    public void PromotionKeepsCorrectBattleResult()
    {
        var result = CommunicationSerializer.SerializeObject(new InitResponseModel<KaNoBuInitResponseModel>
        {
          Name = "ResultName",
          Response = new KaNoBuInitResponseModel(new Field2D(new IFigure[1,1], new bool[1,1]))
        });

        Console.WriteLine(result);

        Assert.AreEqual("{\"Data\":{\"$type\":\"InitResponseModel`1[[TurnBase.KaNoBu.KaNoBuInitResponseModel, TurnBase.KaNoBu]], TurnBase\",\"Name\":\"ResultName\",\"Response\":{\"Field\":{\"Width\":1,\"Height\":1}}}}", result);
    }
}
