using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace bot.dkr
{
    public static class Program
    {
        private static ITelegramBotClient? _botClient;
        private static ReceiverOptions? _receiverOptions;
        static void Main(string[] args)
        {
            _botClient = new TelegramBotClient("token");
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                ThrowPendingUpdates = true
            };

            using var cts = new CancellationTokenSource();
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            Console.WriteLine($"[{DateTime.UtcNow}]   Bot started successfully");
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost => {
                })
                .ConfigureServices((hostContext, services) => {
                    services.AddHostedService<ReminderService>();
                })
                .UseConsoleLifetime()
                .Build();

            host.Run();
            Console.Read();
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
           
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            await MessageService.ProcessMessage(botClient, update);
                            return;
                        }
                    case UpdateType.CallbackQuery:
                        {
                            await MessageService.ProcessMessage(botClient, update);
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {

            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}