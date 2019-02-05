using System.Collections.Generic;

namespace RINGS
{
    public partial class Config
    {
        public override Dictionary<string, object> DefaultValues => new Dictionary<string, object>()
        {
            { nameof(Scale), 1.0d },
            { nameof(X), 200 },
            { nameof(Y), 100 },
            { nameof(W), 1024 },
            { nameof(H), 576 },
            { nameof(IsStartupWithWindows), false },
            { nameof(IsMinimizeStartup), false },
        };
    }
}
