using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobberTelegramBot.Models
{
    public class JobSearcherModel
    {
        public int totalCount { get; set; }
        public List<VacancyModel> jobs { get; set; }
        public JobSearcherModel()
        { jobs = new List<VacancyModel>(); }
    }
}
