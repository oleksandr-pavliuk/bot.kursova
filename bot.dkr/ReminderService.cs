using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace bot.dkr
{
    public class ReminderService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient = new TelegramBotClient("6849620831:AAHT6MgKuCi2bywDrbt2BV3dSzAdzzeueFM");
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"[{DateTime.Now}]   Background service executed (Reminder) ");
                List<long> chatsToRemind = await Repository.GetChatsForRemind(DateTime.Now.ToString("MM/dd/yyyy"));
                foreach(var chat in chatsToRemind)
                {
                    await _botClient.SendTextMessageAsync(chat, "Сьогодні твоя здача курсової роботи.&#128521;", parseMode: ParseMode.Html);
                    await Repository.SetReminderDone(chat);
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
