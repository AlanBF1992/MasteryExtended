using StardewValley.Menus;

namespace MasteryExtended.Compatibility.BGM
{
    public interface IBetterGameMenuApi
    {
        /// <summary>
        /// Just check to see if the provided menu is a Better Game Menu,
        /// without actually casting it. This can be useful if you want to remove
        /// the menu interface from the API surface you use.
        /// </summary>
        /// <param name="menu">The menu to check</param>
        bool IsMenu(IClickableMenu? menu);
    }
}
