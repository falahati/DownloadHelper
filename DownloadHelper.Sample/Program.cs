using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownloadHelper.Sample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            var request = new DownloadRequest(new Uri("http://ipv4.download.thinkbroadband.com/10MB.zip"));
            File.Delete("D:\\Downloads\\TEST\\10MB.zip.tmp");
            var stream = File.OpenWrite("D:\\Downloads\\TEST\\10MB.zip.tmp");
            var partitionManager = new StreamPartition(stream);
            var streamDownload = new StreamDownload(request, partitionManager);
            var form = new DownloadForm(streamDownload);
            Application.Run(form);
        }
    }
}
