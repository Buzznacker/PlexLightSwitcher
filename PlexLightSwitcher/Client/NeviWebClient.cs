using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using PlexLightSwitcher.Worker;
using RestSharp;
using PlexLightSwitcher.Models;
using PlexLightSwitcher.Requests;
using PlexLightSwitcher.Config;

namespace PlexLightSwitcher.Client
{
    public class NeviWebClient
    {
        private NeviWebUser _lastSession;
        private List<NeviWebLocation> _locations;
        private List<NeviWebScene> _scenes = new List<NeviWebScene>();

        public List<NeviWebScene> Scenes { get { return _scenes; } }

        private readonly ILogger<APIWorker> _logger;
        private readonly RestClient _restClient;
        private readonly SwitcherConfiguration _config;

        public NeviWebClient(ILogger<APIWorker> logger, SwitcherConfiguration config) 
        {
            _logger = logger;
            _config = config;
            _restClient = new RestClient("https://neviweb.com");
            PostConstruct();
        }

        public bool SetDeviceIntensity(int deviceId, int intensity, bool withRetry = true)
        {
            _logger.LogInformation($"ApiClient | SetSceneOn | deviceId: {deviceId}");

            var request = new RestRequest("/api/device/{deviceId}/attribute", Method.Put).AddUrlSegment("deviceId", deviceId);
            var jObject = new JObject
            {
                { "onOff", "on" },
                { "intensity", intensity }
            };

            request.AddParameter("application/json", jObject.ToString(), ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148");
            request.AddHeader("session-id", _lastSession.Session);

            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = response.Content;

                if (string.IsNullOrEmpty(responseContent))
                {
                    throw new Exception($"Couldn't gather Locations, too many requests?\nJson: {response.Content}");
                }

                if (responseContent.Contains("USRSESSEXP"))
                {
                    if(withRetry)
                    {
                        GetSession();
                        return SetDeviceIntensity(deviceId, intensity, false);
                    }
                }

                var responseBody = JObject.Parse(response.Content);
                var serverStatus = responseBody.GetValue("onOff").Value<string>();
                if (serverStatus == "on")
                {
                    _logger.LogInformation($"ApiClient | SetSceneOn | deviceId: {deviceId} | Success");
                    return true;
                }
            }
            throw new Exception($"Unknown error while attempting to set the scene");
        }

        private void GetLocations(bool withRetry = true)
        {
            _logger.LogInformation("APIClient | GetLocations");

            var request = new RestRequest("/api/locations", Method.Get);

            request.AddHeader("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148");
            request.AddHeader("session-id", _lastSession.Session);
            request.AddQueryParameter("account$id", _lastSession.Account.Id);

            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = response.Content;
                
                if(string.IsNullOrEmpty(responseContent))
                {
                    throw new Exception($"Couldn't gather Locations, too many requests?\nJson: {response.Content}");
                }

                if(responseContent.Contains("USRSESSEXP"))
                {
                    GetSession();
                    if(withRetry)
                    {
                        GetLocations(false);
                    }
                    return;
                }

                var jsonArray = JArray.Parse(responseContent);
                var locations = JsonConvert.DeserializeObject<List<NeviWebLocation>>(jsonArray.ToString());
                if (locations == null)
                {
                    throw new Exception($"The locations are null, json: {jsonArray}");
                }
                _locations = locations;
                _logger.LogInformation($"APIClient | GetLocations | Count: {_locations.Count}");
            }
            else
            {
                throw new Exception($"Unknown error while attempting to get the Locations, status-code {response.StatusCode}");
            }
        }

        private void GetScenes(int locationId, bool withRetry = true)
        {
            _logger.LogInformation("APIClient | GetScenes");

            var request = new RestRequest("/api/scenes", Method.Get);

            request.AddHeader("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148");
            request.AddHeader("session-id", _lastSession.Session);

            request.AddQueryParameter("location$id", locationId);

            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = response.Content;

                if (string.IsNullOrEmpty(responseContent))
                {
                    throw new Exception($"Couldn't gather Scenes, too many requests?\nJson: {response.Content}");
                }

                if (responseContent.Contains("USRSESSEXP"))
                {
                    GetSession();
                    if(withRetry)
                    {
                        GetScenes(locationId, false);
                    }
                    return;
                }

                var jArray = JArray.Parse(responseContent);
                var scenes = JsonConvert.DeserializeObject<List<NeviWebScene>>(jArray.ToString());
                if (scenes == null)
                {
                    throw new Exception($"The scenes are null, json: {jArray}");
                }
                _scenes.AddRange(scenes);
                _logger.LogInformation($"APIClient | GetScenes | Count: {_scenes.Count}");
            }
            else
            {
                throw new Exception($"Unknown error while attempting to get the Session ID, status-code {response.StatusCode}");
            }
        }


        private void GetSession()
        {
            _logger.LogInformation("APIClient | GetSession");

            var request = new RestRequest("/api/login", Method.Post);
            var json = JsonConvert.SerializeObject(new LoginRequest(_config.Email, _config.Password));

            request.AddHeader("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148");
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonObj = JObject.Parse(response.Content);
                if (!jsonObj.ContainsKey("error") && jsonObj.ContainsKey("session"))
                {
                    var user = JsonConvert.DeserializeObject<NeviWebUser>(jsonObj.ToString());
                    if(user == null)
                    {
                        throw new Exception($"The user is null, json: {jsonObj}");
                    }
                    SaveLastSession(user);
                }
                else
                {
                    throw new Exception($"Couldn't gather Session ID, too many requests?\nJson: {response.Content}");
                }
            }
            else
            {
                throw new Exception($"Unknown error while attempting to get the Session ID, status-code {response.StatusCode}");
            }
        }

        private void PostConstruct()
        {
            LoadLastSession();
            GetLocations();
            LoadScenes();
        }

        private void LoadScenes()
        {
            foreach(var location in _locations)
            {
                GetScenes(location.Id);
            }
        }

        private void LoadLastSession()
        {
            try
            {
                var json = File.ReadAllText("last_session.json");
                _lastSession = JsonConvert.DeserializeObject<NeviWebUser>(json);
            }
            catch (Exception)
            {
                GetSession();
            }
        }

        private void SaveLastSession(NeviWebUser session)
        {
            _lastSession = session;
            File.WriteAllText("last_session.json", JsonConvert.SerializeObject(session));
        }
    }
}
