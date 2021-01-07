using JdUtils.BackgroundWorker;
using JdUtils.BackgroundWorker.Interfaces;
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
            m_index = 0;
            m_delay = 3000;
            m_executor = BackgroundExecutor.Instance(Dispatcher);
            DataContext = this;
        }

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_isRunning;
        private bool m_isProcessing;

        public bool IsRunning
        {
            get => m_isRunning;
            set => SetIsRunning(value);
        }
        private string m_backgroundTest;
        private int m_index;
        private readonly int m_delay;
        private readonly IBackgroundExecutorInstance m_executor;

        public string BackgroundTest
        {
            get => m_backgroundTest;
            set => SetProperty(ref m_backgroundTest, value);
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
            if(!m_isProcessing)
            {
                m_isProcessing = true;
                switch (m_index)
                {
                    case 0:
                        Case01();
                        break;
                    case 1:
                        Case02();
                        break;
                    case 2:
                        Case03();
                        break;
                    case 3:
                        Case04();
                        break;
                    case 4:
                        Case05();
                        m_index = -1;
                        break;
                    default:
                        break;
                }
                m_index++;
                m_isProcessing = false;
            }
        }

        private void Prepare([CallerMemberName]string name = null)
        {
            IsRunning = true;
            BackgroundTest = name;
        }

        private void Case01()
        {
            void Work()
            {
                IsRunning = false;
            }

            Prepare();
            BackgroundExecutor
                .Instance(Dispatcher)
                .Do(Work)
                .AfterDelay(m_delay)
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

            Prepare();
            m_executor
                .Do(Work)
                .OnSuccess(Success)
                .WithDelay(m_delay)
                .Execute();
        }

        private void Case03()
        {
            var number = 2;
            void Work(int param)
            {
                number *= param;
            }

            void Success()
            {
                BackgroundTest += $" {number}";
                IsRunning = false;
            }

            Prepare();
            m_executor
                .Do(Work, 3)
                .OnSuccess(Success)
                .WithDelay(m_delay)
                .Execute();
        }

        private void Case04()
        {
            var number = 3;
            int Work(int param)
            {
                return number * param;
            }

            void Success(int result)
            {
                BackgroundTest += $" {result}";
                IsRunning = false;
            }

            Prepare();
            m_executor
                .Do(Work, 3)
                .OnSuccess(Success)
                .WithDelay(m_delay)
                .Execute();
        }

        private void Case05()
        {
            void Work(int param)
            {
                BackgroundTest += $" {param}";
                IsRunning = false;
            }

            Prepare();
            m_executor
                .Do(Work,6)
                .AfterDelay(m_delay)
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
