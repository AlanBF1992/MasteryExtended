using StardewValley;

namespace MasteryExtended.Menu.Pages
{
    internal class MasteryGeneralPage : MasteryPage
    {
        public MasteryGeneralPage()
            : base((int)Utility.getTopLeftPositionForCenteringOnScreen(720, 320).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(720, 320).Y, 720, 320, showUpperRightCloseButton: true)
        {
        }
    }
}
