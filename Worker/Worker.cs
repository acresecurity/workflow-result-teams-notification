using Worker.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var variables = Environment.GetEnvironmentVariables();
        var keys = Environment.GetEnvironmentVariables().Keys;
        foreach (var k in keys)
        {
            var value = Environment.GetEnvironmentVariable(k?.ToString());
            Console.WriteLine($"[{k?.ToString()}] Value:[{value}]");
        }
        // MessageBody messageCard = new MessageBody()
        // {
        //     Title = "Feenics WatchDog Alert",
        //     Text = $"[] Notification",
        //     Sections = new List<MessageBody.Section>(){
        //                     new MessageBody.Section(){
        //                         Facts = new List<MessageBody.Fact>(){}
        //                     }
        //                 },
        //     Actions = new List<MessageBody.Action>(){
        //                     new MessageBody.Action(){DisplayName= "Repository", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=""}}},
        //                     new MessageBody.Action(){DisplayName= "Compare", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=""}}},
        //                     new MessageBody.Action(){DisplayName= "Workflow", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=""}}}
        //                 }
        // };

        // try
        // {
        //     var cardJson = JsonConvert.SerializeObject(messageCard);
        //     _logger.LogDebug("Sending notification for test [{title}]. Full Card: [{details}]", messageCard.Title, cardJson);
        //     HttpClient client = new HttpClient();
        //     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //     StringContent httpContent = new StringContent(cardJson, System.Text.Encoding.UTF8, "application/json");

        //     await client.PostAsync("", httpContent);

        // }
        // catch (Exception e)
        // {
        //     _logger.LogError(e, "Failed to send failure notification");
        // }
        _applicationLifetime.StopApplication();
    }
}
