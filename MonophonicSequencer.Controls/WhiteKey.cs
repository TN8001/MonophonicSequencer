using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonophonicSequencer.Controls
{
    public class WhiteKey : PianoKey
    {
        static WhiteKey()
        {
            BackgroundProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(Brushes.Snow));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(typeof(WhiteKey)));
            HeightProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(150.0));
            MarginProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(new Thickness(-2.0, 0, 0.0, 0.0)));
            WidthProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(23.0));
            //TextBlock.FontSizeProperty.OverrideMetadata(typeof(WhiteKey), new FrameworkPropertyMetadata(10.0));
        }
    }
}
