using System;
using Ploeh.AutoFixture.Kernel;
using Hanno.ViewModels;
using Hanno.Navigation;
using System.Collections;
using System.Collections.Generic;

namespace MusicMirror.Tests.Customizations
{
    public class ViewModelTransformation : ISpecimenBuilderTransformation
    {
        public ISpecimenBuilder Transform(ISpecimenBuilder builder)
        {
            return new ViewModelPostProcessor(builder);
        }
    }

    internal class ViewModelPostProcessor : ISpecimenBuilderNode
    {
        private readonly ISpecimenBuilder _builder;
        public ISpecimenBuilder Builder => _builder;

        public ViewModelPostProcessor(ISpecimenBuilder builder)
        {
            this._builder = builder;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var result = _builder.Create(request, context);
            if (result == null || !typeof(IViewModel).IsAssignableFrom(result.GetType()))
            {
                return result;
            }
            var viewModel = (IViewModel)result;
            var viewModelRequestAsObject = context.Resolve(typeof(INavigationRequest));
            if (!typeof(INavigationRequest).IsAssignableFrom(viewModelRequestAsObject.GetType()))
            {
                throw new InvalidOperationException("Could not intialized the view model because the navigation request could not be created.");
            }
            viewModel.Initialize((INavigationRequest)viewModelRequestAsObject);
            return viewModel;
        }

        public ISpecimenBuilderNode Compose(IEnumerable<ISpecimenBuilder> builders)
        {
            return new ViewModelPostProcessor(new CompositeSpecimenBuilder(builders));
        }

        public IEnumerator<ISpecimenBuilder> GetEnumerator()
        {
            yield return _builder;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}