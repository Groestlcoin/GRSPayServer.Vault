﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NBitcoin.DataEncoders;
using System.Security.Cryptography;
using System.Security;
using System.Threading;

namespace BTCPayServer.Hwi.Deployment
{
    public class HwiVersions
    {
        public static HwiVersion v2_0_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/2.0.1/hwi-2.0.1-windows-amd64.zip",
                Hash = "2cfdd6ae51e345f8c70214d626430c8d236336688a87f7d85fc6f3d6a8392da8",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/2.0.1/hwi-2.0.1-linux-amd64.tar.gz",
                Hash = "7aefba163b12970f6f538592a6c8943b72f23b22a5baedd1cf4fcfd07b1876e6",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/2_0_1/hwi-2_0_1-mac-amd64.tar.gz",
                Hash = "96bce6742cf12e7051158a8c90961aa077e183679453aa1f5a29ac1795d0e241",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion Latest => v2_0_1;
    }

    public class HwiVersion
    {
        public HwiDownloadInfo Windows { get; set; }
        public HwiDownloadInfo Linux { get; set; }
        public HwiDownloadInfo Mac { get; set; }
        public HwiDownloadInfo Current
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Windows :
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Linux :
                   RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Mac :
                   throw new NotSupportedException();
            }
        }
    }

    public class HwiDownloadInfo
    {
        static HttpClient HttpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(10.0) };
        public string Link { get; set; }
        public string Hash { get; set; }
        public IExtractor Extractor { get; set; }
        private static string GetFileHash(string processName)
        {
            byte[] checksum;
            using (var stream = File.Open(processName, FileMode.Open, FileAccess.Read))
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                checksum = sha.ComputeHash(bufferedStream);
            }
            return Encoders.Hex.EncodeData(checksum);
        }

        /// <summary>
        /// Download HWI, extract, check the hash and returns the full path to the executable
        /// </summary>
        /// <param name="destinationDirectory">Destination where to put the executable</param>
        /// <returns>The full path to the hwi executable</returns>
        public async Task<string> EnsureIsDeployed(string destinationDirectory = null, bool enforceHash = true, CancellationToken cancellationToken = default)
        {
            destinationDirectory = string.IsNullOrEmpty(destinationDirectory) ? "." : destinationDirectory;
            var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hwi.exe" : "hwi";
            var processFullPath = Path.Combine(destinationDirectory, processName);
            bool hasDownloaded = false;

download:
            if (!File.Exists(processFullPath))
            {
                var data = await HttpClient.GetStreamAsync(Link);
                var downloadedFile = Path.Combine(destinationDirectory, Link.Split('/').Last());
                try
                {
                    using (var fs = File.Open(downloadedFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        await data.CopyToAsync(fs, cancellationToken);
                    }
                    await Extractor.Extract(downloadedFile, processFullPath);
                    hasDownloaded = true;
                }
                finally
                {
                    if (File.Exists(downloadedFile))
                        File.Delete(downloadedFile);
                }
            }
            if (File.Exists(processFullPath))
            {
                if (Hash != GetFileHash(processFullPath))
                {
                    if (hasDownloaded)
                    {
                        throw new SecurityException($"Incorrect hash for {processFullPath}");
                    }
                    else if (enforceHash)
                    {
                        File.Delete(processFullPath);
                        goto download;
                    }
                }
            }
            return processFullPath;
        }
    }
}
