namespace BulkSSTCLI
{
    using System.Threading.Tasks;
    using PowerArgs;

    public class Program
    {
        static async Task Main(string[] args)
        {
            await Args.InvokeMainAsync<SpeechToText>(args);
        }
    }
}
