using System;
using System.Windows.Forms;

namespace DownloadHelper.Sample
{
    public partial class DownloadForm : Form
    {
        public DownloadForm()
        {
            InitializeComponent();
        }

        public DownloadForm(StreamDownload download) : this()
        {
            Download = download;
            Download.ProgressChanged += DownloadOnProgressChanged;
        }

        private void DownloadOnProgressChanged(object sender, EventArgs eventArgs)
        {
            lbl_url.Text = Download.Request.Url.ToString();
            lbl_progress.Text = string.Format(@"{0} / {1} {2}", Download.TotalWritten.FormatAsSize(),
                Download.Partition.TotalSize.FormatAsSize(), Download.Progress.FormatAsPercentage());
            lbl_speed.Text = string.Format(@"{0} ({1})", Download.AverageSpeed.FormatAsSpeed(),
                Download.Speed.FormatAsSpeed());
            lbl_status.Text = Download.Status.ToString();
            lv_connections.Items.Clear();
            var i = 0;
            foreach (var connection in Download.Connections)
            {
                i++;
                string[] row =
                {
                    i.ToString(),
                    connection.Message,
                    connection.Speed.FormatAsSpeed(),
                    connection.Progress.FormatAsPercentage(),
                    connection.TotalDownloaded.FormatAsSize()
                };
                var listViewItem = new ListViewItem(row);
                lv_connections.Items.Add(listViewItem);
            }
        }

        public StreamDownload Download { get; set; }

        private async void btn_action_Click(object sender, EventArgs e)
        {
            btn_action.Enabled = false;
            await Download.Start();
            Download.Partition.Stream.Flush();
            Download.Partition.Stream.Close();
            btn_action.Enabled = true;
        }
    }
}