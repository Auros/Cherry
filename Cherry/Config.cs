using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Cherry
{
    internal class Config
    {
        public virtual string RequestCommand { get; set; } = "!bsr";
        public virtual string CancelCommand { get; set; } = "!oops";
    }
}