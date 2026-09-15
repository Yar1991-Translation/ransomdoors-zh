using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApplication = System.Windows.Application;

namespace rans0m
{
    /// <summary>
    /// A single coin floating on screen during a ransom. Click it to pay it in.
    /// </summary>
    public partial class CoinWindow : Window
    {
        /// <summary>Size of the window, also used to keep the coins apart from each other</summary>
        public const double WindowSize = 96;

        private readonly SpawnedCoin _coin;
        private bool _pickedUp;

        public CoinWindow(SpawnedCoin coin)
        {
            InitializeComponent();

            _coin = coin;

            LoadCoinImage(coin);

            if (coin.IsCrucifix)
                valueBadge.Visibility = Visibility.Collapsed;
            else
                txt_value.Text = coin.Value.ToString();

            coinRotation.Angle = Global.rng.Next(-18, 19);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PickUp();
        }

        /// <summary>
        /// Pays the coin into the ransom, then disappears
        /// </summary>
        public void PickUp()
        {
            if (_pickedUp) return;
            _pickedUp = true;

            CollectResult result = Global.CollectFile(_coin.Path);

            CoinSpawner.OnCollected(this);
            Close();

            if (result != CollectResult.None && Global.ransomLeft <= 0)
                Global.CompleteRansom();
        }

        /// <summary>
        /// Removes the coin because the ransom ended, without paying it in
        /// </summary>
        public void CloseForCleanup()
        {
            _pickedUp = true;
            Close();
        }

        /// <summary>
        /// Shows the same icon the coin file gets, falling back to the gold png from the xaml
        /// </summary>
        private void LoadCoinImage(SpawnedCoin coin)
        {
            string iconUri = coin.Type switch
            {
                0 => "crucifix.ico",
                6 => "Gold/HoneyPot.ico",
                _ => $"Gold/Gold{coin.Type}.ico"
            };

            try
            {
                using Stream stream = WpfApplication.GetResourceStream(new Uri($"pack://application:,,,/Assets/{iconUri}")).Stream;

                IconBitmapDecoder decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                img_coin.Source = decoder.Frames.OrderByDescending(frame => frame.PixelWidth).First();
            }
            catch { } // The icon files are picked per size, but stay on the png fallback if decoding ever fails
        }
    }
}
