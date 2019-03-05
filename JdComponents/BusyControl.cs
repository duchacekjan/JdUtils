using System.Windows;
using System.Windows.Controls;

namespace JdComponents
{
    /// <summary>
    /// Control for indication of work on background
    /// </summary>
    public class BusyControl : Control
    {
        public static readonly DependencyProperty IsBusyProperty;

        /// <summary>
        /// Static constructor for <see cref="DependencyProperty"/> initialization
        /// </summary>
        static BusyControl()
        {
            var owner = typeof(BusyControl);
            IsBusyProperty = DependencyProperty.Register(nameof(IsBusy), typeof(bool), owner);
            DefaultStyleKeyProperty.OverrideMetadata(owner, new FrameworkPropertyMetadata(owner));
        }

        /// <summary>
        /// Constructor for initialization of preview events to prevent
        /// keyboard and mouse input when is busy
        /// </summary>
        public BusyControl()
        {
            PreviewKeyDown += HandleIfBusy;
            PreviewMouseDoubleClick += HandleIfBusy;
            PreviewMouseDown += HandleIfBusy;
            PreviewMouseWheel += HandleIfBusy;
        }

        private void HandleIfBusy(object sender, RoutedEventArgs e)
        {
            e.Handled = IsBusy;
        }

        /// <summary>
        /// Indicator of work on background
        /// </summary>
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }
    }
}
