using System;
using System.Speech;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new SpeechSynthesizer();
             reader.Speak("Hi I am Manikandan");
        }
    }
}
