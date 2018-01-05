using System;
using System.Globalization;
using System.Windows.Forms;

namespace DownloadHelper.Sample
{
    public partial class MainForm : Form
    {
        private Timer t = null;

        public MainForm()
        {
            InitializeComponent();
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            //using (
            //    var file = new FileStream("D:\\Downloads\\TEST\\xvideos.com_8ab2a5644d58f827c94bbd70e86bb534.mp4.tmp", FileMode.OpenOrCreate,
            //        FileAccess.ReadWrite))
            //{
            //var limiter = new DownloadSpeedLimiter(30*1024);
            //var connection =
            //    new Download(
            //        new DownloadRequest(
            //            new Uri(
            //                "http://ipv4.download.thinkbroadband.com/1GB.zip")),
            //        new DirectoryInfo("D:\\Downloads\\TEST"), null, new DirectoryInfo("D:\\Downloads\\TEST"));
            //connection.SpeedLimiter = limiter;
            //connection.StatusChanged += (o, args) => lbl_status.Text = connection.Status.ToString();
            //connection.MessageChanged += (o, args) => lbl_message.Text = connection.Message;
            //connection.ProgressChanged += (o, args) =>
            //{
            //    lbl_percentage.Text = connection.Progress.ToString("F") + "%";
            //    lbl_bytes.Text = FormatSizeString(connection.TotalDownloaded) + " (" + FormatSizeString(connection.TotalWritten) + ") / " +
            //                     FormatSizeString(connection.TotalSize);
            //    lbl_speed.Text = FormatSpeedString(connection.AverageSpeed) + " (" + FormatSpeedString(connection.Speed) + ")";
            //};
            //t?.Stop();
            //t?.Dispose();
            //t = new Timer {Interval = 500};
            //t.Tick += (o, args) =>
            //{
            //    lbl_status.Text = connection.Status.ToString();
            //    lbl_percentage.Text = connection.Progress.ToString("F") + "%";
            //    lbl_bytes.Text = FormatSizeString(connection.TotalDownloaded) + " (" +
            //                     FormatSizeString(connection.TotalWritten) + ") / " +
            //                     FormatSizeString(connection.TotalSize);
            //    lbl_speed.Text = FormatSpeedString(connection.AverageSpeed) + " (" + FormatSpeedString(connection.Speed) +
            //                     ")";
            //};
            //t.Start();
            //try
            //{
            //    await connection.Start();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
            //}
        }
    }
}