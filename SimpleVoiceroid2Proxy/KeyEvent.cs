using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVoiceroid2Proxy
{
    class KeyEvent : IDisposable
    {

        public KeyEvent ()
        {

        }

        public async Task ConsumeAsync()
        {
            while (true)
            {
                try
                {
                    await this.HandleAsync();
                }
                catch (Exception exc)
                {

                    Program.Logger.Error(exc);
                }
            }
        }

        public async Task HandleAsync()
        {

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key.ToString();
                //await Program.AIVoiceroidEngine.TalkAsync($"{key}");
                //Program.Logger.Break();
                switch (key)
                {
                    case "F1":
                        Program.AIVoiceroidEngine.Toggle();
                        break;
                    case "F2":
                        String state = Program.AIVoiceroidEngine.ChangeInterruptMode();
                        await Program.AIVoiceroidEngine.TalkAsync($"{state}にしました");
                        break;
                    case "F3":
                        String voice = Program.AIVoiceroidEngine.ChangePresets();
                        await Program.AIVoiceroidEngine.TalkAsync($"話者を{voice}に切り替えました");
                        break;
                    case "Spacebar":
                        Console.Clear();
                        Program.Server.PrintInfo();
                        Program.AIVoiceroidEngine.PrintInfo();
                        Program.AIVoiceroidEngine.PrintInterruptMode();
                        Program.AIVoiceroidEngine.PrintPresets();
                        Program.AIVoiceroidEngine.PrintHelp();

                        await Program.AIVoiceroidEngine.TalkAsync($"コンソールをクリアしました");
                        break;
                }



            }
        }



        public void Dispose()
        {
        }
    }
}
