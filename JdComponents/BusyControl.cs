using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JdComponents
{
    /// <summary>
    /// Control for indication of work on background
    /// </summary>
    public class BusyControl : ContentControl
    {
        public static readonly DependencyProperty IsBusyProperty;
        public static readonly DependencyProperty IndicatorBackgroundProperty;
        public static readonly DependencyProperty IndicatorTemplateProperty;

        /// <summary>
        /// Static constructor for <see cref="DependencyProperty"/> initialization
        /// </summary>
        static BusyControl()
        {
            var owner = typeof(BusyControl);
            IsBusyProperty = DependencyProperty.Register(nameof(IsBusy), typeof(bool), owner);
            IndicatorBackgroundProperty = DependencyProperty.Register(nameof(IndicatorBackground), typeof(Brush), owner);
            IndicatorTemplateProperty = DependencyProperty.Register(nameof(IndicatorTemplate), typeof(DataTemplate), owner);
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
        
        /// <summary>
        /// Data template of indicator
        /// </summary>
        public DataTemplate IndicatorTemplate
        {
            get => (DataTemplate)GetValue(IndicatorTemplateProperty);
            set => SetValue(IndicatorTemplateProperty, value);
        }

        /// <summary>
        /// Background of busy indicator
        /// </summary>
        public Brush IndicatorBackground
        {
            get => (Brush)GetValue(IndicatorBackgroundProperty);
            set => SetValue(IndicatorBackgroundProperty, value);
        }

        /// <summary>
        /// Indicator of work on background
        /// </summary>
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        /// <summary>
        /// Handles preview events when busy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleIfBusy(object sender, RoutedEventArgs e)
        {
            e.Handled = IsBusy;
        }
    }
}
