using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Chromotherapy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CHANNEL_MAX_VALUE = 255;
        private const int CHANNEL_MIN_VALUE = 0;

        public Channel R { get; set; }
        public Channel G { get; set; }
        public Channel B { get; set; }

        private bool IsContinue { get; set; }
        private Queue<Channel> Channels { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp += this.MainWindow_KeyUp;
            this.InitializeChromotherapy();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.IsContinue = false;
            }
        }

        private void InitializeChromotherapy()
        {
            this.StartChangingColors();
        }

        private void StartChangingColors()
        {
            this.btnStart.Visibility = Visibility.Collapsed;

            this.R = new Channel(0);
            this.G = new Channel(0);
            this.B = new Channel(0);

            this.IsContinue = true;

            this.Channels = new Queue<Channel>();

            this.Channels.Enqueue(this.R);
            this.Channels.Enqueue(this.G);
            this.Channels.Enqueue(this.R);
            this.Channels.Enqueue(this.B);
            this.Channels.Enqueue(this.G);
            this.Channels.Enqueue(this.R);
            this.Channels.Enqueue(this.B);
            this.Channels.Enqueue(this.R);

            this.ChangeColorAsync(this.Channels.Dequeue());
        }

        private Task ChangeColorAsync(Channel channel)
        {
            return Task.Factory.StartNew(() =>
            {
                if (this.IsContinue)
                {
                    this.ChangeColor(channel);
                }
                else
                {
                    this.StopChromotherapy();
                }
            });
        }

        private void StopChromotherapy()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.gdRoot.Background = Brushes.Black;
                this.btnStart.Visibility = Visibility.Visible;
            });
        }

        private void ChangeColor(Channel channel)
        {
            channel.IsIncreasing = channel.Value == 0;

            while ((channel.IsIncreasing && channel.Value < CHANNEL_MAX_VALUE) || (!channel.IsIncreasing && channel.Value > CHANNEL_MIN_VALUE))
            {
                if (channel.IsIncreasing && channel.Value < CHANNEL_MAX_VALUE)
                {
                    channel.Value++;
                }

                if (!channel.IsIncreasing && channel.Value > CHANNEL_MIN_VALUE)
                {
                    channel.Value--;
                }

                this.UpdateBackground(this.R, this.G, this.B);

                Thread.Sleep(50);

                if (!this.IsContinue)
                {
                    this.StopChromotherapy();
                    break;
                }
            }

            if (this.Channels.Count > 0)
            {
                this.ChangeColorAsync(this.Channels.Dequeue());
            }
            else
            {
                this.StartChangingColors();
            }
        }

        private async void UpdateBackground(Channel r, Channel g, Channel b)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() => this.gdRoot.Background = new SolidColorBrush(Color.FromRgb(r.Value, g.Value, b.Value)));
            }
            catch
            {

            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            this.StartChangingColors();
        }
    }

    public enum RgbChannel
    {
        R,
        G,
        B
    }

    public class Channel
    {
        public byte Value { get; set; }
        public bool IsIncreasing { get; set; }

        public Channel(byte value)
        {
            this.Value = value;
            this.IsIncreasing = value == 0;
        }
    }
}
