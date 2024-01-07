using Raton.Models.DbModels;
using Raton.Models.DbModels.Enums;
using ReactiveUI;
using static Raton.Models.DbModels.Enums.SexEnumClass;

namespace Raton.Tables.Models
{
    public class TableAnimalModel : ReactiveObject, ITableModel
    {
        public int TableID { get; private set; }
        private string _id;
        public string ID
        {
            get => _id;
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        private string _sex;
        public string Sex
        {
            get => _sex;
            set { this.RaiseAndSetIfChanged(ref _sex, value); }
        }
        private string _comment;
        public string Comment
        {
            get => _comment;
            set { this.RaiseAndSetIfChanged(ref _comment, value); }
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set { this.RaiseAndSetIfChanged(ref _isDirty, value); }
        }

        public TableAnimalModel(AnimalModel animalModel)
        {
            TableID = animalModel.TableID;
            _id = animalModel.ID;
            _sex = SexEnumClass.ConvertFromSexEnumToString(animalModel.Sex);
            _comment = animalModel.Comment ?? string.Empty;
            _isDirty = false;
        }
    }
}
