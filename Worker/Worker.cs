using Worker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

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
        string prLink = "";
        string prTitle = "";
        MessageBody messageCard = null;

        var keys = Environment.GetEnvironmentVariables().Keys;
        Console.WriteLine("ENV-----------------------------------------------------------------");

        foreach (var k in keys)
        {
            var value = Environment.GetEnvironmentVariable(k?.ToString());
            Console.WriteLine($"[{k?.ToString()}] Value:[{value}]");
        }

        Console.WriteLine("END-----------------------------------------------------------------");


        var REPOSITORY_NAME = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") ?? "";
        var WORKFLOW_NAME = Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") ?? "";
        var GITHUB_EVENT = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") ?? "";
        var BRANCH_NAME = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF") ?? "";
        if (string.IsNullOrWhiteSpace(BRANCH_NAME))
        {
            BRANCH_NAME = Environment.GetEnvironmentVariable("GITHUB_REF_NAME") ?? "";
        }
        var GITHUB_RUN_ID = Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "";

        var INPUT_WEBHOOK = Environment.GetEnvironmentVariable("INPUT_WEBHOOK") ?? "";
        var INPUT_WORKFLOW = Environment.GetEnvironmentVariable("INPUT_WORKFLOW") ?? "";
        var TITLE_OVERRIDE = Environment.GetEnvironmentVariable("INPUT_TITLE_OVERRIDE") ?? "";
        var DESCRIPTION = Environment.GetEnvironmentVariable("INPUT_DESCRIPTION") ?? "";
        var STEPS = Environment.GetEnvironmentVariable("INPUT_STEPS") ?? "";
        var JOB = Environment.GetEnvironmentVariable("INPUT_JOB") ?? "";
        var NEEDS = Environment.GetEnvironmentVariable("INPUT_NEEDS") ?? "";

        var beforeCommit = "";
        var afterCommit = "";

        try
        {
            if (!string.IsNullOrWhiteSpace(INPUT_WORKFLOW))
            {
                var workflowObj = JObject.Parse(INPUT_WORKFLOW);

                if (string.IsNullOrWhiteSpace(BRANCH_NAME))
                {
                    BRANCH_NAME = workflowObj?["ref"]?.ToString() ?? "";
                }

                var e = workflowObj?["event"];
                if (e != null)
                {
                    var compare = e?["compare"]?.ToString();
                    if (compare != null)
                    {
                        compareLink = compare;
                    }
                    else
                    {
                        beforeCommit = e?["before"]?.ToString() ?? "";
                        afterCommit = e?["after"]?.ToString() ?? "";
                    }

                    prLink = e?["pull_request"]?["_links"]?["html"]?["href"]?.ToString() ?? "";
                    prTitle = e?["pull_request"]?["title"]?.ToString() ?? "";

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

                    var repoLink = e?["repository"]?["html_url"] ?? "";
                    if (repoLink != null && !string.IsNullOrWhiteSpace(repoLink.ToString()))
                    {
                        repositoryLink = repoLink.ToString();
                    }
                }
            }


            if (string.IsNullOrWhiteSpace(repositoryLink))
            {
                repositoryLink = $"https://github.com/{REPOSITORY_NAME}";
            }
            if (string.IsNullOrWhiteSpace(compareLink) && !string.IsNullOrWhiteSpace(beforeCommit) && !string.IsNullOrWhiteSpace(afterCommit))
            {
                compareLink = $"{repositoryLink}/compare/{beforeCommit}...{afterCommit}";
            }
            var jobRunWasSuccessful = false;
            if (!string.IsNullOrWhiteSpace(JOB))
            {
                var jobObj = JObject.Parse(JOB);
                jobRunWasSuccessful = ((jobObj["status"]?.ToString() ?? "") == "success");
            }
            string formattedSteps = "";
            if (!jobRunWasSuccessful && !string.IsNullOrWhiteSpace(STEPS) && STEPS.Count() > 5)
            {
                var stepsObj = JObject.Parse(STEPS);
                foreach (var o in stepsObj)
                {
                    var value = o.Value?["outcome"]?.ToString() ?? "";
                    formattedSteps = formattedSteps + "  \n" + o.Key + " - `" + value + "`";
                }
            }

            var formattedDesc = DESCRIPTION;
            if (commits != null && commits.Count > 0)
            {
                formattedDesc = formattedDesc + "  \n  " + "**Commits:**";
                int MAX_COMMITS = 10;
                for (int i = 0; i < MAX_COMMITS && i < commits.Count; i++)
                {
                    var currentCommit = commits[i];
                    formattedDesc = formattedDesc + "\n+ " + $"[{currentCommit.Author} - {currentCommit.Message}]({currentCommit.UrlLink})";
                }
                if (commits.Count > MAX_COMMITS)
                {
                    formattedDesc = formattedDesc + "\n+ " + $"{(commits.Count - MAX_COMMITS)} more...";
                }
            }

            messageCard = new MessageBody()
            {
                Title = !string.IsNullOrWhiteSpace(TITLE_OVERRIDE) ? TITLE_OVERRIDE : $"[{REPOSITORY_NAME}] - [{WORKFLOW_NAME}]",
                Text = formattedDesc,
                ThemeColor = jobRunWasSuccessful ? CardColours.GOOD : CardColours.DANGER,
                Sections = new List<MessageBody.Section>(){
                            new MessageBody.Section(){
                                Facts = new List<MessageBody.Fact>(){
                                    new MessageBody.Fact(){Name="Result", Value=jobRunWasSuccessful ? "PASSED" : "FAILED"},
                                    new MessageBody.Fact(){Name="Repository", Value=REPOSITORY_NAME},
                                    new MessageBody.Fact(){Name="Branch", Value=BRANCH_NAME},
                                    new MessageBody.Fact(){Name="Event", Value=GITHUB_EVENT},
                                }
                            }
                        },
                Actions = new List<MessageBody.Action>(){
                            new MessageBody.Action(){DisplayName= "Repository", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=repositoryLink}}},
                            new MessageBody.Action(){DisplayName= "Compare", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=compareLink}}},
                            new MessageBody.Action(){DisplayName= "Pull Request", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=prLink}}},
                            new MessageBody.Action(){DisplayName= "Workflow", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=$"{repositoryLink}/actions/{(!string.IsNullOrWhiteSpace(GITHUB_RUN_ID) ? $"runs/{GITHUB_RUN_ID}" : "")}"}}}
                        }
            };
            if (!string.IsNullOrWhiteSpace(formattedSteps))
            {
                messageCard.Sections[0].Facts.Add(new MessageBody.Fact()
                {
                    Name = "Steps",
                    Value = formattedSteps
                });
            }
            if (!string.IsNullOrWhiteSpace(prTitle))
            {
                messageCard.Sections[0].Facts.Add(new MessageBody.Fact()
                {
                    Name = "PR",
                    Value = prTitle
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed constructing MessageCard for notification - sending basic card instead");
            _logger.LogError(ex.ToString());
        }


        try
        {
            if (messageCard == null)
            {
                messageCard = new MessageBody()
                {
                    Title = !string.IsNullOrWhiteSpace(TITLE_OVERRIDE) ? TITLE_OVERRIDE : $"[{REPOSITORY_NAME}] - [{WORKFLOW_NAME}]",
                    Text = DESCRIPTION,
                    ThemeColor = CardColours.WARNING,
                    Sections = new List<MessageBody.Section>(){
                            new MessageBody.Section(){
                                Facts = new List<MessageBody.Fact>(){
                                    new MessageBody.Fact(){Name="Result", Value=JOB},
                                    new MessageBody.Fact(){Name="Repository", Value=REPOSITORY_NAME},
                                    new MessageBody.Fact(){Name="Branch", Value=BRANCH_NAME},
                                    new MessageBody.Fact(){Name="Event", Value=GITHUB_EVENT},
                                }
                            }
                        },
                    Actions = new List<MessageBody.Action>(){
                            new MessageBody.Action(){DisplayName= "Repository", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=$"https://github.com/{REPOSITORY_NAME}"}}},
                            new MessageBody.Action(){DisplayName= "Compare", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=compareLink}}},
                            new MessageBody.Action(){DisplayName= "Pull Request", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=prLink}}},
                            new MessageBody.Action(){DisplayName= "Workflow", Targets = new List<MessageBody.ActionTarget>(){ new MessageBody.ActionTarget() {UriLink=$"https://github.com/{REPOSITORY_NAME}/actions/{(!string.IsNullOrWhiteSpace(GITHUB_RUN_ID) ? $"runs/{GITHUB_RUN_ID}" : "")}"}}}
                        }
                };
            }
            var cardJson = JsonConvert.SerializeObject(messageCard);
            _logger.LogDebug("Sending notification for test [{title}]. Full Card: [{details}]", messageCard.Title, cardJson);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            StringContent httpContent = new StringContent(cardJson, System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync(INPUT_WEBHOOK, httpContent);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send failure notification");
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}

