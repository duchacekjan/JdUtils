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
        private Button m_btn;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += AfterLoaded;
        }

        private void AfterLoaded(object sender, RoutedEventArgs e)
        {
            m_btn = FindName("PART_Test") as Button;
            m_btn.SetValueSafe(s => s.Content, "POKUS");
        }
    }
}
