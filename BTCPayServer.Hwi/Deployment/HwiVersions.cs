using System;
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
        public static HwiVersion v1_1_2 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1.1.2/hwi-1.1.2-windows-amd64.zip",
                Hash = "42797941cb126bc8b50ccd8001fb3db7c216ac30795876fba0b734c03234e2ae",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1.1.2/hwi-1.1.2-linux-amd64.tar.gz",
                Hash = "aa3fc5947305e2348d163d2a704a0836c3ce1dd930a55203004f1ef03c983aac",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1.1.2/hwi-1.1.2-mac-amd64.tar.gz",
                Hash = "feb8a7bf10ed8a5e775811b8a7ce887437fd5dc8039272f48ecb888b17c59cd5",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion v1_2_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1_2_1/hwi-1_2_1-windows-amd64.zip",
                Hash = "3fc085b8086469c1983f189806f10eadb0f43bc9da75d5fa91f15c292898c6cc",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1_2_1/hwi-1_2_1-linux-amd64.tar.gz",
                Hash = "b432635e6f609f7498a696029dd61188ceb5e4776660a3414a528280249aa0f1",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/Groestlcoin/HWI/releases/download/1_2_1/hwi-1_2_1-mac-amd64.tar.gz",
                Hash = "52186c3f0e888c85b7867e95955d8086bcde87b8ffa4179cdf0f151471bf102a",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion Latest => v1_2_1;
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
