using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class ConfigViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public IEnumerable<ThreadPriority> ThreadPriorityList => Enum.GetValues(typeof(ThreadPriority)).Cast<ThreadPriority>();
    }
}
