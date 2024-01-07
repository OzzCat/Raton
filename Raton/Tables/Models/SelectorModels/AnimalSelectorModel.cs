using Raton.Models.DbModels;

namespace Raton.Tables.Models.SelectorModels
{
    public class AnimalSelectorModel : IndexableSelectorModel<AnimalModel>
    {
        public AnimalSelectorModel(AnimalModel tModel) : base(tModel)
        {
        }
    }
}
