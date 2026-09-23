using TurnBase;

namespace TurnBase
{
    public interface IFieldCopier<TField>
    {
        TField CopyForPlayer(TField field, int playerId);
    }
}