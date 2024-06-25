using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenshotApp
{
    public partial class Form1 : Form
    {
        private UdpClient _client;
        private static readonly int MaxUdpPacketSize = 8192;
        public Form1()
        {
            InitializeComponent();
            _client = new UdpClient();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            SendReq();
        }

        private void SendReq()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 51111);
            try
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes("screen");
                _client.Send(requestBytes, requestBytes.Length, serverEndPoint);

                
                IPEndPoint remoteEndPoint = null;
                byte[] sizeBytes = _client.Receive(ref remoteEndPoint);
                int screenshotSize = BitConverter.ToInt32(sizeBytes, 0);

                // Получение самого скриншота по частям
                byte[] screenshotBytes = new byte[screenshotSize];
                int receivedBytes = 0;

                while (receivedBytes < screenshotSize)
                {
                    byte[] chunk = _client.Receive(ref remoteEndPoint);
                    Array.Copy(chunk, 0, screenshotBytes, receivedBytes, chunk.Length);
                    receivedBytes += chunk.Length;
                }

                using (MemoryStream ms = new MemoryStream(screenshotBytes))
                {
                    Image screenshot = Image.FromStream(ms);
                    pictureBox1.Image = screenshot;
                }

                Invoke(new Action(() => { button1.Enabled = true; }));
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
