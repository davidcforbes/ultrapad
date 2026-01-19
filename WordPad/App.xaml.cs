using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Storage;
using WordPad.Classes;

namespace RectifyPad
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

        public static FontClass FClass { get; private set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        public Window MainWindow => m_window;

        public static ElementTheme RootTheme
        {
            get
            {
                if ((App.Current as App)?.m_window?.Content is FrameworkElement rootElement)
                {
                    return rootElement.RequestedTheme;
                }
                return ElementTheme.Default;
            }
            set
            {
                if ((App.Current as App)?.m_window?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }




        #region Error handling
        private static void OnUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Log the exception before marking it as observed
            System.Diagnostics.Debug.WriteLine($"Unobserved Task Exception: {e.Exception}");
            System.IO.File.AppendAllText("crash.log", $"{DateTime.Now}: Unobserved Task Exception: {e.Exception}\n");
            e.SetObserved();
        }

        private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Log the exception before marking it as handled
            System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {e.Exception}");
            System.IO.File.AppendAllText("crash.log", $"{DateTime.Now}: Unhandled Exception: {e.Exception}\n");
            // Don't handle it - let it crash with visible error
            e.Handled = false;
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            // Log first chance exceptions for debugging
            System.Diagnostics.Debug.WriteLine($"First Chance Exception: {e.Exception}");
        }
        #endregion
    }
}