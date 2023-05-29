//using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JobberTelegramBot.Models
{
    public class UserData
    {
        
        public long ChatId { get; set; }
        public string UserName { get; set; }
        public string UserProffesion { get; set; }
        public string UserSalary { get; set; }
        public string UserJobLocation { get; set; }
        //public UserResumeFile UserResumeFile { get; set; }
        //public UserData(byte[] filebytes) 
        //{
        //    UserResumeFile = new UserResumeFile(filebytes);
        //}
    }
}
