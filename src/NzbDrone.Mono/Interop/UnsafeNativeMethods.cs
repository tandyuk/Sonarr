using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace NzbDrone.Mono.Interop
{
    internal enum IoctlRequest : uint
    {
        // Hardcoded ioctl for FICLONE on a typical linux system
        // #define FICLONE _IOW(0x94, 9, int)
        FICLONE = 0x40049409
    }

    [Flags]
    internal enum OpenFlag : uint
    {
        O_RDONLY = 0x00000000,
        O_WRONLY = 0x00000001,
        O_RDWR   = 0x00000002,
        O_CREAT  = 0x00000100,
        O_TRUNC  = 0x00001000,
        O_APPEND = 0x00002000
    }

    internal static class UnsafeNativeMethods
    {
        [DllImport("libc", EntryPoint = "open", SetLastError = true)]
        internal static extern SafeUnixHandle Open(string path, OpenFlag flag, int mode);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport("libc", EntryPoint = "close", SetLastError = true)]
        internal static extern int Close(IntPtr handle);
        
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        private static extern int Ioctl(SafeUnixHandle dst_fd, IoctlRequest request, SafeUnixHandle src_fd);
        public static int Ioctl_FICLONE(SafeUnixHandle dst_fd, SafeUnixHandle src_fd)
        {
            return Ioctl(dst_fd, IoctlRequest.FICLONE, src_fd);
        }

        internal static string Strerror(int error)
        {
            try
            {
                var buffer = new StringBuilder(256);
                var result = Strerror(error, buffer, (ulong)buffer.Capacity);
                return (result != -1) ? buffer.ToString() : null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        [DllImport("MonoPosixHelper", EntryPoint = "Mono_Posix_Syscall_strerror_r", SetLastError = true)]
        private static extern int Strerror(int error, [Out] StringBuilder buffer, ulong length);
    }
}
