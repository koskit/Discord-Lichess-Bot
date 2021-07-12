using Discord;
using Discord.WebSocket;
using DockerHelper;
using LichessApiHelper;
using LichessApiHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordLichessBot
{
    public class Program
    {
        public static void Main() => new Program().MainAsync()
                                                  .GetAwaiter()
                                                  .GetResult();

        private LichessClient _LichessClient;
        private DiscordSocketClient _DiscordClient;
        private bool _IsDiscordConnectedAndReady = false;

        private int _JobSleepDuration;
        private bool _JobReportTeamChanges;
        private string _LichessTeamName;
        private SocketTextChannel _DiscordReportingTextChannel;

        public async Task MainAsync()
        {
            Console.Write("[Info] Discord bot for lichess is starting... ");

            #region [Job Setup]

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("[Info] Beginning job setup... ");

            _JobSleepDuration = DockerEnvironment.GetEnvironmentVariableWithFallback<int>("JOB_ITERATION_SLEEP_DURATION", 30000);
            Console.WriteLine($"[Info] JOB_ITERATION_SLEEP_DURATION={_JobSleepDuration}ms");

            _JobReportTeamChanges = DockerEnvironment.GetEnvironmentVariableWithFallback<bool>("JOB_REPORT_TEAM_CHANGES", true);
            Console.WriteLine($"[Info] JOB_REPORT_TEAM_CHANGES={_JobSleepDuration}ms");

            Console.WriteLine("[Info] Job setup finished.");

            #endregion [Job Setup]

            #region [Lichess Setup]

            Console.WriteLine(Environment.NewLine);
            Console.Write("[Info] Connecting to lichess server... ");

            string lichessPat = DockerEnvironment.GetEnvironmentVariable("LICHESS_PAT");
            _LichessTeamName = DockerEnvironment.GetEnvironmentVariable("LICHESS_TEAM_NAME");

            _LichessClient = new LichessClient(lichessPat);
            try
            {
                List<TeamMember> teamMembers = await _LichessClient.GetTeamMembersAsync(_LichessTeamName);
                Console.WriteLine("connected!");

                if (teamMembers.Count < 1)
                    Console.WriteLine(
                        $"[Warning] Connected to lichess but no team members were found for team \"{_LichessTeamName}\". " +
                        $"No messages will be send to discord until members are added.");
            }
            catch (Exception ex)
            {
                Console.Write("error");
                Console.WriteLine(
                    $"[Error] Application encountered an error while connecting to lichess. " +
                    $"Exception: {GetExceptionMessage(ex)}");
            }

            Console.Write("[Info] Finished Lichess Setup!");
            Console.WriteLine(Environment.NewLine);

            #endregion [Lichess Setup]

            #region [Discord Setup]

            Console.WriteLine(Environment.NewLine);
            Console.Write($"[Info] Connecting to discord server... ");

            _DiscordClient = new DiscordSocketClient();
            _DiscordClient.Log += DiscordLog;
            _DiscordClient.Ready += DiscordIsReady;

            string discordServerName = DockerEnvironment.GetEnvironmentVariable("DISCORD_SERVER_NAME");
            string discordLichessTextChannelName = DockerEnvironment.GetEnvironmentVariable("DISCORD_SERVER_TEXT_CHANNEL");
            string discordBotToken = DockerEnvironment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

            try
            {
                await _DiscordClient.LoginAsync(TokenType.Bot, discordBotToken);
                await _DiscordClient.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error");
                Console.WriteLine(
                    $"[Error] Application encountered an error while connecting to discord. " +
                    $"Exception: {GetExceptionMessage(ex)}");
            }

            //Wait for discord client to be ready in a horrible manner.
            while (!_IsDiscordConnectedAndReady)
                Thread.Sleep(500);

            Console.WriteLine($"connected!");

            Console.Write($"[Info] Searching for server \"{discordServerName}\"... ");

            SocketGuild discordServer = _DiscordClient.Guilds
                                                      .Where(g => g.Name.ToUpper() == discordServerName.ToUpper())
                                                      .SingleOrDefault();

            if (discordServer == default)
                throw new Exception(
                    $"[Error] Server \"{discordServerName}\" was not found. " +
                    $"Please ensure the name is spelled correctly.");

            Console.WriteLine("found!");

            Console.Write($"[Info] Searching for server's text channel \"{discordLichessTextChannelName}\"... ");


            SocketGuildChannel lichessChannel = discordServer.Channels
                                                             .Where(c => c.Name.ToUpper() == discordLichessTextChannelName.ToUpper())
                                                             .SingleOrDefault();

            if (lichessChannel == default)
                throw new Exception(
                    $"[Error] Text channel \"{discordLichessTextChannelName}\" was not found on server \"{discordServerName}\" " +
                    $"Please ensure the name is spelled correctly.");

            _DiscordReportingTextChannel = discordServer.GetTextChannel(lichessChannel.Id);

            Console.WriteLine("found!");

            Console.Write("[Info] Finished Discord Setup!");
            Console.WriteLine(Environment.NewLine);

            #endregion [Discord Setup]

            await RunDiscordReportingForLichessJob();
        }

        public async Task RunDiscordReportingForLichessJob()
        {
            await _DiscordReportingTextChannel.SendMessageAsync($"Lichess bot is online!");
            await _DiscordReportingTextChannel.SendMessageAsync($"I will be spectating for updates on team: {_LichessTeamName}!");

            List<TeamMember> teamMembers = await _LichessClient.GetTeamMembersAsync(_LichessTeamName);
            string teamPlayers = string.Join(", ", teamMembers.Select(t => t.Username));

            await _DiscordReportingTextChannel.SendMessageAsync($"Current players in team: {teamPlayers}!");

            //Dictionary definition: {TeamMember},{LastStatus}
            Dictionary<TeamMember, bool> teamReports = new Dictionary<TeamMember, bool>();
            foreach (TeamMember member in teamMembers)
            {
                teamReports.Add(member, member.Online);
            }

            do
            {
                Thread.Sleep(_JobSleepDuration);

                try
                {
                    teamMembers = await _LichessClient.GetTeamMembersAsync(_LichessTeamName);

                    //Add new team members.
                    foreach (TeamMember member in teamMembers)
                    {
                        bool isNewPlayer = !teamReports.Select(r => r.Key.Username)
                                                       .Contains(member.Username);

                        if (isNewPlayer)
                        {
                            teamReports.Add(member, member.Online);

                            if (_JobReportTeamChanges)
                                await _DiscordReportingTextChannel.SendMessageAsync(
                                    $"{member.Username} joined the {_LichessTeamName} team!");
                        }
                    }

                    //Find removed members that are not in the team anymore.
                    List<TeamMember> removedMembers = new List<TeamMember>();
                    foreach (TeamMember member in teamReports.Select(r => r.Key))
                    {
                        bool isRemovedMember = !teamMembers.Select(r => r.Username)
                                                           .Contains(member.Username);

                        if (isRemovedMember)
                        {
                            removedMembers.Add(member);

                            if (_JobReportTeamChanges)
                                await _DiscordReportingTextChannel.SendMessageAsync(
                                    $"{member.Username} left the {_LichessTeamName} team!");
                        }
                    }

                    //Remove them from the report dictionary so we no longer keep track of them.
                    removedMembers.ForEach(removedMember => teamReports.Remove(removedMember));

                    //Do the reporting on discord on status changes.
                    foreach (TeamMember member in teamMembers)
                    {
                        KeyValuePair<TeamMember, bool> player = teamReports.Where(r => r.Key.Username == member.Username)
                                                                           .Single();

                        bool wasPlayerConnected = player.Value;

                        //If player just connected (was not online)
                        if (member.Online && !wasPlayerConnected)
                        {
                            //Update the report.
                            teamReports[player.Key] = member.Online;

                            //Announce to discord.
                            await _DiscordReportingTextChannel.SendMessageAsync(
                                $"{player.Key.Username} just connected to Lichess!");
                        }
                        //If player disconnected (was online but not anymore)
                        else if (!member.Online && wasPlayerConnected)
                        {
                            //Update the report.
                            teamReports[player.Key] = member.Online;

                            //Announce to discord.
                            await _DiscordReportingTextChannel.SendMessageAsync(
                                $"{player.Key.Username} just disconnected from Lichess!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Bot encountered error while executing the job. Exception: {GetExceptionMessage(ex)}");
                    return;
                }
            } while (!Environment.HasShutdownStarted);
        }

        #region [Helper Methods]

        private Task DiscordIsReady()
        {
            _IsDiscordConnectedAndReady = true;
            return Task.CompletedTask;
        }

        private Task DiscordLog(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private string GetExceptionMessage(Exception ex)
        {
            return ex.Message +
                (ex.InnerException == null
                ? string.Empty
                : "\r\n" + ex.InnerException.Message);
        }

        #endregion [Helper Methods]
    }
}
