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
    public class BlackKey : PianoKey
    {
        static BlackKey()
        {
            BackgroundProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(Brushes.Black));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(typeof(BlackKey)));
            ForegroundProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(Brushes.White));
            HeightProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(95.0));
            MarginProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(new Thickness(-100.0, 0, -100.0, 0.0)));
            VerticalAlignmentProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(VerticalAlignment.Top));
            WidthProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(11.0));
            Panel.ZIndexProperty.OverrideMetadata(typeof(BlackKey), new FrameworkPropertyMetadata(1));
        }

    }
}
