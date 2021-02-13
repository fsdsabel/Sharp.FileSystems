using Sharp.FileSystems.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf;

namespace Sharp.FileSystem.Smb.Discovery
{
    public class SmbFileSystemDiscovery : IFileSystemDiscovery
    {
        private const string SmbDomain = "_smb._tcp.local.";

        /// <summary>
        /// Discovers available SMB servers via mDNS/Bonjour.
        /// </summary>
        /// <param name="scanTime">Default is 2 seconds</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SmbHost[]> DiscoverSmbHostsAsync(TimeSpan scanTime = default, CancellationToken cancellationToken = default)
        {
            var responses = await ResolveAsync(scanTime, cancellationToken);

            return responses
                .Select(host => (host, ip: GetIpAddress(host)))
                .Where(host => !string.IsNullOrEmpty(host.ip))
                .Select(host => new SmbHost(host.host.DisplayName, $"smb://{host.ip}"))
                .ToArray();
        }


        public async Task<IFileSystemDiscoveryResult[]> DiscoverRootDirectoriesAsync(TimeSpan scanTime = default, CancellationToken cancellationToken = default)
        {
            var hosts = await DiscoverSmbHostsAsync(scanTime, cancellationToken);
            var smbFileSystem = new SmbFileSystem();
            return hosts.SelectMany(host =>
                    smbFileSystem.Directory.GetLogicalDrives(host.SmbUrl).Select(ld =>
                        new SmbFileSystemDiscoveryResult(
                            smbFileSystem,
                            host.SmbUrl,
                            host.DisplayName)
               )).ToArray();
        }

        /// <summary>
        /// Resolves available SMB servers continuously until disposed.
        /// </summary>
        /// <param name="scanTime">Default is 2 seconds</param>
        public IObservable<SmbHost> DiscoverSmbHostsContinuous(TimeSpan scanTime = default)
        {
            var inner = ResolveContinuous(scanTime);
            return inner.Repeat()
                .Select(host => (host, ip: GetIpAddress(host)))
                .Where(host => !string.IsNullOrEmpty(host.ip))
                .Select(host => new SmbHost(host.host.DisplayName, $"smb://{host.ip}"));
        }

        public IObservable<IFileSystemDiscoveryResult> DiscoverRootDirectoriesContinuous(TimeSpan scanTime = default)
        {
            var inner = DiscoverSmbHostsContinuous(scanTime);
            var smbFileSystem = new SmbFileSystem();
            return inner.Repeat()
                .SelectMany(host =>
                    smbFileSystem.Directory.GetLogicalDrives(host.SmbUrl).Select(ld =>
                        new SmbFileSystemDiscoveryResult(
                            smbFileSystem,
                            host.SmbUrl,
                            host.DisplayName)                        
               ));
        }

        protected virtual IObservable<IZeroconfHost> ResolveContinuous(TimeSpan scanTime)
        {
            return ZeroconfResolver.ResolveContinuous(SmbDomain, scanTime); ;
        }

        protected virtual async Task<IReadOnlyList<IZeroconfHost>> ResolveAsync(TimeSpan scanTime, CancellationToken cancellationToken)
        {
            return await ZeroconfResolver.ResolveAsync(SmbDomain, scanTime, cancellationToken: cancellationToken);
        }

        protected virtual string GetIpAddress(IZeroconfHost host)
        {
            var port = host.Services[SmbDomain].Port;
            return (host.IPAddresses.FirstOrDefault(ip => IPAddress.Parse(ip).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?? host.DisplayName.ToLowerInvariant()) + ":" + port;
        }

    }
}
