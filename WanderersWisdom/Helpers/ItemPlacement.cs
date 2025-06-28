using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;

namespace WanderersWisdom.Helpers
{
    public class ItemPlacement : MutablePlacement
    {
        public ItemPlacement(string name, ContainerLocation location, int geoCost) : base(name)
        {
            Cost = new GeoCost(geoCost);
            Location = location;
            containerType = Container.Shiny;
            Items.Add(Finder.GetItem(name));
        }
    }
}
