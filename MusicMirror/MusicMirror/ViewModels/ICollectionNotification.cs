using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.ViewModels
{
    interface ICollectionNotification<TElement>
    {
        T Accept<T>(ICollectionNotificationVisitor<T, TElement> visitor);
    }
}
