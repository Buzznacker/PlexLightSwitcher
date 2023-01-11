using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexLightSwitcher.Client;
using PlexLightSwitcher.Config;
using PlexLightSwitcher.Models;
using System.Collections.Concurrent;

namespace PlexLightSwitcher.Worker
{
    public class APIWorker : BackgroundService
    {
        private readonly ConcurrentQueue<PlexMessage> _queue = new ConcurrentQueue<PlexMessage>();
        private readonly ILogger<APIWorker> _logger;

        public APIWorker(ILogger<APIWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = FetchConfig();
            var neviWebClient = null as NeviWebClient;

            if (config == null)
            {
                _logger.LogError("The configuration file is not setup correctly, this application will not work correctly");
            }

            try
            {
                neviWebClient = new NeviWebClient(_logger, config);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "");
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if there are any messages in the queue
                    if (neviWebClient != null && config != null  && _queue.TryDequeue(out var message))
                    {
                        // Process the message
                        var str = JsonConvert.SerializeObject(message, Formatting.Indented);
                        _logger.LogInformation($"Processing message: {str}");
                        ProcessMessage(neviWebClient, message, config);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                }
                // Wait for a short time before checking the queue again
                await Task.Delay(100, stoppingToken);
            }
        }

        public void AddMessage(PlexMessage message)
        {
            // Add a message to the queue
            _queue.Enqueue(message);
        }

        private void ProcessMessage(NeviWebClient webClient, PlexMessage message, SwitcherConfiguration config)
        {
            if (message.Player.Title == config.PlayerName)
            {
                var sceneName = "";
                var passed = true;

                switch (message.Event)
                {
                    case "media.play":
                    case "media.resume":
                        if (message.Metadata.Type == "episode")
                            sceneName = config.EventsToScenes.MediaPlayEpisode;
                        else
                            sceneName = config.EventsToScenes.MediaPlayMovie;
                        break;
                    case "media.stop":
                    case "media.pause":
                        sceneName = config.EventsToScenes.MediaStop;
                        break;
                    default:
                        passed = false;
                        break;
                }

                if (!passed)
                {
                    return;
                }

                var scene = webClient.Scenes.FirstOrDefault(s => s.Name == sceneName);

                if (scene == null)
                {
                    throw new Exception($"The scene ({sceneName}) has not been loaded by the APIClient, try restarting the application");
                }

                foreach (var action in scene.Actions)
                {
                    if (action.AttributeName != "intensity")
                    {
                        _logger.LogWarning($"the {sceneName} scene has an unsupported action: {action.AttributeName} skipping this action");
                        continue;
                    }
                    webClient.SetDeviceIntensity(action.DeviceId, int.Parse(action.AttributeValue));
                }
            }
        }

        private SwitcherConfiguration? FetchConfig()
        {
            try
            {
                var json = File.ReadAllText("swtichersettings.json");
                return JsonConvert.DeserializeObject<SwitcherConfiguration>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
            return null;
        }
    }
}
