using System;
using System.Collections.Generic;

namespace MusicMirror.ViewModels
{
    public interface ICollectionNotificationVisitor<T, TElement>
    {
        T Visit(ElementAddedNotification<TElement> notification);
        T Visit(ElementRemovedNotification<TElement> notification);
        T Visit(ResetCollectionNotification notification);
    }

    public abstract class ElementNotificationBase<TElement>
    {
        private readonly IEnumerable<TElement> _elements;

        protected ElementNotificationBase(IEnumerable<TElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            _elements = elements;
        }

        public IEnumerable<TElement> Elements => _elements;
    }

    public class ElementAddedNotification<TElement> : ElementNotificationBase<TElement>
    {
        public ElementAddedNotification(IEnumerable<TElement> elements) : base(elements)
        {
        }
    }

    public class ElementRemovedNotification<TElement> : ElementNotificationBase<TElement>
    {
        public ElementRemovedNotification(IEnumerable<TElement> elements) : base(elements)
        {
        }
    }

    public class ResetCollectionNotification
    {
    }
}