using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Surveys.Web.Hubs;

namespace Surveys.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManualController : ControllerBase
    {
        private readonly IHubContext<GridEventsHub> _hubContext;

        public ManualController(IHubContext<GridEventsHub> gridEventsHubContext)
        {
            this._hubContext = gridEventsHubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var messageContent = await reader.ReadToEndAsync();
                if (messageContent.Length > 0)
                {
                    await this._hubContext.Clients.All.SendAsync(
                        "gridupdate",
                        messageContent);
                }
            }

            return Ok();
        }
    }

}