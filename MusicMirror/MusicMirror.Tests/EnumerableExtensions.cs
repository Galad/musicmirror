using Hanno.Testing.Autofixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.Tests
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
        {
            return source.Concat(element.Yield());
        }

        public static IEnumerable<T> Yield<T>(this T source)
        {
            yield return source;            
        }

        public static ITaskVerifies AsITaskVerifies(this IEnumerable<ITaskVerifies> taskVerifies)
        {
            if (taskVerifies == null)
                throw new ArgumentNullException(nameof(taskVerifies), $"{nameof(taskVerifies)} is null.");
            return new CompositeTaskVerifies(taskVerifies);
        }

        private class CompositeTaskVerifies : ITaskVerifies
        {
            private readonly IEnumerable<ITaskVerifies> _taskVerifies;

            public CompositeTaskVerifies(IEnumerable<ITaskVerifies> taskVerifies)
            {
                if (taskVerifies == null)
                    throw new ArgumentNullException(nameof(taskVerifies), $"{nameof(taskVerifies)} is null.");
                _taskVerifies = taskVerifies;
            }

            public void Verify()
            {
                foreach (var t in _taskVerifies)
                {
                    t.Verify();
                }
            }
        }
    }
}
