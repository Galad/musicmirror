using Microsoft.Practices.Unity;

namespace MusicMirror
{
    public interface ICompositionModule
    {
        void Compose(IUnityContainer container);
    }

    public class CompositeCompositionModule : ICompositionModule
    {
        private readonly ICompositionModule[] _modules;

        public CompositeCompositionModule(params ICompositionModule[] modules)
        {
            _modules = modules;
        }

        public void Compose(IUnityContainer container)
        {
            foreach (var module in _modules)
            {
                module.Compose(container);
            }
        }
    }
}
