using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using TurnBase;
using TurnBase.KaNoBu;


[SceneReference("GameInitNone.tscn")]
public partial class GameInitNone : IGameInit
{
    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Run(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
    {
        var preparedField = Field2D.Create(model.Request.Width, model.Request.Height);
        for (var i = 0; i < model.Request.Width; i++)
        {
            for (var j = 0; j < model.Request.Height; j++)
            {
                var p = new Point { X = i, Y = j };
                var ship = model.Request.AvailableFigures[0];
                preparedField[p] = KaNoBuFigure.Create(model.PlayerId, ship, true, 0);
                model.Request.AvailableFigures.Remove(ship);
            }
        }

        return new InitResponseModel<KaNoBuInitResponseModel>
        {
            Name = "None",
            Response = new KaNoBuInitResponseModel(preparedField)
        };
    }
}
