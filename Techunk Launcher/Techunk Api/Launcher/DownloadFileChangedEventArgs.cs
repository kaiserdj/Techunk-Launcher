using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Techunk_Api.Launcher
{
    public enum MFile { Library, Resource, Minecraft };

    public class DownloadFileChangedEventArgs : EventArgs
    {
        public MFile FileKind;
        public string FileName;
        public int TotalFileCount;
        public int ProgressedFileCount;
    }
}
