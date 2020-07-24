using JdUtils.Converters;
using JdUtils.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace JgsUtils.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public enum E1
        { 
            V1,
            V2
        }

        public enum E2
        {
            V3,
            V4
        }


        private Button m_btn;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += AfterLoaded;
            var t1 = E1.V2;
            var cnv = new EnumConverter();
            var t2 = cnv.Convert(t1, typeof(E2), null, null);
        }

        private void AfterLoaded(object sender, RoutedEventArgs e)
        {
            m_btn = FindName("PART_Test") as Button;
            m_btn.SetValueSafe(s => s.Content, "POKUS");
        }
    }
}
