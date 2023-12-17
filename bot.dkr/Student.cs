using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.dkr
{
    public class Student
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public string? FullName { get; set; }
        public bool Completed { get; set; } = false;
        public bool isReminder { get; set; } = false;
        public bool isAdmin { get; set; } = false;
    }
}
