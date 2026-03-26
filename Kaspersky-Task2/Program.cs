using Kaspersky_Task2.Cli;
using Kaspersky_Task2.Cli.Commands;
using Kaspersky_Task2.Core.Api;
using Kaspersky_Task2.Core.Downloads;
using Kaspersky_Task2.Core.Errors;
using Kaspersky_Task2.Options;

var options = ClientOptions.FromEnvironmentAndArgs(args);

var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute)
};

var errorMapper = new BackendErrorMapper();
var downloader = new ArchiveDownloader();
var backendApi = new BackendApiClient(httpClient, options, errorMapper, downloader);

var dispatcher = new CommandDispatcher(new IClientCommand[]
{
    new ListFilesCommand(backendApi),
    new CreateArchiveCommand(backendApi),
    new StatusCommand(backendApi),
    new DownloadCommand(backendApi),
    new CreateAndDownloadArchiveCommand(backendApi, options)
});

var interactiveShell = new InteractiveShell(dispatcher);
var app = new ClientApp(interactiveShell, dispatcher);
var utilityShell = new UtilityShell(app);

if (args.Length == 0)
{
    var utilityExitCode = await utilityShell.RunAsync(CancellationToken.None);
    Environment.ExitCode = utilityExitCode;
    return;
}

var exitCode = await app.RunPosixAsync(args, CancellationToken.None);
Environment.ExitCode = exitCode;
