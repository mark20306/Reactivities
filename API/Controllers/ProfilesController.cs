using Application.Profiles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProfilesController : BaseApiController
    {
        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            return HandleResult(await Mediator.Send(new Details.Query{UserName = username}));
        }

        [HttpGet("{username}/activities")]
        public async Task<IActionResult> GetUserActivities(string predicate, string username)
        {
            return HandleResult(await Mediator.Send(new ListActivities.Query{Predicate = predicate, Username = username}));
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Edit.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }
    }
}