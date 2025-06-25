using Microsoft.AspNetCore.Mvc;

using Fringe.Service.Interfaces;

using System.Threading.Tasks;

namespace Fringe.API.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class ExploreController : ControllerBase

    {

        private readonly IExploreService _exploreService;

        public ExploreController(IExploreService exploreService)

        {

            _exploreService = exploreService;

        }

        [HttpGet]

        public async Task<IActionResult> Get()

        {

            var events = await _exploreService.GetExploreEventsAsync();

            return Ok(events);

        }

    }

}



 