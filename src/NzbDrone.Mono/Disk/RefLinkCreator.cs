using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Mono.Interop;

namespace NzbDrone.Mono.Disk
{
    public interface ICreateRefLink
    {
        bool TryCreateRefLink(string target, string link);
    }

    public class RefLinkCreator : ICreateRefLink
    {
        private readonly Logger _logger;

        public RefLinkCreator(Logger logger)
        {
            _logger = logger;
        }

        public bool TryCreateRefLink(string target, string link)
        {
            if (OsInfo.IsLinux)
            {
                try
                {
                    using (var target_fd = UnsafeNativeMethods.Open(target, OpenFlag.O_RDONLY, 0))
                    using (var link_fd = UnsafeNativeMethods.Open(link, OpenFlag.O_RDWR | OpenFlag.O_CREAT, 0))
                    {
                        if (UnsafeNativeMethods.Ioctl_FICLONE(target_fd, link_fd) == -1)
                        {
                            throw new UnixIOException();
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex, "Failed to create reflink from '{0}' to '{1}'", link, target);
                }
            }

            return false;
        }
    }


}
