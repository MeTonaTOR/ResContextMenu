using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResContextMenu {
    internal static class Program {

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder result, int size, string filePath);

        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            NotifyIcon trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Use a default icon
                Visible = true
            };


            int modeNum = 0;
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            String ignoreme = "";

            int width = 0;
            int height = 0;

            while (EnumDisplaySettings(null, modeNum, ref devMode)) {
                if (ignoreme != $"{devMode.dmPelsWidth}x{devMode.dmPelsHeight}") {

                    width = devMode.dmPelsWidth;
                    height = devMode.dmPelsHeight;

                    contextMenu.Items.Add($"{width}x{height}", null, (O, K) => {
                        String[] textFromMenuItem = O.GetType().GetProperty("Text").GetValue(O).ToString().Split('x');

                        width = int.Parse(textFromMenuItem[0]);
                        height = int.Parse(textFromMenuItem[1]);

                        ChangeResolution(width, height);
                    });

                    ignoreme = $"{devMode.dmPelsWidth}x{devMode.dmPelsHeight}";
                }
                modeNum++;
            }

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = contextMenu;

            Application.Run();
        }

        public static void ChangeResolution(int width, int height) {

            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new string(new char[32]);
            dm.dmFormName = new string(new char[32]);
            dm.dmSize = (short)Marshal.SizeOf(dm);

            if (EnumDisplaySettings(null, -1, ref dm))
            {
                dm.dmPelsWidth = width;
                dm.dmPelsHeight = height;

                int result = ChangeDisplaySettings(ref dm, 0x02);

                if (result != 0)
                {
                    Console.WriteLine("Unable to process your request. The resolution is not supported.");
                }
                else
                {
                    result = ChangeDisplaySettings(ref dm, 0x01);

                    switch (result)
                    {
                        case 0:
                            Console.WriteLine("Resolution changed successfully!");
                            break;
                        default:
                            Console.WriteLine("Failed to change the resolution.");
                            break;
                    }
                }
            }
        }
    }
}
