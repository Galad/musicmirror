using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Kernel;
using Hanno.ViewModels;
using Hanno.Navigation;
using Ploeh.AutoFixture.Xunit2;
using MusicMirror.ViewModels;
using Hanno.Services;
using Moq;
using System.Reactive.Linq;
using Hanno;
using Hanno.Validation;
using Hanno.CqrsInfrastructure;
using Xunit.Sdk;
using Xunit;
using Hanno.Testing.Autofixture;
using System.Reflection;

namespace MusicMirror.Tests.Customizations
{
    public class ViewModelCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Behaviors.Add(new ViewModelTransformation());
            fixture.Customize<Mock<IViewModelServices>>(c => c.Do(m =>
            {
                m.DefaultValue = DefaultValue.Empty;
                m.Setup(s => s.Schedulers).Returns(fixture.Create<ISchedulers>());
                m.Setup(s => s.Validator).Returns(fixture.Create<IValidator>());
                m.Setup(s => s.CommandBus).Returns(fixture.Create<IAsyncCommandBus>());
                m.Setup(s => s.QueryBus).Returns(fixture.Create<IAsyncQueryBus>());
                m.Setup(s => s.QueryStateEvents).Returns(fixture.Create<IQueryStateEvents>());
                m.Setup(s => s.RequestNavigation).Returns(fixture.Create<IRequestNavigation>());
                m.Setup(s => s.RuleProvider).Returns(fixture.Create<IRuleProvider>());                
            })
            .OmitAutoProperties());
        }
    }

    public class ViewModelCompositeCustomization : CompositeCustomization
    {
        public ViewModelCompositeCustomization() : base(
            new FileCompositeCustomization(),
            new RxCustomization(),
            new ViewModelCustomization())
        {
        }
    }

    public class ViewModelAutoData : AutoDataAttribute
    {
        public ViewModelAutoData() : base(new Fixture().Customize(new ViewModelCompositeCustomization()))
        {
        }
    }

    public class ViewModelInlineAutoData : CompositeDataAttribute
    {
        public ViewModelInlineAutoData(params object[] data) : base(new InlineDataAttribute(data), new ViewModelAutoData())
        {
        }
    }
}
