using System.Globalization;
using System.Text;


namespace MKBB
{
    class Program
    {
        static void Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; }
            catch { }
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
