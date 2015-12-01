using Hanno.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ConfigurationViewModel ConfigurationViewModel { get; private set; }
        public SynchronizationStatusViewModel SynchronizationStatusViewModel { get; private set; }

        public MainPageViewModel(
            ConfigurationViewModel configurationViewModel,
            SynchronizationStatusViewModel synchronizationStatusViewModel,
            IViewModelServices services) : base(services)
        {            
            if(configurationViewModel == null)
            {
                throw new ArgumentNullException(nameof(configurationViewModel));
            }
            if(synchronizationStatusViewModel == null)
            {
                throw new ArgumentNullException(nameof(synchronizationStatusViewModel));
            }
            ConfigurationViewModel = configurationViewModel;
            SynchronizationStatusViewModel = synchronizationStatusViewModel;
            RegisterChild(ConfigurationViewModel);
            RegisterChild(SynchronizationStatusViewModel);
        }
    }
}
