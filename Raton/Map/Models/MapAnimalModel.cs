using Mapsui.Layers;
using Raton.Models.DbModels;
using ReactiveUI;
using System.Collections.Generic;
using static Raton.Models.DbModels.Enums.SexEnumClass;

namespace Raton.Map.Models
{
    public class MapAnimalModel : ReactiveObject, IModelWithLayer
    {
        public int TableID { get; private set; }
        public string ID { get; set; }
        public SexEnum Sex { get; set; }
        public List<string> Series { get; set; }
        public Layer? Layer { get; set; }
        public List<MapDatedPointModel>? ActivePoints { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }

        public MapAnimalModel(AnimalModel animal)
        {
            TableID = animal.TableID;
            ID = animal.ID;
            Sex = animal.Sex;
            _isChecked = false;
            Series = new List<string>();
        }
    }
}
