using chat_server.Models;
using chat_server.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RostersController : ControllerBase
    {
        private readonly IRosterService _rosterService;
        public RostersController(IRosterService rosterService) 
        {
            _rosterService = rosterService;
        }
        // GET: api/<RostersController>
        [HttpGet]
        public ActionResult<List<Roster>> Get()
        {
            return _rosterService.Get();
        }

        // GET api/<RostersController>/5
        [HttpGet("{id}")]
        public ActionResult<Roster> Get(string id)
        {
            var roster = _rosterService.Get(id);

            if (roster == null)
            {
                return NotFound($"Roster with ID = {id} not found");
            }

            return roster;
        }

        // POST api/<RostersController>
        [HttpPost]
        public ActionResult<Roster> Post([FromBody] Roster roster)
        {
            _rosterService.Create(roster);

            return CreatedAtAction(nameof(Get), new { id = roster.Id }, roster);
        }

        // PUT api/<RostersController>/5
        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] Roster roster)
        {
            var existingRoster = _rosterService.Get(id);

            if (existingRoster == null)
            {
                return NotFound($"Roster with Id = {id} not found");
            }

            _rosterService.Update(id, roster);

            return NoContent();
        }

        [HttpPut("follower/{userId}")]
        public ActionResult AddFollower(string userId, [FromBody] string followerId)
        {
            var existingRoster = _rosterService.Get(userId);
            string[] result;
            if (existingRoster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }
            else {
                int newLength = existingRoster.Follower.Length + 1;
                result = new string[newLength];

                for(int i = 0; i < existingRoster.Follower.Length; i++) {
                    result[i] = existingRoster.Follower[i];
                }

                result[newLength - 1] = followerId;
            }

            existingRoster.Follower = result;

            _rosterService.UpdateFollower(userId, existingRoster);

            return NoContent();
        }

        [HttpPut("updateNick/{userId}")]
        public ActionResult UpdateNick(string userId, [FromBody] string nick)
        {
            var existingRoster = _rosterService.Get(userId);
            
            if(existingRoster == null)
            {
                return NotFound($"Roster with ID = {userId} Not Found");
            }

            existingRoster.NickName = nick;
            _rosterService.Update(userId, existingRoster);

            return NoContent();
        }

        [HttpPut("updatePhoto/{userId}")]
        public ActionResult UpdatePhoto(string userId, [FromBody] string photo)
        {
            var existingRoster = _rosterService.Get(userId);

            if (existingRoster == null)
            {
                return NotFound($"Roster with ID = {userId} Not Found");
            }

            existingRoster.Photo = photo;
            _rosterService.Update(userId, existingRoster);

            return NoContent();
        }

        // DELETE api/<RostersController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            var roster = _rosterService.Get(id);

            if (roster == null)
            {
                return NotFound($"Roster with Id = {id} not found");
            }

            _rosterService.Remove(id);

            return Ok($"Roster with id = {id} is deleted");
        }
    }
}
