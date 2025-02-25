﻿using System;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Toolbox.Core.IO
{
    public class STFileSaver
    {
        public class SaveLog
        {
            public string SaveTime = "";
        }

        /// <summary>
        /// Saves the <see cref="IFileFormat"/> as a file from the given <param name="FileName">
        /// </summary>
        /// <param name="IFileFormat">The format instance of the file being saved</param>
        /// <param name="FileName">The name of the file</param>
        /// <param name="Alignment">The Alignment used for compression. Used for Yaz0 compression type. </param>
        /// <returns></returns>
        public static SaveLog SaveFileFormat(IFileFormat fileFormat, string filePath, EventHandler onCompress = null)
        {
            SaveLog log = new SaveLog();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            fileFormat.FileInfo.FilePath = filePath;

            if (fileFormat.FileInfo.KeepOpen && File.Exists(filePath))
            {
                string savedPath = Path.GetDirectoryName(filePath);
                string tempPath = Path.Combine(savedPath, "tempST.bin");

                //Save a temporary file first to not disturb the opened file
                using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fileFormat.Save(fileStream);
                    if (fileFormat.FileInfo.Compression != null)
                    {
                        // Stream comp = CompressFile(File.OpenRead(tempPath), fileFormat);
                        //fileStream.CopyTo(comp);
                    }

                    if (fileFormat is IDisposable)
                        ((IDisposable)fileFormat).Dispose();

                    //After saving is done remove the existing file
                    File.Delete(filePath);

                    //Now move and rename our temp file to the new file path
                    File.Move(tempPath, filePath);

                    fileFormat.Load(File.OpenRead(filePath));
                }
            }
            else if (fileFormat.FileInfo.Compression != null)
            {
                onCompress?.Invoke(fileFormat, EventArgs.Empty);

                MemoryStream mem = new MemoryStream();
                fileFormat.Save(mem);
                if (fileFormat.FileInfo.Compression is Yaz0)
                {
                    File.WriteAllBytes(filePath, YAZ0.Compress(mem.ToArray(), Runtime.Yaz0CompressionLevel, (uint)((Yaz0)fileFormat.FileInfo.Compression).Alignment));
                }
                else
                {
                    mem = new MemoryStream(mem.ToArray());
                    var finalStream = CompressFile(mem, fileFormat);

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        finalStream.CopyTo(fileStream);
                    }
                }
            }
            else
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fileFormat.Save(fileStream);
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            log.SaveTime = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            return log;
        }

        public static void SaveFileBackground(IFileFormat fileFormat, string fileName, Action onCompress = null, Action onCompleted = null)
        {
            Thread Thread = new Thread((ThreadStart)(() =>
            {
                SaveFileThread(fileFormat, fileName, onCompress, onCompleted);
            }));
            Thread.Start();
        }
        private static void SaveFileThread(IFileFormat fileFormat, string fileName, Action onCompress = null, Action onCompleted = null)
        {
            MemoryStream mem = new MemoryStream();
            fileFormat.Save(mem);

            onCompress?.Invoke();
            if (fileFormat.FileInfo.Compression is Yaz0)
            {
                File.WriteAllBytes(fileName, YAZ0.Compress(mem.ToArray(), Runtime.Yaz0CompressionLevel, (uint)((Yaz0)fileFormat.FileInfo.Compression).Alignment));
            }
            else if (fileFormat.FileInfo.Compression != null)
            {
                mem = new MemoryStream(mem.ToArray());
                var finalStream = CompressFile(mem, fileFormat);

                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                    finalStream.CopyTo(fileStream);
                }
            }
            else
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fileFormat.Save(fileStream);
                }
            }
            onCompleted?.Invoke();
        }

        /// <summary>
        /// Saves the <see cref="IFileFormat"/> into the given stream.
        /// </summary>
        /// <param name="fileFormat"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Stream SaveFileFormat(IFileFormat fileFormat)
        {
            //   SaveLog log = new SaveLog();

            MemoryStream mem = new MemoryStream();
            fileFormat.Save(mem);

            if (fileFormat.FileInfo.Compression != null) {
               return CompressFile(mem, fileFormat);
            }

            return mem;
        }

        static Stream CompressFile(FileStream mem, IFileFormat fileFormat)
        {
            var compressionFormat = fileFormat.FileInfo.Compression;
            var compressedStream = compressionFormat.Compress(mem);
            //Update the compression size
            fileFormat.FileInfo.CompressedSize = (uint)compressedStream.Length;
            return compressedStream;
        }

        static Stream CompressFile(MemoryStream mem, IFileFormat fileFormat)
        {
            var compressionFormat = fileFormat.FileInfo.Compression;
            var compressedStream = compressionFormat.Compress(mem);
            //Update the compression size
            fileFormat.FileInfo.CompressedSize = (uint)compressedStream.Length;
            return compressedStream;
        }
    }
}
