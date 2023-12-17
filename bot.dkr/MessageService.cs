using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.dkr
{
    public static class MessageService
    {
        public static async Task ProcessMessage(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"[{DateTime.UtcNow}]   Update received");
            string message = "";
            long chatId;
            try
            {
                message = update.Message.Text;
                chatId = update.Message.Chat.Id;

            }
            catch
            {
                message = update.CallbackQuery.Data.ToString();
                chatId = update.CallbackQuery.Message.Chat.Id;
            }
            
            switch (message) 
            {
                case "/start":
                    await StartCommandHandler(botClient, chatId);
                    break;
                case "submission":
                    await SubmissionCommandHandler(botClient, chatId);
                    break;
                case "cancel":
                    await CancelCommandHandler(botClient, chatId);
                    break;
                case "reminder":
                    await ReminderCommandHandler(botClient, chatId);
                    break;
                case var date when date.StartsWith("date"):
                    await DateCommandHandler(botClient, chatId, date);
                    break;
                case var time when time.StartsWith("time"):
                    await TimeCommandHandler(botClient, chatId, time.Remove(0,5));
                    break;
                case var name when name.StartsWith("ЛА"):
                    await FullNameCommandHandler(botClient, chatId, message);
                    break;
                case "/admin":
                    await AdminCommandHandler(botClient, chatId);
                    break;
                case "list":
                    await ListCommandHandler(botClient, chatId);
                    break;
                default:
                    await botClient.SendTextMessageAsync(chatId, "Вибачте, я не знаю даної команди.");
                    await StartCommandHandler(botClient, chatId);
                    break;
            }
        }

        
        private static async Task StartCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            await Repository.SaveNewStudentAsync(chatId);
            await botClient.SendTextMessageAsync(chatId, "Привіт, тебе вітає бот для запису на здачу курсових робіт ТПЗА. &#9995;", parseMode: ParseMode.Html, replyMarkup:
                            new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>()
                            {
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Записатися на здачу", "submission"),
                                            InlineKeyboardButton.WithCallbackData("Відмінити бронювання", "cancel"),
                                        },
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithUrl("Кампус", "https://ecampus.kpi.ua/home"),
                                            InlineKeyboardButton.WithCallbackData("Налаштувати нагадування", "reminder")
                                        }
                            }));
        }
        private static async Task SubmissionCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, "Оберіть дату запису на захист курсової роботи. &#128198;", parseMode: ParseMode.Html, replyMarkup:
                            new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>()
                            {
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("18.12", "date 12/18/2023"),
                                            InlineKeyboardButton.WithCallbackData("19.12", "date 12/19/2023"),
                                        },
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("20.12", "date 12/20/2023"),
                                            InlineKeyboardButton.WithCallbackData("21.12", "date 12/21/2023")
                                        },
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("22.12", "date 12/22/2023"),
                                            InlineKeyboardButton.WithCallbackData("23.12", "date 12/23/2023")
                                        }
                            }));
        }
        private static async Task CancelCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            await Repository.CancelForChatAsync(chatId);
            await botClient.SendTextMessageAsync(chatId, "Успішно відмінено запис на здачу курсової роботи. &#128076;", parseMode: ParseMode.Html);
        }
        private static async Task ReminderCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            await Repository.SetReminderForChatAsync(chatId);
            await botClient.SendTextMessageAsync(chatId, "Успішно встановлено нагадування про здачу курсової роботи. &#128226;", parseMode: ParseMode.Html);
        }
        private static async Task DateCommandHandler(ITelegramBotClient botClient, long chatId, string dateMessage)
        {
            var keyboard = await Repository.CheckDateKeyboard(dateMessage);
            if (keyboard.Count == 0) 
            {
                await botClient.SendTextMessageAsync(chatId, "Немає, на жаль, вільних годин, обери іншу дату. &#128532;", parseMode: ParseMode.Html, replyMarkup:
                           new InlineKeyboardMarkup(
                           new List<InlineKeyboardButton[]>()
                           {
                           new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Записатися на здачу", "submission"),
                                            InlineKeyboardButton.WithCallbackData("Відмінити бронювання", "cancel"),
                                        },
                           new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithUrl("Кампус", "https://ecampus.kpi.ua/home"),
                                            InlineKeyboardButton.WithCallbackData("Налаштувати нагадування", "reminder")
                                        }
                           }));
            }
            await Repository.SaveDateForChatAsync(chatId, DateOnly.Parse(dateMessage.Remove(0, 5)));
            await botClient.SendTextMessageAsync(chatId, "Добре, оберіть час на який Ви б хотіли записатися. &#128336;", parseMode: ParseMode.Html, replyMarkup:new InlineKeyboardMarkup(keyboard));
        }
        private static async Task TimeCommandHandler(ITelegramBotClient botClient, long chatId, string time)
        {
            await Repository.SaveTimeForChatAsync(chatId, TimeOnly.Parse(time));
            await botClient.SendTextMessageAsync(chatId, "Напишіть своє ім'я з групою по прикладу. \n Приклад : ЛА-11 Павлюк Олександр");
        }
        private static async Task FullNameCommandHandler(ITelegramBotClient botClient, long chatId, string name)
        {
            await Repository.SaveNameForChatAsync(chatId, name);
            await botClient.SendTextMessageAsync(chatId, "Дякуємо, ви успішно записані. &#128076;", parseMode: ParseMode.Html);
            await StartCommandHandler(botClient, chatId);
        }
        private static async Task AdminCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            if(CheckAdminChatId(chatId))
            {
                await botClient.SendTextMessageAsync(chatId, "Доброго дня, Іван Іванович. Раді вас бачити &#129332;", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>()
                            {
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Переглянути список бронювань", "list")
                                        },
                            new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithUrl("Кампус", "https://ecampus.kpi.ua/home")
                                        }
                            }));
            }
            else await StartCommandHandler(botClient, chatId);
        }
        private static async Task ListCommandHandler(ITelegramBotClient botClient, long chatId)
        {
            var list = await Repository.GetGroupedByDateListAsync(chatId);
            await botClient.SendTextMessageAsync(chatId, list, parseMode:ParseMode.Html);
        }
        private static bool CheckAdminChatId(long chatId)
        {
            if(chatId == 873742312) 
                return true;
            else return false;
        }
    }
}
