using System;
using System.Text;


namespace MKBB
{
    class Program
    {
        static void Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; }
            catch { }
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
