using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using JobberTelegramBot.Models;
using System.Reflection.Metadata.Ecma335;


internal class Program
{
    static Dictionary<long, int> i = new Dictionary<long, int>();
    static Dictionary<long, int> step = new Dictionary<long, int>();
    static Profile profile = new Profile();  
    static ApiClient apiClient = new ApiClient();
    private static void Main(string[] args)
    {
       
            var botClient = new TelegramBotClient("6072918501:AAFjEe8uDSrlsNathb6riqxnksKQwp6UiSo");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            botClient.StartReceiving
                (
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
                );
            Console.WriteLine("Запущений бот " + botClient.GetMeAsync().Result.FirstName);
            //Console.ReadLine();
            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellation)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException => $"Помилка в API:\n {apiRequestException.ErrorCode}" +
                    $"\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };
                return Task.CompletedTask;
            }

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
            try
            {

                if (update.Type == UpdateType.Message && update.Message.Text != null)
                {
                    await HandlerMessage(botClient, update.Message);
                }

                if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); return;
            }
            return;
            }

        async Task HandlerMessage(ITelegramBotClient botClient, Message message)
        {
            await Console.Out.WriteLineAsync(message.Text);
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Привіт! Я знаю, що ти найкращий працівник у всьому Всесвіті, " +
                "але кляті роботодавці не помічають тебе, і тому я хочу тобі допомогти. Щоб розпочати роботу з ботом, введи команду /registrate");
                step.TryAdd(message.Chat.Id, 0);
            }
            if (message.Text == "/menu")
            {
                InlineKeyboardMarkup menu =
                new(new[]
                {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Профіль", callbackData: "profile"),
                            InlineKeyboardButton.WithCallbackData(text: "Вакансії", callbackData: "vacancy")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Резюме", callbackData: "resume")
                        }
                });

                await botClient.SendTextMessageAsync(message.Chat.Id, "Що Ви хочете зробити?", replyMarkup: menu);
            }

            try
            {
                if (message.Text == "/registrate" & CheckProfile(message.Chat.Id) == false & (step.TryAdd(message.Chat.Id, 0) | step[message.Chat.Id] == 0))
                {
                    step.TryAdd(message.Chat.Id, 0);
                    step[message.Chat.Id]++;
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть Ваше ім'я");
                }
                else if (message.Text == "/registrate" & CheckProfile(message.Chat.Id) == true)
                {

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш профіль уже зареєстровано");
                }
                else if (step[message.Chat.Id] == 1 & !message.Text.StartsWith("/"))
                {
                    step[message.Chat.Id]++;
                    profile.Name = message.Text;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Добре, {message.Text}. Введіть професію, спеціальність, за якою Ви бажаєте знайти роботу");
                }
                else if (step[message.Chat.Id] == 2 & !message.Text.StartsWith("/"))
                {
                    step[message.Chat.Id]++;
                    profile.Profession = message.Text;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введіть Вашу бажану заробітню плату (Введіть лише число)");
                }
                else if (step[message.Chat.Id] == 3 & int.TryParse(message.Text, out int result) & !message.Text.StartsWith("/"))
                {
                    step[message.Chat.Id]++;
                    profile.Salary = message.Text;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введіть локацію, де Ви бажаєте працювати");
                }
                else if (step[message.Chat.Id] == 3 & int.TryParse(message.Text, out int res) == false)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Потрібно було ввести число. Спробуйте ще раз");
                }
                else if (step[message.Chat.Id] == 4 & !message.Text.StartsWith("/"))
                {
                    profile.Location = message.Text;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Реєстрацію завершено! Щоб відкрити робочу панель введіть команду /menu");
                    apiClient.CreateUser(message.Chat.Id, profile.Name, profile.Profession, profile.Salary, profile.Location);
                    step[message.Chat.Id] = 0;
                    await Console.Out.WriteLineAsync("User registrated");
                }
                else if (step[message.Chat.Id] == 1 | step[message.Chat.Id] == 2
                    | step[message.Chat.Id] == 3 | step[message.Chat.Id] == 4 & message.Text.StartsWith("/"))
                {
                    step[message.Chat.Id] = 0;
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Реєстрацію провалено. Спробуйте пройти реєстрацію ще раз /registrate");
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }
        async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {

            if (callbackQuery.Data.Equals("profile") & CheckProfile(callbackQuery.Message.Chat.Id) == true)
            {
                InlineKeyboardMarkup profMenu =
                new(new[]
                {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Видалити", callbackData: "deleteProf"),
                            InlineKeyboardButton.WithCallbackData(text: "Меню", callbackData: "menu")

                        },

                });
                var response = await apiClient.GetUser(callbackQuery.Message.Chat.Id);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Ваш профіль:\nІм'я: {response[0].UserName}\nПрофесія: {response[0].UserProffesion}\n" +
                    $"Зарплата: {response[0].UserSalary}\nЛокація: {response[0].UserJobLocation}", replyMarkup: profMenu);

                return;
            }
            if (callbackQuery.Data.Equals("deleteProf"))
            {
                step.Remove(callbackQuery.Message.Chat.Id);
                apiClient.DeleteUser(callbackQuery.Message.Chat.Id);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ваш профіль було видалено. Для подальшої роботи з ботом слід пройти" +
                    " реєстрацію заново /registrate");
            }
            if (callbackQuery.Data.Equals("menu"))
            {
                InlineKeyboardMarkup menu =
                new(new[]
                {
                      new[]
                      {
                            InlineKeyboardButton.WithCallbackData(text: "Профіль", callbackData: "profile"),
                            InlineKeyboardButton.WithCallbackData(text: "Вакансії", callbackData: "vacancy")
                      },
                      new[]
                      {
                            InlineKeyboardButton.WithCallbackData(text: "Резюме", callbackData: "resume")
                      }
                });
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Що Ви хочете зробити?", replyMarkup: menu);

            }
            if (callbackQuery.Data.Equals("vacancy") & CheckProfile(callbackQuery.Message.Chat.Id) == true)
            {
                i.TryAdd(callbackQuery.Message.Chat.Id, 0);
                var response = await apiClient.GetVacancy(callbackQuery.Message.Chat.Id);
                if (response.jobs.Count != 0)
                {


                    InlineKeyboardMarkup vacancyMenu =
                    new(new[]
                    {
                      new[]
                      {
                            InlineKeyboardButton.WithCallbackData(text: "Далі", callbackData: "next"),
                            InlineKeyboardButton.WithCallbackData(text: "Меню", callbackData: "menu")
                      },
                      new[]
                      {
                          InlineKeyboardButton.WithUrl(text: "Перейти", url: $"{response.jobs[0].link}")
                      }
                    });
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                        $"<b>{response.jobs[0].title}</b>\n" +
                        $"<i>Країна/Місто:</i> {response.jobs[0].location}\n" +
                        $"<i>Зарплата:</i> {response.jobs[0].salary}\n" +
                        $"<i>Тип:</i> {response.jobs[0].type}\n" +
                        $"<i>Компанія:</i> {response.jobs[0].company}\n" +
                        $"<i>Опис:</i> {response.jobs[0].snippet}\n" +
                        $"<i>Дата:</i> {response.jobs[0].updated}", parseMode: ParseMode.Html, replyMarkup: vacancyMenu);
                } else await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "На даний момент більше немає вакансій за даними Вашого профілю /menu");
            }
            if (callbackQuery.Data.Equals("next"))
            {
                i.TryAdd(callbackQuery.Message.Chat.Id, 0);
                var response = await apiClient.GetVacancy(callbackQuery.Message.Chat.Id);
                
                InlineKeyboardMarkup vacancyMenu =
                new(new[]
                {
                      new[]
                      {
                            InlineKeyboardButton.WithCallbackData(text: "Далі", callbackData: "next"),
                            InlineKeyboardButton.WithCallbackData(text: "Меню", callbackData: "menu")
                      },
                      new[]
                      {
                          InlineKeyboardButton.WithUrl(text: "Перейти", url: $"{response.jobs[i[callbackQuery.Message.Chat.Id]].link}")
                      }
                });

                if (i[callbackQuery.Message.Chat.Id] >= response.jobs.Count - 1)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "На даний момент більше немає вакансій за даними Вашого профілю /menu"); i[callbackQuery.Message.Chat.Id] = 0;
                }
                else
                {
                    i[callbackQuery.Message.Chat.Id]++;
                    
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                    $"<b>{response.jobs[i[callbackQuery.Message.Chat.Id]].title}</b>\n" +
                    $"<i>Країна/Місто:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].location}\n" +
                    $"<i>Зарплата:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].salary}\n" +
                    $"<i>Тип:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].type}\n" +
                    $"<i>Компанія:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].company}\n" +
                    $"<i>Опис:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].snippet}\n" +
                    $"<i>Дата:</i> {response.jobs[i[callbackQuery.Message.Chat.Id]].updated}", parseMode: ParseMode.Html, replyMarkup: vacancyMenu);
                }
                


            }
            if (callbackQuery.Data.Equals("resume") & CheckProfile(callbackQuery.Message.Chat.Id) == true)
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Зараз Штучний Інтелект спробує скласти резюме за інформацією " +
                    "Вашого профілю(/menu). \n\n<b>Важливо!</b> Штучний інтелект створює резюме за даними профілю, введеними Вами при реєстрації, тому варто перевірити їх правильність i коректність." +
                    " Це резюме є лише прикладом можливого реального резюме.", parseMode: ParseMode.Html);
                await apiClient.GetResumeFile(botClient, callbackQuery.Message.Chat.Id);
            }
        }
        
        bool CheckProfile(long chatId)
        {
                //var check = await apiClient.GetUser(chatId);
                //if (apiClient.GetUser(chatId).Result[0].ChatId == chatId)
                //{
                //    Console.WriteLine("Працює");
                //    return true;
                //}
                if (apiClient.GetUser(chatId).Result.Count != 0) return true;
                return false;
        }

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
        //Console.ReadLine();
    }

}
public class Profile
{
    public string Name { get; set; }
    public string Profession { get; set; }
    public string Salary { get; set; }
    public string Location { get; set; }
}