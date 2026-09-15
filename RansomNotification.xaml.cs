using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace rans0m
{
    public partial class RansomNotification : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly List<SpawnedCoin> _coins;

        public RansomNotification(List<SpawnedCoin> coins)
        {
            InitializeComponent();

            _coins = coins;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Closed += (_, _) => _timer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HideSystemMenu(this);

            // Init the window
            txt_timer.Text = $"剩余时间 {Global.ransomTimeLeft / 60:D2}:{Global.ransomTimeLeft % 60:D2}";
            Global.RandomPosWindow(this);

            // Init 9 taunt windows
            for (int i = 0; i < 9; i++)
            {
                TauntWindow tauntWindow = new TauntWindow();
                tauntWindow.Show();
            }

            // Puts the collectible coins on screen
            CoinSpawner.Start(_coins);

            Global.GlitchIdle(this, true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txt_ransomLeft.Text = Global.ransomLeft.ToString();
            txt_timer.Text = $"剩余时间 {Global.ransomTimeLeft / 60:D2}:{Global.ransomTimeLeft % 60:D2}";
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // TODO : Check if file is a valid .gold file
                e.Effects = DragDropEffects.Link;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;

            bool sfxPlayed = false; // Prevent multiple sfx to play when user drags multiple coins
            foreach (string file in files) // technically it's easily cheat but who cares
            {
                if (Global.CollectFile(file, playSfx: !sfxPlayed) == CollectResult.Gold)
                    sfxPlayed = true; // The sfx already played for this one
            }

            if (Global.ransomLeft <= 0)
            {
                Global.CompleteRansom();
            }
        }
    }
}
