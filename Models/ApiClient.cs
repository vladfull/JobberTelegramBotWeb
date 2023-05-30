using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JobberTelegramBot.Models
{
    public class ApiClient
    {
        private HttpClient _httpClient;
        public ApiClient()
        {
            _httpClient = new HttpClient();
        }
        public async void CreateUser(long chatId, string username, string profession, string salary, string location)
        {
            await _httpClient.PostAsync($"https://jobberwebapi.azurewebsites.net/api/UsersData/PostUserDataAsync?chatId={chatId}&name={username}&prof={profession}&salary={salary}&location={location}", null);
        }
        public async void DeleteUser(long chatId)
        {
            await _httpClient.DeleteAsync($"https://jobberwebapi.azurewebsites.net/api/UsersData/DeleteUserDataAsync?chatId={chatId}");
        }
        public async Task<List<UserData>> GetUser(long chatId)
        {
            var result = await _httpClient.GetAsync($"https://jobberwebapi.azurewebsites.net/api/UsersData/GetUserDataAsync?chatId={chatId}");
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserData>>(content);
        }
        public async Task<JobSearcherModel> GetVacancy(long chatId) 
        {
            var prof = GetUser(chatId).Result;
            string keywords = $"{prof[0].UserProffesion}";
            var response = await _httpClient.PostAsync($"https://jobberwebapi.azurewebsites.net/api/VacancySearch?keywords={keywords}&location={prof[0].UserJobLocation}", null);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<JobSearcherModel>(response.Content.ReadAsStringAsync().Result);
        } 
        public async Task GetResumeFile(ITelegramBotClient botClient, long chatId)
        {
            var prof = GetUser(chatId).Result;
            string textukr = $"Напиши резюме на роботу {prof[0].UserProffesion}. Мене звати {prof[0].UserName}. " +
                $"Претендую на заробітню плату {prof[0].UserSalary} гривень. Хочу працювати в {prof[0].UserJobLocation}";
            string texteng = $"Write a resume for the job of a {prof[0].UserProffesion}. My name is {prof[0].UserName}. " +
                $"I apply for a salary of UAH {prof[0].UserSalary}. I want to work in {prof[0].UserJobLocation}.";
            var response = await _httpClient.GetAsync($"https://jobberwebapi.azurewebsites.net/api/Resume/GetResumeAsync?question={texteng}");
            response.EnsureSuccessStatusCode();
            var memorystream = await response.Content.ReadAsStreamAsync();   
            Console.WriteLine(memorystream);
                    
            await botClient.SendDocumentAsync(chatId, InputFile.FromStream(memorystream, fileName: "resume.pdf"));
                
            
        }

    }
}
