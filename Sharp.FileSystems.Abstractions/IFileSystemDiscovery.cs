using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sharp.FileSystems.Abstractions
{
    public interface IFileSystemDiscovery
    {
        Task<IFileSystemDiscoveryResult[]> DiscoverRootDirectoriesAsync(TimeSpan scanTime = default, CancellationToken cancellationToken = default);

        IObservable<IFileSystemDiscoveryResult> DiscoverRootDirectoriesContinuous(TimeSpan scanTime = default);
    }

    public interface IFileSystemDiscoveryResult
    {
        string DisplayName { get; }

        IFileSystem FileSystem { get; }

        string RootPath { get; }
    }
}
