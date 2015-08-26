using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hanno.Services;

namespace MusicMirror
{
	public class MessageBoxMessageDialog : IAsyncMessageDialog
	{
		public Task Show(CancellationToken ct, string title, string content)
		{
			MessageBox.Show(content, title);
			return Task.FromResult(true);
		}
	}
}
