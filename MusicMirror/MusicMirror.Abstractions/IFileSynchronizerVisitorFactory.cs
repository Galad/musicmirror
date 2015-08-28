using MusicMirror.Entities;
using System.Diagnostics;
using System.IO;

namespace MusicMirror
{
	public interface IFileSynchronizerVisitorFactory
	{
		IFileSynchronizerVisitor CreateVisitor(MusicMirrorConfiguration configuration);
	}
}