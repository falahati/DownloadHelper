using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DownloadHelper.EventArguments;
using DownloadHelper.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DownloadHelper.Tests.Tests
{
    [TestClass]
    public class DownloadConnectionTest
    {
        private const long KB = 1024;
        private const long MB = 1024*KB;

        private CancellationTokenSource _serverCancellation;
        private Task _serverTask;

        private Uri DummyUri { get; set; }
        private HttpFile File { get; set; }
        private HttpServer Server { get; set; }

        [TestCleanup]
        public void Cleanup()
        {
            _serverCancellation.Cancel();
            _serverTask.Wait();
            _serverTask.Dispose();
            _serverCancellation.Dispose();
            File.Dispose();
        }

        [TestInitialize]
        public void Initialize()
        {
            _serverCancellation = new CancellationTokenSource();
            File = HttpFile.MakeFile(100*MB).Result;
            Server = new HttpServer();
            DummyUri = new Uri(Server.Root + "file.bin");
            _serverTask = Server.Start(File, _serverCancellation.Token);
        }


        [TestMethod]
        public void InvalidResponseRange()
        {
            Server.FileName = "MyFile.zip";
            var file = GetDummyFile();
            ResponseReceivedEventArgs responseReceivedEventArgs = null;
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), new DownloadRange(0, File.Size*2), fs)
                )
                {
                    connection.ResponseReceived += (sender, args) => { responseReceivedEventArgs = args; };
                    try
                    {
                        Assert.IsNotNull(StartConnection(connection).Result);
                        Assert.Fail("Connection failed to capture invalid response.");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex.InnerException is DownloadException);
                    }
                }
                fs.Close();
            }
            Assert.IsNotNull(responseReceivedEventArgs);
            Assert.AreEqual(HttpStatusCode.RequestedRangeNotSatisfiable, responseReceivedEventArgs.Response.StatusCode);
            Assert.AreEqual(0, responseReceivedEventArgs.Response.Range.Start);
            Assert.AreEqual(File.Size, responseReceivedEventArgs.Response.FileSize);
            Assert.AreEqual(0, responseReceivedEventArgs.Response.ContentLength);
            Assert.AreEqual(Server.FileName, responseReceivedEventArgs.Response.FileName);
            Assert.AreEqual(Server.ContactType, responseReceivedEventArgs.Response.ContentType);
            Assert.AreEqual(DummyUri, responseReceivedEventArgs.Response.Url);
            file.Delete();
        }

        [TestMethod]
        public void ShrinkOnResponse()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), fs))
                {
                    connection.ResponseReceived += (sender, args) =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        connection.Range = connection.Range.ShrinkLength(5*KB);
                        // ReSharper restore AccessToDisposedClosure
                    };
                    Assert.IsNotNull(StartConnection(connection).Result);
                }
                fs.Close();
            }
            Assert.AreEqual(5*KB, file.Length);
            Assert.IsTrue(File.IsEqual(0, 5*KB, file).Result);
            file.Delete();
        }

        [TestMethod]
        public void ShrinkOnRangedDataAvailable()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), new DownloadRange(5*KB), fs))
                {
                    EventHandler<DataAvailableEventArgs> eventHandler = null;
                    eventHandler = (sender, args) =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        if (connection.TotalDownloaded == 10*KB)
                        {
                            connection.Range = connection.Range.ShrinkLength(10*KB);
                            connection.DataAvailable -= eventHandler;
                        }
                        // ReSharper restore AccessToDisposedClosure
                    };
                    connection.DataAvailable += eventHandler;
                    Assert.IsNotNull(StartConnection(connection).Result);
                }
                fs.Close();
            }
            Assert.AreEqual(10*KB, file.Length);
            Assert.IsTrue(File.IsEqual(5*KB, 10*KB, file).Result);
            file.Delete();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BadShrinkOnRangedDataAvailable()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), new DownloadRange(5*KB), fs))
                {
                    EventHandler<DataAvailableEventArgs> eventHandler = null;
                    eventHandler = (sender, args) =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        if (connection.TotalDownloaded == 10*KB)
                        {
                            connection.Range = connection.Range.ShrinkLength(10*KB - 1);
                            connection.DataAvailable -= eventHandler;
                        }
                        // ReSharper restore AccessToDisposedClosure
                    };
                    connection.DataAvailable += eventHandler;
                    try
                    {
                        Assert.IsNotNull(StartConnection(connection).Result);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException != null)
                            throw ex.InnerException;
                        throw;
                    }
                }
                fs.Close();
            }
            file.Delete();
        }


        [TestMethod]
        public void ShrinkOnRangedProgressChanged()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), new DownloadRange(5*KB), fs))
                {
                    EventHandler eventHandler = null;
                    eventHandler = (sender, args) =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        if (connection.TotalDownloaded == 10*KB)
                        {
                            connection.Range = connection.Range.ShrinkLength(40*KB);
                            connection.ProgressChanged -= eventHandler;
                        }
                        // ReSharper restore AccessToDisposedClosure
                    };
                    connection.ProgressChanged += eventHandler;
                    Assert.IsNotNull(StartConnection(connection).Result);
                }
                fs.Close();
            }
            Assert.AreEqual(40*KB, file.Length);
            Assert.IsTrue(File.IsEqual(5*KB, 40*KB, file).Result);
            file.Delete();
        }

        [TestMethod]
        public void Download()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), fs))
                {
                    Assert.IsNotNull(StartConnection(connection).Result);
                }
                fs.Close();
            }
            Assert.IsTrue(File.IsEqual(file).Result);
            file.Delete();
        }

        [TestMethod]
        public void DownloadResume()
        {
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    using (var connection = new DownloadConnection(GetDummyRequest(), fs))
                    {
                        EventHandler eventHandler = null;
                        eventHandler = (sender, args) =>
                        {
                            // ReSharper disable AccessToDisposedClosure
                            if (connection.TotalDownloaded >= 100 * KB)
                            {
                                connection.ProgressChanged -= eventHandler;
                                cancellation.Cancel();
                            }
                            // ReSharper restore AccessToDisposedClosure
                        };
                        connection.ProgressChanged += eventHandler;
                        Assert.IsNotNull(StartConnection(connection, cancellation.Token).Result);
                        // ReSharper disable once MethodSupportsCancellation
                        Assert.IsNotNull(StartConnection(connection).Result);
                    }
                }
                fs.Close();
            }
            Assert.IsTrue(File.IsEqual(file).Result);
            file.Delete();
        }

        [TestMethod]
        public void Limited20MB()
        {
            Limited(20*MB);
        }

        [TestMethod]
        public void Limited2MB()
        {
            Limited(2*MB);
        }

        [TestMethod]
        public void Limited500KB()
        {
            Limited(500*KB);
        }

        [TestMethod]
        public void ValidResponse()
        {
            Server.FileName = "MyFile.zip";
            var file = GetDummyFile();
            ResponseReceivedEventArgs responseReceivedEventArgs = null;
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), fs))
                {
                    connection.ResponseReceived += (sender, args) =>
                    {
                        args.Abort = true;
                        responseReceivedEventArgs = args;
                    };
                    try
                    {
                        Assert.IsNotNull(StartConnection(connection).Result);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex.InnerException is OperationCanceledException))
                        {
                            throw;
                        }
                    }
                }
                fs.Close();
            }
            Assert.IsNotNull(responseReceivedEventArgs);
            Assert.IsTrue((responseReceivedEventArgs.Response.StatusCode == HttpStatusCode.PartialContent) ||
                          (responseReceivedEventArgs.Response.StatusCode == HttpStatusCode.OK));
            Assert.AreEqual(0, responseReceivedEventArgs.Response.Range.Start);
            Assert.AreEqual(File.Size, responseReceivedEventArgs.Response.Range.Length);
            Assert.AreEqual(File.Size, responseReceivedEventArgs.Response.FileSize);
            Assert.AreEqual(File.Size, responseReceivedEventArgs.Response.ContentLength);
            Assert.AreEqual(Server.FileName, responseReceivedEventArgs.Response.FileName);
            Assert.AreEqual(Server.ContactType, responseReceivedEventArgs.Response.ContentType);
            Assert.AreEqual(DummyUri, responseReceivedEventArgs.Response.Url);
            file.Delete();
        }

        [TestMethod]
        public void ValidRangeResponse()
        {
            Server.FileName = "MyFile.zip";
            var file = GetDummyFile();
            ResponseReceivedEventArgs responseReceivedEventArgs = null;
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), new DownloadRange(100, 200), fs))
                {
                    connection.ResponseReceived += (sender, args) =>
                    {
                        args.Abort = true;
                        responseReceivedEventArgs = args;
                    };
                    try
                    {
                        Assert.IsNotNull(StartConnection(connection).Result);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex.InnerException is OperationCanceledException))
                        {
                            throw;
                        }
                    }
                }
                fs.Close();
            }
            Assert.IsNotNull(responseReceivedEventArgs);
            Assert.AreEqual(HttpStatusCode.PartialContent, responseReceivedEventArgs.Response.StatusCode);
            Assert.AreEqual(100, responseReceivedEventArgs.Response.Range.Start);
            Assert.AreEqual(200, responseReceivedEventArgs.Response.Range.End);
            Assert.AreEqual(File.Size, responseReceivedEventArgs.Response.FileSize);
            Assert.AreEqual(101, responseReceivedEventArgs.Response.ContentLength);
            Assert.AreEqual(Server.FileName, responseReceivedEventArgs.Response.FileName);
            Assert.AreEqual(Server.ContactType, responseReceivedEventArgs.Response.ContentType);
            Assert.AreEqual(DummyUri, responseReceivedEventArgs.Response.Url);
            file.Delete();
        }

        private FileInfo GetDummyFile()
        {
            return new FileInfo(Path.GetTempFileName());
        }

        private DownloadRequest GetDummyRequest()
        {
            return new DownloadRequest(DummyUri);
        }

        private void Limited(long limit)
        {
            var expectedTime = 1000d*File.Size/limit;
            Debug.WriteLine("Limited at {0:N} B/s, expected to download in {1:F} seconds", limit, expectedTime/1000d);

            var limiter = new DownloadSpeedLimiter(limit);
            var file = GetDummyFile();
            using (var fs = file.OpenWrite())
            {
                using (var connection = new DownloadConnection(GetDummyRequest(), fs))
                {
                    connection.SpeedLimiter = limiter;
                    var completedEventArgs = StartConnection(connection).Result;

                    Assert.IsNotNull(completedEventArgs);

                    var elapsed = completedEventArgs.Completed - completedEventArgs.Started;

                    Assert.AreEqual(expectedTime, elapsed.TotalMilliseconds, expectedTime * 0.2);
                    Assert.AreEqual(limit, completedEventArgs.AverageSpeed, limit * 0.2);
                }
                fs.Close();
            }
            file.Delete();
        }

        private async Task<CompletedEventArgs> StartConnection(DownloadConnection connection, CancellationToken stopToken = default(CancellationToken))
        {
            Debug.WriteLine("Downloading {0:N} bytes file", File.Size);
            CompletedEventArgs completedEventArgs = null;
            connection.StatusChanged += (sender, args) =>
            {
                // ReSharper disable once AccessToDisposedClosure
                Debug.WriteLine("[Status]\t\t" + connection.Status.ToString());
            };
            connection.MessageChanged += (sender, args) =>
            {
                // ReSharper disable once AccessToDisposedClosure
                Debug.WriteLine("[Message]\t" + connection.Message);
            };
            connection.Completed += (sender, args) => { completedEventArgs = args; };
            var sw = new Stopwatch();
            sw.Start();
            await connection.Start(stopToken);
            sw.Stop();
            Debug.WriteLine("Download ended after {0:F} seconds", sw.Elapsed.TotalSeconds);

            if (completedEventArgs != null)
            {
                Debug.WriteLine("Completed in {0:F} seconds at average speed of {1:N} B/s",
                    (completedEventArgs.Completed - completedEventArgs.Started).TotalSeconds,
                    completedEventArgs.AverageSpeed);
            }
            else
            {
                Debug.WriteLine("Completed {0} ('{1}')", connection.Status, connection.Message);
            }
            return completedEventArgs;
        }
    }
}