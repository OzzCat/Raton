using Mapsui.Layers;

namespace Raton.Map.Models
{
    public interface IModelWithLayer
    {
        public Layer? Layer { get; }
    }
}
