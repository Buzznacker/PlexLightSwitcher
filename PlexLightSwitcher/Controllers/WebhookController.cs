using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlexLightSwitcher.Models;
using PlexLightSwitcher.Worker;

namespace PlexLightSwitcher.Controllers
{
    [ApiController]
    public class WebhookController : ControllerBase
    {      
        private readonly ILogger<WebhookController> _logger;
        private readonly APIWorker _worker;

        public WebhookController(ILogger<WebhookController> logger, APIWorker worker)
        {
            _logger = logger;
            _worker = worker;    
        }

        [HttpPost("/plex/webhook")]
        public async Task Webhook()
        {
            var request = HttpContext.Request;
            var json = request.Form["payload"].ToString();
            try
            {
                var plexMessage = JsonConvert.DeserializeObject<PlexMessage>(json);
                if (plexMessage != null)
                {
                    _worker.AddMessage(plexMessage);
                }           
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "");
            }          
        }
    }
}