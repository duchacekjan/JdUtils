using JdUtils.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using cnv = JdUtils.Converters;
namespace JgsUtils.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {


        private Button m_btn;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += AfterLoaded;
            var t1 = E1.Collapsed;
            var cnv = new cnv.EnumConverter();
            var t2 = cnv.Convert(t1, typeof(Visibility), null, null);
        }

        private void AfterLoaded(object sender, RoutedEventArgs e)
        {
            m_btn = FindName("PART_Test") as Button;
            m_btn.SetValueSafe(s => s.Content, "POKUS");
        }
    }
    public enum E1
    {
        Collapsed = 2,
        [Description("Zabirajici misto")]
        Hidden = 1
    }
}
