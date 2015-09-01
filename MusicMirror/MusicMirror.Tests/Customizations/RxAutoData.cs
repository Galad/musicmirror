using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Hanno.Testing.Autofixture;

namespace MusicMirror.Tests.Customizations
{
	public class DomainRxAutoDataAttribute : AutoDataAttribute
	{
		public DomainRxAutoDataAttribute() : base(new Fixture().Customize(new RxCustomization()).Customize(new HannoAutoConfiguredMoqCustomization()))
		{
		}
	}
}
