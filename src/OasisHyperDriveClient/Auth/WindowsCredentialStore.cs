using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using OasisHyperDriveClient.Core.Auth;

namespace OasisHyperDriveClient.Auth;

[SupportedOSPlatform("windows")]
public class WindowsCredentialStore : ICredentialStore
{
    private const string TargetName = "OASISHyperDriveClient_JWT";

    public void SaveToken(string token)
    {
        var blob = Encoding.UTF8.GetBytes(token);
        var cred = new CREDENTIAL
        {
            Type = 1,
            TargetName = TargetName,
            CredentialBlobSize = (uint)blob.Length,
            CredentialBlob = Marshal.AllocHGlobal(blob.Length),
            Persist = 2,
            UserName = "oasis"
        };
        Marshal.Copy(blob, 0, cred.CredentialBlob, blob.Length);
        try { CredWrite(ref cred, 0); }
        finally { Marshal.FreeHGlobal(cred.CredentialBlob); }
    }

    public string? LoadToken()
    {
        if (!CredRead(TargetName, 1, 0, out var credPtr)) return null;
        try
        {
            var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
            if (cred.CredentialBlob == IntPtr.Zero || cred.CredentialBlobSize == 0) return null;
            var blob = new byte[cred.CredentialBlobSize];
            Marshal.Copy(cred.CredentialBlob, blob, 0, blob.Length);
            return Encoding.UTF8.GetString(blob);
        }
        finally { CredFree(credPtr); }
    }

    public void ClearToken() => CredDelete(TargetName, 1, 0);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll")]
    private static extern void CredFree([In] IntPtr buffer);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, uint type, int flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }
}
