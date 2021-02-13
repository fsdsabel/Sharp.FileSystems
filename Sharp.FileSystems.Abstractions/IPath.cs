namespace Sharp.FileSystems.Abstractions
{
    public interface IPath
    {
        /// <summary>
        /// Exposes the underlying filesystem implementation. This is useful for implementing extension methods.
        /// </summary>
        IFileSystem FileSystem { get; }

        /// <inheritdoc cref="Path.Combine(string[])"/>
        string Combine(params string[] paths);
    }
}