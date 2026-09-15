using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WpfApplication = System.Windows.Application;

namespace rans0m
{
    public class Global
    {
        public static Overlay? overlayWindow;
        // Titles used by the pop up windows
        public static readonly List<string> tauntTitles = new() {
            "RANS0M",
            "MOSNAR",
            "RANSOM",
            "M0NARS",
            "你是个白痴",
            "未命名",
            "未命名 (3)",
            "找到你了",
            "RANSOM.exe",
            "RAANNNSSSSOOOOOMMMMMM",
            "时间到了",
            "把钱交出来",
            "错误",
            "DHAUFGH",
            "_________",
            "IMG.JPG"
        };

        // Images used by the pop up windows
        private static List<BitmapImage> _tauntImages;
        public static List<BitmapImage> tauntImages
        {
            get
            {
                if (_tauntImages == null)
                {
                    _tauntImages = new()
                    {
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch1.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch2.jpeg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch3.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch4.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/glitch5.jpg"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/idiot.png"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/tauntface.png"),
                        LoadBitmapImage("pack://application:,,,/Assets/Taunts/tauntflower.png"),
                    };
                }
                return _tauntImages;
            }
        }





        // ----------------- GLOBAL VARIABLES -----------------

        public static int ransomLeft = 0; // Cash to pay
        public static int ransomTimeLeft = 0; // 3rd phase countdown
        public static bool underRansom = false;
        public static Action? RansomPayed;
        public static List<string> usedCoins = new(); // this is to avoid people from copy pasting coins, not that secure tho

        public static bool crucifixUsed = false;
        public static bool canAttack = true;
        public static System.Drawing.Point lastRegisteredMousePos;
        public static bool spyingMouse = false;

        public static Random rng = new Random();
        public static System.Drawing.Rectangle screenBounds => Screen.PrimaryScreen.WorkingArea;







        // ---------------------- PUBLIC METHODS ----------------------

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        private static BitmapImage LoadBitmapImage(string uri)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        static public byte[] GetBytesFromResource(string uri)
        {
            System.Windows.Resources.StreamResourceInfo streamInfo = WpfApplication.GetResourceStream(new Uri($"pack://application:,,,/Assets/{uri}"));
            byte[] bytes = new byte[streamInfo.Stream.Length];
            streamInfo.Stream.Read(bytes, 0, (int)streamInfo.Stream.Length);

            return bytes;
        }

        static public Stream GetResourceSteam(string uri)
        {
            return WpfApplication.GetResourceStream(new Uri($"pack://application:,,,/Assets/{uri}")).Stream;
        }

        static public Thickness CombineThickness(Thickness a, Thickness b)
        {
            return new Thickness(a.Left+b.Left, a.Top+b.Top, a.Right+b.Right, a.Bottom+b.Bottom);
        }

        public static void KeyPressed(Keys key)
        {
            if (spyingMouse)
            {
                lastRegisteredMousePos = new System.Drawing.Point(-1, -1); // Invalidate the last registered mouse position if a key is pressed during the spy phase so it also triggers the ransom
            }
        }

        /// <summary>
        /// Randomly positions a control within the screen bounds.
        /// </summary>
        public static void RandomPosWindow(Window window)
        {
            int x = Global.rng.Next(0, (int)(Global.screenBounds.Width - window.ActualWidth));
            int y = Global.rng.Next(0, (int)(Global.screenBounds.Height - window.ActualHeight));

            window.Left = x;
            window.Top = y;
        }

        /// <summary>
        /// Randomly positions a control within the screen bounds.
        /// </summary>
        public static void RandomPosControl(FrameworkElement element)
        {
            int x = Global.rng.Next(0, (int)(Global.screenBounds.Width - element.ActualWidth));
            int y = Global.rng.Next(0, (int)(Global.screenBounds.Height - element.ActualHeight));

            element.Margin = new Thickness(x, y, 0, 0);
        }

        /// <summary>
        /// Centers a control within the screen bounds.
        /// </summary>
        public static void CenterControl(FrameworkElement element)
        {
            element.Margin = new Thickness((Global.screenBounds.Width / 2) - element.ActualWidth / 2, (Global.screenBounds.Height / 2) - element.ActualHeight / 2, 0, 0);
        }

        /// <summary>
        /// Centers a control within the screen bounds.
        /// </summary>
        public static void CenterWindow(Window window)
        {
            window.Left = (Global.screenBounds.Width / 2) - window.ActualWidth / 2;
            window.Top = (Global.screenBounds.Height / 2) - window.ActualHeight / 2;
        }

        /// <summary>
        /// Cool glitch idle animation, used for the ransom pop ups
        /// </summary>
        public static async void GlitchIdle(Window control, bool divideAndTaunt = false)
        {
            (double x, double y) = await control.Dispatcher.InvokeAsync(() =>
                (control.Left, control.Top));

            while (true)
            {
                await Task.Delay(200);
                try
                {
                    if (!Global.underRansom)
                    {
                        await control.Dispatcher.InvokeAsync(() =>
                        {
                            control.Close();
                        });
                        break;
                    }

                    await control.Dispatcher.InvokeAsync(() =>
                    {
                        if (divideAndTaunt)
                        {
                            if (Global.rng.Next(1, 100) <= 2)
                            {
                                x = Global.rng.Next(0, (int)(Global.screenBounds.Width - control.ActualWidth));
                                y = Global.rng.Next(0, (int)(Global.screenBounds.Height - control.ActualHeight));
                                TauntWindow tauntWindow = new TauntWindow();
                                tauntWindow.Show();
                            }
                        }

                        control.Left = x + Global.rng.Next(-5, 5);
                        control.Top = y + Global.rng.Next(-5, 5);
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        public static void HideSystemMenu(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & ~WS_SYSMENU);
        }
    }
}
