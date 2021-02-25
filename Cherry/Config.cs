using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Cherry
{
    internal class Config
    {
        public virtual bool QueueOpened { get; set; } = false;
        public virtual string RequestCommand { get; set; } = "!bsr";
        public virtual string CancelCommand { get; set; } = "!oops";
    }
}