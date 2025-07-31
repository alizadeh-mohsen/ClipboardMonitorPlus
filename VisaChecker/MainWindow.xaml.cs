using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace VisaChecker
{

    public partial class MainWindow : Window
    {
        private DispatcherTimer _clipboardMonitorTime;
        private const string ClipBoardBusy = "Clipboard is busy, please try again.";
        private readonly AppDbContext _context;
        private CollectionViewSource categoryViewSource;
        private const int MaxClipboardTextLength = 15;
        private bool monitoring;

        public MainWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            categoryViewSource =
                (CollectionViewSource)FindResource(nameof(categoryViewSource));
            inputTextBox.Focus();
            _clipboardMonitorTime = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            monitoring = true;
            status.Text = "Monitoring...";
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Title += $" {version}";
            _clipboardMonitorTime.Tick += ClipboardMonitorTime_Tick;
            _clipboardMonitorTime.Start();
        }

        private void LoadData()
        {
            try
            {
                _context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var data = _context.Gov.Where(c => c.Name.Contains(inputTextBox.Text)).OrderBy(x => x.Name).ToList();

                categoryDataGrid.ItemsSource = data;
                itemCountVisa.Text = data.Count().ToString();
                Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void ClipboardMonitorTime_Tick(object? sender, EventArgs e)
        {
            try
            {
                var clipboardText = Clipboard.GetText().Trim();

                if (inputTextBox.Text == clipboardText)
                    return;

                if (clipboardText.Length > MaxClipboardTextLength)
                    clipboardText = clipboardText.Substring(0, MaxClipboardTextLength - 1);

                inputTextBox.Text = clipboardText;
                if (Clipboard.ContainsText())
                {
                    var currentText = Clipboard.GetText();

                    if (!listBox.Items.Contains(currentText)
                        && !string.IsNullOrWhiteSpace(currentText))
                    {
                        listBox.Items.Add(currentText);
                        itemCountClipBoard.Text = listBox.Items.Count.ToString();
                        Activate();
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                itemCountVisa.Text = ClipBoardBusy;
            }
            catch (Exception ex)
            {
                itemCountVisa.Text = ex.Message;
            }
        }


        private void inputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (inputTextBox.Text.Length == 0)
            {
                categoryDataGrid.ItemsSource = null;
                itemCountVisa.Text = "0";
                return;
            }

            LoadData();
        }



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            monitoring = !monitoring;
            if (monitoring)
            {
                startButton.Content = "Stop Monitoring"; 
                _clipboardMonitorTime.Tick += ClipboardMonitorTime_Tick;
                _clipboardMonitorTime.Start();
                status.Text = "Monitoring...";
            }
            else
            {
                startButton.Content = "Start Monitoring";
                _clipboardMonitorTime.Tick -= ClipboardMonitorTime_Tick;
                _clipboardMonitorTime.Stop();
                status.Text = "";
            }
        }


        #region ClipBoard monitor

        private void listBox_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (listBox.SelectedItem != null)
                {
                    if (Clipboard.GetText().Equals(listBox.SelectedItem.ToString()))
                        Clipboard.Clear();
                    listBox.Items.Remove(listBox.SelectedItem);
                    itemCountClipBoard.Text = listBox.Items.Count.ToString();
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                itemCountClipBoard.Text = ClipBoardBusy;
            }
            catch (Exception ex)
            {
                itemCountClipBoard.Text = ex.Message;
            }

        }

        private void listBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (listBox.SelectedItem != null)
                    Clipboard.SetText(listBox.SelectedItem.ToString());
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                itemCountClipBoard.Text = ClipBoardBusy;
            }
            catch (Exception ex)
            {
                itemCountClipBoard.Text = ex.Message;
            }
        }

        private void ClearListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();
            itemCountClipBoard.Text = "0";
            Clipboard.Clear();
        }

        #endregion
    }
}