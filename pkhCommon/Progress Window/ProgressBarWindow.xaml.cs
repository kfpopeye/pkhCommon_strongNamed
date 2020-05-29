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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace pkhCommon.WPF
{
    //NOTE: CANNOT LOCALIZE THIS WINDOW USING LEX. RUNS ON SEPARATE THREAD

    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public bool IsCanceled { get; set; }

        public ProgressBarWindow()
        {
            InitializeComponent();
            IsCanceled = false;
        }

        /// <summary>
        /// Creates an indeterminate progress bar (no button)
        /// </summary>
        /// <param name="Message"></param>
        public ProgressBarWindow(string Message)
        {
            InitializeComponent();
            IsCanceled = false;
            this._bar.IsIndeterminate = true;
            this._message.Text = Message;
            this.CancelButton.Visibility = Visibility.Collapsed;
        }

        public void UpdateProgress(string comment)
        {
            this.Dispatcher.Invoke(new Action<string>(

            delegate (string s)
            {
                this._message.Text = s;
            }),
            System.Windows.Threading.DispatcherPriority.Background, comment);
        }

        public void UpdateProgress(string comment, int current, int total)
        {
            this.Dispatcher.Invoke(new Action<string, int, int>(

            delegate(string s, int v, int t)
            {
                this._message.Text = s;
                this._bar.Maximum = System.Convert.ToDouble(t);
                this._bar.Value = System.Convert.ToDouble(v);
            }),
            System.Windows.Threading.DispatcherPriority.Background, comment, current, total);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Title = Title + " - DEBUG BUILD";
#endif
        }
    }
}