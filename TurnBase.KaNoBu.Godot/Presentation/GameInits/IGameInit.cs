using System.Threading;
using System.Threading.Tasks;
using TurnBase.KaNoBu;

public interface IGameInit
{
    Task<InitResponseModel<KaNoBuInitResponseModel>> Run(InitModel<KaNoBuInitModel> model, CancellationToken token = default);
}