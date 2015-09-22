using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.ViewModels
{
    public class SynchronizedFilesCountViewModel
    {
        private static SynchronizedFilesCountViewModel _empty = new SynchronizedFilesCountViewModel(0, 0);

        public SynchronizedFilesCountViewModel(int synchronizedFilesCount, int totalFileCount)
        {
            SynchronizedFilesCount = synchronizedFilesCount;
            TotalFileCount = totalFileCount;
        }

        public int SynchronizedFilesCount { get; private set; }
        public int TotalFileCount { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return SynchronizedFilesCount == 0 && TotalFileCount == 0;
            }
        }

        public static SynchronizedFilesCountViewModel Empty
        {
            get
            {
                return _empty;
            }
        }
    }
}
