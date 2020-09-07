using System.Threading.Tasks;

namespace TcsCalendar
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new TcsDividendsCalendar().DisplayCalendar();
        }
    }
}
