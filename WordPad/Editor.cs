using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;

namespace RectifyPad
{
    public class Editor : RichEditBox
    {
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            // Keyboard state checking removed - now handled in MainWindow through keyboard event overrides
            base.OnKeyDown(e);
        }
    }
}
