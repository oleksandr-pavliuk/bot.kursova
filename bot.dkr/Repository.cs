using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.dkr
{
    public static class Repository
    {
        public static async Task<List<InlineKeyboardButton[]>> CheckDateKeyboard(string dateMessage)
        {
            using var context = new ApplicationContext();
            var times = new List<TimeOnly?>() { TimeOnly.Parse("16:30:00"), TimeOnly.Parse("17:00:00"), TimeOnly.Parse("17:30:00"), TimeOnly.Parse("18:00:00") };
            var timeKeyboardList = new List<InlineKeyboardButton[]>();
            var studentsTime = context.Students.Where(s => s.Date == DateOnly.Parse(dateMessage.Remove(0, 5))).Select(x => x.Time).ToList();

            foreach (var time in studentsTime)
            {
                times.Remove(time);
            }

            foreach (var time in times)
            {
                timeKeyboardList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{time?.ToString("HH:mm:ss")}", $"time {time?.ToString("HH:mm:ss")}") });
            }

            return timeKeyboardList;
        }
        public static async Task SaveDateForChatAsync(long chatId, DateOnly date)
        {
            using var context = new ApplicationContext();
            var student = await context.Students.Where(s => s.ChatId == chatId).FirstAsync();
            if (student == null)
            {
                await context.Students.AddAsync(new Student { ChatId = chatId });
            }
            student.Date = date;
            await context.SaveChangesAsync();
        }
        public static async Task SaveNewStudentAsync(long chatId)
        {
            using var context = new ApplicationContext();
            if (context.Students.Where(s => s.ChatId == chatId).FirstOrDefault() == null)
                context.Add(new Student { ChatId = chatId });

            await context.SaveChangesAsync();
        }
        public static async Task SaveTimeForChatAsync(long chatId, TimeOnly? time)
        {
            using var context = new ApplicationContext();
            context.Students.Where(s => s.ChatId == chatId).FirstAsync().Result.Time = time;
            await context.SaveChangesAsync();
        }
        public static async Task SaveNameForChatAsync(long chatId, string name)
        {
            using var context = new ApplicationContext();
            var student = await context.Students.Where(s => s.ChatId == chatId).FirstAsync();
            student.FullName = name;
            student.Completed = true;
            await context.SaveChangesAsync();
        }
        public static async Task<bool> CancelForChatAsync(long chatId)
        {
            using var context = new ApplicationContext();
            var student = await context.Students.FirstOrDefaultAsync(s => s.ChatId == chatId);
            if (student is null)
            {
                return false;
            }
            context.Students.Remove(student);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<string> GetGroupedByDateListAsync(long chatId)
        {
            using var context = new ApplicationContext();
            var list = context.Students.Where(s => s.Completed == true).ToListAsync().Result.GroupBy(s => s.Date);
            StringBuilder result = new StringBuilder();
            foreach (var date in list)
            {
                result.AppendLine("<b>"+date.Key.ToString()+ "</b> &#128198;");
                result.AppendLine();
                foreach (var student in date.OrderBy(s => s.Time))
                {
                    result.AppendLine(student.Time + " ---> " +student.FullName);
                }
                result.AppendLine();
                result.AppendLine();
            }
            return result.ToString();
        }
        public static async Task SetReminderForChatAsync(long chatId)
        {
            using var context = new ApplicationContext();
            var student = await context.Students.FirstOrDefaultAsync(s => s.ChatId == chatId);
            student.isReminder = true;
            await context.SaveChangesAsync();
        }
        public static async Task<DateTimeOffset> GetDateTimeForReminder(long chatId)
        {
            DateTime time = DateTime.Now;
            using var context = new ApplicationContext();
            var student = await context.Students.Where(s => s.ChatId == chatId).FirstAsync();
            if (student.isReminder)
            {
                time = DateTime.Parse($"{student.Date} {student.Time}");
            }
            return time;
        }
        internal static async Task<List<long>> GetChatsForRemind(string date)
        {
            using var context = new ApplicationContext();
            List<long> chatIds = await context.Students.Where(s => s.Date == DateOnly.Parse(date) && s.isReminder && s.Completed).Select(s => s.ChatId).ToListAsync();
            return chatIds;
        }

        internal static async Task SetReminderDone(long chat)
        {
            using var context = new ApplicationContext();
            var student = context.Students.FirstOrDefault(s => s.ChatId == chat);
            student.isReminder = false;
            await context.SaveChangesAsync();
        }

        public static async Task<TimeOnly?> GetTimeForChatAsync(long chatId)
        {
            using var context = new ApplicationContext();
            return await context.Students.Where(s => s.ChatId == chatId).Select(s => s.Time).FirstOrDefaultAsync();
        }
    }
}
