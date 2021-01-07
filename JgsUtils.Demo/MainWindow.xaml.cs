using JdUtils;
using JdUtils.Extensions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using cnv = JdUtils.Converters;
namespace JgsUtils.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private Button m_btn;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += AfterLoaded;
            var t1 = E1.Collapsed;
            var cnv = new cnv.EnumConverter();
            var t2 = cnv.Convert(t1, typeof(Visibility), null, null);
            DataContext = this;
        }

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_isRunning;
        public bool IsRunning
        {
            get => m_isRunning;
            set => SetIsRunning(value);
        }

        private void SetIsRunning(bool value)
        {
            SetProperty(ref m_isRunning, value, nameof(IsRunning));
            if (value)
            {
                StartOnBackground();
            }
        }

        private void StartOnBackground()
        {
            Case01();
            Case02();
        }

        private void Case01()
        {
            void Work()
            {
                IsRunning = false;
            }

            new BackgroundWorkerBuilder(Dispatcher)
                .Do(Work)
                .AfterDelay(5000)
                .Execute();
        }

        private void Case02()
        {
            void Work()
            {
            }

            void Success()
            {
                IsRunning = false;
            }

            new BackgroundWorkerBuilder(Dispatcher)
                .Do(Work)
                .OnSuccess(Success)
                .WithDelay(5000)
                .Execute();
        }

        /// <summary>
        /// Sets property and if the new value does not match the old one
        /// raises <see cref="PropertyChanged"/>
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="field">Backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of property</param>
        protected virtual void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>
        /// </summary>
        /// <param name="propertyName">Name of property changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
