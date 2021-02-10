namespace Sharp.FileSystem.Smb.Discovery
{
    public class SmbHost
    {
        public SmbHost(string displayName, string smbUrl)
        {
            DisplayName = displayName;
            SmbUrl = smbUrl;
        }

        public string DisplayName { get; }

        public string SmbUrl { get; }
    }
}
