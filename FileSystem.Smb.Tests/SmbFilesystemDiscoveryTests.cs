using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp.FileSystem.Smb.Discovery;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sharp.FileSystem.Smb.Tests
{
    [TestClass]
    public class SmbFileSystemDiscoveryTests
    {
        [TestMethod]
        public async Task Can_resolve_smb_hosts()
        {
            var testHostIps = Dns.GetHostEntry(TestHelpers.TestHost).AddressList;
            var hostUrls = await new SmbFileSystemDiscovery().DiscoverSmbHostsAsync(TimeSpan.FromMilliseconds(2000));
            hostUrls.FirstOrDefault(host =>
            {
                var uri = new Uri(host.SmbUrl);

                if (testHostIps.Any(ip => ip.ToString() == uri.Host) ||
                    TestHelpers.TestHost.Equals(uri.Host, StringComparison.OrdinalIgnoreCase))
                {
                    var drives = new SmbFileSystem().Directory.GetLogicalDrives(host.SmbUrl);

                    return drives.Any(d => d == TestHelpers.AnonymousShare);
                }
                return false;
            }).Should().NotBeNull("test host should be discoverable by mDNS");
        }

        [TestMethod]
        public async Task Can_resolve_smb_hosts_as_observable()
        {
            var testHostIps = Dns.GetHostEntry(TestHelpers.TestHost).AddressList;
            var hostUrls = new SmbFileSystemDiscovery().DiscoverSmbHostsContinuous(TimeSpan.FromMilliseconds(2000));

            bool found = false;
            var subscription = hostUrls.Subscribe(host =>
            {
                var uri = new Uri(host.SmbUrl);

                if (testHostIps.Any(ip => ip.ToString() == uri.Host) ||
                    TestHelpers.TestHost.Equals(uri.Host, StringComparison.OrdinalIgnoreCase))
                {
                    var drives = new SmbFileSystem().Directory.GetLogicalDrives(host.SmbUrl);

                    found |= drives.Any(d => d == TestHelpers.AnonymousShare);
                }                
            });

            await Task.Delay(2000);

            subscription.Dispose();

            found.Should().BeTrue("test host should be discoverable by mDNS");
        }
    }
}
