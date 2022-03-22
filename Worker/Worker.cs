using Worker.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        string compareLink = "";
        List<Commit> commits = new List<Commit>();
        string repositoryLink = "";


        var variables = Environment.GetEnvironmentVariables();
        var keys = Environment.GetEnvironmentVariables().Keys;
        // Console.WriteLine("ENV-----------------------------------------------------------------");

        // foreach (var k in keys)
        // {
        //     var value = Environment.GetEnvironmentVariable(k?.ToString());
        //     Console.WriteLine($"[{k?.ToString()}] Value:[{value}]");
        // }

        // Console.WriteLine("END-----------------------------------------------------------------");

        var workflow = Environment.GetEnvironmentVariable("INPUT_WORKFLOW") ?? "";
        if (!string.IsNullOrWhiteSpace(workflow))
        {
            var workflowObj = JObject.Parse(workflow);
            var compare = workflowObj?["compare"]?.ToString();
            if (compare != null)
            {
                compareLink = compare;
            }

            var e = workflowObj?["event"];
            if (e != null)
            {
                var c = e?["commits"];
                var commitsArr = c as JArray;
                if (commitsArr != null && commitsArr.Count > 0)
                {
                    foreach (var v in commitsArr)
                    {
                        var author = v["author"]?["username"]?.ToString();
                        var msg = v["message"]?.ToString();
                        var url = v["url"]?.ToString();
                        commits.Add(new Commit()
                        {
                            Author = author ?? "",
                            Message = msg ?? "",
                            UrlLink = url ?? ""
                        });
                    }
                }

                var repoLink = e?["repository"]?["url"] ?? "";
                if (repoLink != null && !string.IsNullOrWhiteSpace(repoLink.ToString()))
                {
                    repositoryLink = repoLink.ToString();
                }
            }


        }
        var TITLE = Environment.GetEnvironmentVariable("INPUT_TITLE") ?? "";
        var DESCRIPTION = Environment.GetEnvironmentVariable("INPUT_DESCRIPTION") ?? "";
        var REPOSITORY_NAME = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") ?? "";
        var WORKFLOW_NAME = Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") ?? "";
        var STEPS = Environment.GetEnvironmentVariable("INPUT_STEPS") ?? "";
        var JOB = Environment.GetEnvironmentVariable("INPUT_JOB") ?? "";
        var NEEDS = Environment.GetEnvironmentVariable("INPUT_NEEDS") ?? "";
        var GITHUB_EVENT = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") ?? "";
        var BRANCH_NAME = Environment.GetEnvironmentVariable("GITHUB_REF_NAME") ?? "";


        if (string.IsNullOrWhiteSpace(repositoryLink))
        {
            repositoryLink = $"https://github.com/{REPOSITORY_NAME}";
        }

        Console.WriteLine($"[BRANCH_NAME] Value:[{BRANCH_NAME}]");

        Console.WriteLine($"[compareLink] Value:[{compareLink}]");
        Console.WriteLine($"[repositoryLink] Value:[{repositoryLink}]");

        Console.WriteLine($"[commits] Value:[{JsonConvert.SerializeObject(commits)}]");
        Console.WriteLine($"[TITLE] Value:[{TITLE}]");
        Console.WriteLine($"[DESCRIPTION] Value:[{DESCRIPTION}]");
        Console.WriteLine($"[REPOSITORY_NAME] Value:[{REPOSITORY_NAME}]");
        Console.WriteLine($"[WORKFLOW_NAME] Value:[{WORKFLOW_NAME}]");
        Console.WriteLine($"[GITHUB_EVENT] Value:[{GITHUB_EVENT}]");

        Console.WriteLine($"[STEPS] Value:[{STEPS}]");
        Console.WriteLine($"[JOB] Value:[{JOB}]");

        Console.WriteLine($"[NEEDS] Value:[{NEEDS}]");









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
