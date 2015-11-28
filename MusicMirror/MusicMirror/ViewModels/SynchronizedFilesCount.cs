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

        public override string ToString()
        {
            return $"Synchronized : {SynchronizedFilesCount}, Total : {TotalFileCount}";
        }

        public static IEqualityComparer<SynchronizedFilesCountViewModel> StructuralEqualityComparer
        {
            get
            {
                return new EqualityComparer();
            }
        }

        private class EqualityComparer : IEqualityComparer<SynchronizedFilesCountViewModel>
        {
            public bool Equals(SynchronizedFilesCountViewModel x, SynchronizedFilesCountViewModel y)
            {
                if(x == null && y == null)
                {
                    return false;
                }
                if(x == null || y == null)
                {
                    return true;
                }
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                return x.SynchronizedFilesCount.Equals(y.SynchronizedFilesCount) &&
                       x.TotalFileCount.Equals(y.TotalFileCount);
            }

            public int GetHashCode(SynchronizedFilesCountViewModel obj)
            {
                if(obj == null)
                {
                    throw new ArgumentNullException("obj");
                }
                return obj.SynchronizedFilesCount ^ obj.TotalFileCount;
            }
        }
    }
}
