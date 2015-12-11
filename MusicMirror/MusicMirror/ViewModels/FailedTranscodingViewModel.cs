using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.ViewModels
{
    public class FailedTranscodingViewModel
    {
        private readonly Exception _error;
        private readonly IFileNotification _fileNotification;

        public FailedTranscodingViewModel(IFileNotification fileNotification, Exception error)
        {
            if (fileNotification == null) throw new ArgumentNullException(nameof(fileNotification));
            if (error == null) throw new ArgumentNullException(nameof(error));
            _error = error;
            _fileNotification = fileNotification;
        }

        public Exception Error => _error;
        public IFileNotification FileNotification => _fileNotification;
    }
}
