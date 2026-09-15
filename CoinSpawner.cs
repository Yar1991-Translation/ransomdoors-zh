using WpfApplication = System.Windows.Application;

namespace rans0m
{
    /// <summary>
    /// Puts the coins on screen during a ransom, and refills the ones the player picks up
    /// </summary>
    public static class CoinSpawner
    {
        /// <summary>How many coins are on screen at the same time</summary>
        public const int MaxVisibleCoins = 12;

        /// <summary>Gap kept between coins (and around the ransom window) so they stay easy to click</summary>
        private const double Spacing = 24;

        private static readonly List<SpawnedCoin> _pending = new();
        private static readonly List<CoinWindow> _active = new();
        private static bool _running;

        /// <summary>
        /// Starts spawning the given coins, replacing anything already on screen
        /// </summary>
        public static void Start(IEnumerable<SpawnedCoin> coins)
        {
            Stop();

            if (coins != null)
                _pending.AddRange(coins);

            _running = true;
            Fill();
        }

        /// <summary>
        /// Removes every coin from the screen
        /// </summary>
        public static void Stop()
        {
            _running = false;
            _pending.Clear();

            foreach (CoinWindow coin in _active.ToList())
            {
                try { coin.CloseForCleanup(); }
                catch { }
            }

            _active.Clear();
        }

        /// <summary>
        /// Called by a coin window once the player picked it up
        /// </summary>
        public static void OnCollected(CoinWindow coin)
        {
            _active.Remove(coin);

            if (_running)
                Fill();
        }

        private static void Fill()
        {
            while (_running && _active.Count < MaxVisibleCoins && _pending.Count > 0)
            {
                SpawnedCoin coin = _pending[0];
                _pending.RemoveAt(0);

                try
                {
                    CoinWindow window = new CoinWindow(coin);
                    (window.Left, window.Top) = FindFreeSpot();

                    _active.Add(window);
                    window.Show();
                }
                catch { }
            }
        }

        /// <summary>
        /// Looks for a spot where the coin doesn't sit on another coin or on the ransom window
        /// </summary>
        private static (double left, double top) FindFreeSpot()
        {
            System.Drawing.Rectangle bounds = Global.screenBounds;
            double maxX = Math.Max(1, bounds.Width - CoinWindow.WindowSize);
            double maxY = Math.Max(1, bounds.Height - CoinWindow.WindowSize);

            for (int attempt = 0; attempt < 60; attempt++)
            {
                double x = Global.rng.NextDouble() * maxX;
                double y = Global.rng.NextDouble() * maxY;

                if (OverlapsRansomWindow(x, y) || OverlapsActiveCoin(x, y))
                    continue;

                return (bounds.Left + x, bounds.Top + y);
            }

            // Screen is packed, drop it somewhere random rather than not spawning it at all
            return (bounds.Left + Global.rng.NextDouble() * maxX, bounds.Top + Global.rng.NextDouble() * maxY);
        }

        private static bool OverlapsActiveCoin(double x, double y)
        {
            foreach (CoinWindow coin in _active)
            {
                if (Overlaps(x, y, coin.Left, coin.Top))
                    return true;
            }

            return false;
        }

        private static bool OverlapsRansomWindow(double x, double y)
        {
            RansomNotification? ransomWindow = WpfApplication.Current?.Windows.OfType<RansomNotification>().FirstOrDefault();
            if (ransomWindow == null)
                return false;

            double width = ransomWindow.ActualWidth > 0 ? ransomWindow.ActualWidth : ransomWindow.Width;
            double height = ransomWindow.ActualHeight > 0 ? ransomWindow.ActualHeight : ransomWindow.Height;

            return Overlaps(x, y, ransomWindow.Left, ransomWindow.Top, width, height);
        }

        /// <summary>
        /// Does the coin at (x,y) overlap the given rectangle (keeping Spacing between them)?
        /// </summary>
        private static bool Overlaps(double x, double y, double otherLeft, double otherTop,
            double otherWidth = CoinWindow.WindowSize, double otherHeight = CoinWindow.WindowSize)
        {
            return x - Spacing < otherLeft + otherWidth
                && x + CoinWindow.WindowSize + Spacing > otherLeft
                && y - Spacing < otherTop + otherHeight
                && y + CoinWindow.WindowSize + Spacing > otherTop;
        }
    }
}
