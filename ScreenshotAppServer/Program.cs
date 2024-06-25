using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ScreenshotAppServer
{
    internal class Program
    {
        private static UdpClient _server;
        private static readonly int MaxUdpPacketSize = 8192;
        static void Main(string[] args)
        {
            _server = new UdpClient(51111);

            while (true)
            {
                try
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = _server.Receive(ref clientEndPoint);
                    string request = System.Text.Encoding.UTF8.GetString(buffer);

                    if (request == "screen")
                    {

                        Bitmap screenshot = CaptureScreenshot();
                        byte[] screenshotBytes;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            screenshot.Save(ms, ImageFormat.Png);
                            screenshotBytes = ms.ToArray();
                        }

                        byte[] sizeBytes = BitConverter.GetBytes(screenshotBytes.Length);
                        _server.Send(sizeBytes, sizeBytes.Length, clientEndPoint);

                        int offset = 0;
                        while (offset < screenshotBytes.Length)
                        {
                            int chunkSize = Math.Min(MaxUdpPacketSize, screenshotBytes.Length - offset);
                            byte[] chunk = new byte[chunkSize];
                            Array.Copy(screenshotBytes, offset, chunk, 0, chunkSize);
                            _server.Send(chunk, chunk.Length, clientEndPoint);
                            offset += chunkSize;
                        }

                        Console.WriteLine("Screenshot sent.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static Bitmap CaptureScreenshot()
        {
            Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }

            return screenshot;
        }
    }
}
