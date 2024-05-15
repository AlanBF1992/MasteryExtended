using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
