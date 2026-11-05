using System.Collections.Generic;
using TurnBase;
using TurnBase.KaNoBu;

namespace TurnBase.KaNoBu
{

    public class KaNoBuRulesPredefinedFieldWithShips : KaNoBuRules
    {
        public KaNoBuRulesPredefinedFieldWithShips(Field2D field) : base(field)
        {
        }

        public override bool TryApplyInitResponse(KaNoBuField mainField, int playerNumber, KaNoBuInitResponseModel playerResponse)
        {
            return true;
        }
    }
}