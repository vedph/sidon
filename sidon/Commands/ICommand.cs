using System.Threading.Tasks;

namespace Sidon.Commands
{
    internal interface ICommand
    {
        Task Run();
    }
}
