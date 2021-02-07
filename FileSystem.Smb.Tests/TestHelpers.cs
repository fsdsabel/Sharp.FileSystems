using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FileSystem.Smb.Tests
{
    public enum ShareType
    {
        Anonymous,
        Authenticated
    }

    static class TestHelpers
    {
        public const string TestHost = "corenode";

        public const string AnonymousShare = "shared";
        
        public const string AuthenticatedShare = "auth_shared";

        public const string Username = "";
        public const string Password = "";

        public static string ShareUri(string path, ShareType type)
        {
            if (type == ShareType.Anonymous)
            {
                return $"smb://{TestHost}/{AnonymousShare}/{path}";
            }
            return $"smb://{Username}:{Password}@{TestHost}/{AuthenticatedShare}/{path}";
        }

        
    }

    public class TestShareAttribute : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[] { ShareType.Anonymous };
            //yield return new object[] { ShareType.Authenticated };
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
                return string.Format(CultureInfo.CurrentCulture, "Share type - {0} ({1})", methodInfo.Name, string.Join(",", data));

            return null;
        }
    }
}
