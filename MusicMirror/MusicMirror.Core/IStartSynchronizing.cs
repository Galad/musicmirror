using System;

namespace MusicMirror
{
    public interface IStartSynchronizing
    {
        IDisposable Start();
    }
}