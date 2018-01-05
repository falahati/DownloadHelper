using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DownloadHelper.Tests.Tests
{
    [TestClass]
    public class StreamPartitionTest
    {
        [TestMethod]
        public void RandomWrite()
        {
            const int bufferSize = 100;
            const int numberOfVolumes = 20;
            using (var stream = new MemoryStream())
            {
                var volumes = new List<StreamVolume>();
                var rand = new Random();
                for (var i = 0; i < numberOfVolumes; i++)
                    volumes.Add(new StreamVolume(stream, new StreamVolumeRange(i*bufferSize, (i + 1)*bufferSize - 1)));
                foreach (var i in Enumerable.Range(0, bufferSize).OrderBy(i => rand.Next()))
                    foreach (var volume in volumes)
                    {
                        volume.Seek(i, SeekOrigin.Begin);
                        volume.Write(new[] {(byte) i}, 0, 1);
                        Assert.AreEqual(volume.Position, i);
                    }
                var expectedOutput =
                    Enumerable.Repeat(Enumerable.Range(0, bufferSize), numberOfVolumes).SelectMany(i => i);
                Assert.IsTrue(stream.ToArray().Select(b => (int) b).SequenceEqual(expectedOutput));
            }
        }

        [TestMethod]
        public void PartitionAllocation()
        {
            const int bufferSize = 100;
            const int numberOfVolumes = 20;
            using (var stream = new MemoryStream())
            {
                stream.SetLength(bufferSize * numberOfVolumes);
                var partition = new StreamPartition(stream);
                for (int i = 0; i < numberOfVolumes; i++)
                {
                    var volume = partition.GetFreeVolume(0, bufferSize);
                    Assert.IsNotNull(volume);
                    Assert.AreEqual(volume.Range.Start, i * bufferSize);
                    Assert.AreEqual(volume.Range.Length, bufferSize);
                }

                var lastVolume = partition.GetFreeVolume(0, bufferSize);
                Assert.IsNull(lastVolume);
            }
        }
    }
}