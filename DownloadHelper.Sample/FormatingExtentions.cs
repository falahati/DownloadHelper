using System;
using System.Globalization;

namespace DownloadHelper.Sample
{
    public static class FormatingExtentions
    {
        public static string FormatAsSize(this long byteSize)
        {
            var NumberFormat = new NumberFormatInfo();
            var kiloByteSize = byteSize/1024D;
            var megaByteSize = kiloByteSize/1024D;
            var gigaByteSize = megaByteSize/1024D;

            if (byteSize < 1024)
                return string.Format(NumberFormat, "{0} B", byteSize);
            if (byteSize < 1048576)
                return string.Format(NumberFormat, "{0:0.00} kB", kiloByteSize);
            if (byteSize < 1073741824)
                return string.Format(NumberFormat, "{0:0.00} MB", megaByteSize);
            return string.Format(NumberFormat, "{0:0.00} GB", gigaByteSize);
        }
        public static string FormatAsPercentage(this double speed)
        {
            if (double.IsNaN(speed))
            {
                return "";
            }
            return Math.Max(Math.Min(speed, 100), 0).ToString("F") + "%";
        }

        public static string FormatAsSpeed(this long speed)
        {
            var NumberFormat = new NumberFormatInfo();
            var kbSpeed = speed/1024F;
            var mbSpeed = kbSpeed/1024F;

            //if (speed <= 0)
            //    return string.Empty;
            if (speed < 1024)
                return speed + " B/s";
            if (speed < 1048576)
                return kbSpeed.ToString("#.00", NumberFormat) + " kB/s";
            return mbSpeed.ToString("#.00", NumberFormat) + " MB/s";
        }

        public static string FormatAsTime(this TimeSpan span)
        {
            var hours = ((int) span.TotalHours).ToString();
            var minutes = span.Minutes.ToString();
            var seconds = span.Seconds.ToString();
            if ((int) span.TotalHours < 10)
                hours = "0" + hours;
            if (span.Minutes < 10)
                minutes = "0" + minutes;
            if (span.Seconds < 10)
                seconds = "0" + seconds;

            return $"{hours}:{minutes}:{seconds}";
        }
    }
}