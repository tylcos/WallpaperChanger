using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Background_Changer_Mk1
{
    class Program
    {
        private static readonly ImageFormat Format = ImageFormat.Bmp;
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        
        static void Main(string[] args)
        {
           if       (args.Length == 1)
               SetFile(args[0]);
            else if (args.Length == 2 && args[1] == "/r")
               SetRegistry(args[0]);
        }
        
        private static void SetRegistry (string path)
        {
            RegistryPermission perm = new RegistryPermission(RegistryPermissionAccess.AllAccess, @"Control Panel\Desktop");
            perm.Demand();

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            key.SetValue("WallpaperStyle", "2");
            key.SetValue("TileWallpaper", "0");

            SystemParametersInfo(20, 0, path, 0x01 | 0x02);
        }

        private static void SetFile(string path)
        {
            string wallpaperPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                   + @"\Microsoft\Windows\Themes\transcodedWallpaper";

            using (Image srcImage = Image.FromFile(path))
            {
                if (srcImage.Height != 1080 || srcImage.Width != 1920)
                {
                    int newHeight = 1080, newWidth = 1920;
                    
                    using (var newImage = new Bitmap(newWidth, newHeight))
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
                        newImage.Save(wallpaperPath, Format);
                    }
                }
                else
                    srcImage.Save(wallpaperPath, Format);
            }

            foreach (Process process in Process.GetProcessesByName("explorer"))
                process.Kill();
                        
            //Process.Start("explorer.exe");
        }
    }
}
