using System.ComponentModel.DataAnnotations;

namespace RINGS.Common
{
    public enum FilterTypes
    {
        [Display(Name = "完全一致")]
        FullMatch = 0,

        [Display(Name = "を含む")]
        Contains,

        [Display(Name = "から始まる")]
        StartWith,

        [Display(Name = "で終わる")]
        EndWith,

        [Display(Name = "正規表現")]
        Regex = 10,
    }
}
