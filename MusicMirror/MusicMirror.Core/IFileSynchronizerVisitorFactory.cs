using System.Diagnostics;
using System.IO;

namespace MusicMirror
{
	public interface IFileSynchronizerVisitorFactory
	{
		IFileSynchronizerVisitor CreateVisitor(Configuration targetDirectory);
	}
}