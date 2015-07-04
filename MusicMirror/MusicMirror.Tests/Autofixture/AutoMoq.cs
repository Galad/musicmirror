using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hanno.Testing.Autofixture;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace MusicMirror.Tests.Autofixture
{
	public class HannoCustomization : CompositeCustomization
	{
		public HannoCustomization() :
			base(
			new AutoConfiguredMoqCustomization(),
			new IoCustomization())
		{
		}
	}

	public class AutoMoqDataAttribute : AutoDataAttribute
	{
		public AutoMoqDataAttribute()
			: base(new Fixture().Customize(new HannoCustomization()))
		{
		}
	}

	public class InlineAutoMoqDataAttribute : CompositeDataAttribute
	{
		public InlineAutoMoqDataAttribute(params object[] values) : base(new InlineDataAttribute(values), new AutoMoqDataAttribute())
		{
		}
	}
}
