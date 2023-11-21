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

        [HttpGet("online/{userId}")]
        public ActionResult<List<Roster>> GetRoster(string userId) 
        {
            var roster = _rosterService.Get(userId);

            if(roster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }

            List<Roster> demo = new List<Roster>();

            demo = _rosterService.GetOnlineRoster(roster.Follower);

            return demo;

        }

        [HttpGet("suggestion/{userId}")]
        public ActionResult<List<Roster>> GetSuggestionRoster(string userId)
        {
            var roster = _rosterService.Get(userId);

            if (roster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }

            List<Roster> demo = new List<Roster>();

            if (roster.Follower.Count > 20)
            {
                demo = _rosterService.GetSuggestionRoster(roster.Follower);

                return demo;
            }
            else
            {
                demo = _rosterService.Get();

                // Exclude Blocked list from suggestion list

                var blockedList = roster.Blocked;

                // Exclude myself from the List
                demo = demo.Where(r => r.UserId != userId && (blockedList == null || !blockedList.Contains(r.UserId))).ToList();

                Random random = new Random();
                List<Roster> randomRoster = demo.OrderBy(x => random.Next()).Take(10).ToList();

                return randomRoster;
            }
        }

        // POST api/<RostersController>
        [HttpPost]
        public ActionResult<Roster> Post([FromBody] Roster roster)
        {

            var exitstingRoster = _rosterService.Get(roster.UserId);

            // Can't create with same userId
            if (exitstingRoster != null)
            {
                return BadRequest(exitstingRoster);
            }

            roster.Follower = new List<Follower>();
            roster.Blocked = new string[] { };

            roster.LastOnline = DateTime.Now;

            _rosterService.Create(roster);

            return CreatedAtAction(nameof(Get), new { id = roster.Id }, roster);
        }

        /// <summary>
        ///  Bulk Entry for User Creation
        /// </summary>
        /// <param name="roster"></param>
        /// <returns></returns>
        [HttpPost("bulk_register")]
        public ActionResult<IEnumerable<Roster>> BulkUserCreation([FromBody] List<Roster> rosterList)
        {

            if(rosterList == null || rosterList.Any())
            {
                return BadRequest("No rosters provided for Bulk Creation");
            }

            foreach(var roster in rosterList)
            {

                roster.Follower = new List<Follower>();
                roster.Blocked = new string[] { };

                roster.LastOnline = DateTime.Now;

                _rosterService.Create(roster);
            }


            return Ok();
        }

        [HttpGet("bulk")]
        public ActionResult<IEnumerable<Roster>> GetBulk()
        {
            return Ok();
        }

        [HttpPost("last_online")]
        public ActionResult<List<Roster>> LastOnline([FromBody] UserIdModel userList)
        {
            if(userList == null || userList.UserId.Length < 1) 
            { 
                return BadRequest("Request List Can't be Null");
            }

            var lastOnlineRoster = _rosterService.GetLastOnlineRoster(userList.UserId);

            return lastOnlineRoster;
        }

        /// <summary>
        /// BlockList
        /// </summary>
        /// <param name="blocklist"></param>
        /// <returns>Roster</returns>
        
        [HttpPost("blocklist")]
        public ActionResult<Roster> BlockUser([FromBody] BlockIdModel blocklist)
        {
            if (blocklist == null)
            {
                return BadRequest("Request List Can't be Null");
            }

            var roster = _rosterService.Get(blocklist.UserId);

            if (roster == null)
            { 
                return NotFound();
            }

            int newLength = 1;
            string[] result = new string[newLength];

            try
            {
                newLength = roster.Blocked.Length + 1;
                result = new string[newLength];

                for (int i = 0; i < roster.Blocked.Length; i++)
                {
                    result[i] = roster.Blocked[i];
                }
            }
            catch (Exception e) { 
            
            }
            
            

            result[newLength - 1] = blocklist.BlockId;

            roster.Blocked = result;

            roster.Follower.RemoveAll(follower => follower.UserId == blocklist.BlockId);

            _rosterService.UpdateFollower(blocklist.UserId, roster);


            return roster;
        }

        [HttpPost("getBlockUser")]
        public ActionResult<BlockedResponseModel> GetBlockedResult(BlockIdModel blockList)
        {
            var roster = _rosterService.Get(blockList.UserId);

            if (roster == null)
            {
                return NotFound($"Roster with Id = {blockList.UserId} not found");
            }

            bool isBlocked = roster.Blocked.Contains(blockList.BlockId);

            BlockedResponseModel response = new BlockedResponseModel {
                IsBlocked = isBlocked
            };

            return Ok(response);
        }

        [HttpDelete("blocklist")]
        public ActionResult RemoveBlockedUser(BlockIdModel blockList)
        {
            var roster = _rosterService.Get(blockList.UserId);

            if (roster == null)
            {
                return NotFound($"Roster with Id = {blockList.UserId} not found");
            }

            if (roster.Blocked.Contains(blockList.BlockId))
            {
                // Create a new array excluding the follower to be removed
                var updatedBlocked = roster.Blocked.Where(id => id != blockList.BlockId).ToArray();
                roster.Blocked = updatedBlocked;

                //update the roster
                _rosterService.UpdateFollower(blockList.UserId, roster);
            }

            return NoContent();
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

        // update Follower list
        [HttpPut("follower/{userId}")]
        public ActionResult AddFollower(string userId, [FromBody] FollowerModel followerModel)
        {

            if(followerModel == null || string.IsNullOrWhiteSpace(followerModel.FollowerId))
            {
                return BadRequest("Invalid Request Body");
            }            

            var existingRoster = _rosterService.Get(userId);
            string[] result;
            if (existingRoster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }
            else {
                existingRoster.Follower.Add(new Follower
                {
                    UserId = followerModel.FollowerId,
                    IsFriend = followerModel.FollowBack
                });
            }

            _rosterService.UpdateFollower(userId, existingRoster);


            if (followerModel.FollowBack)
            {
                // Need to update friendship from partner roster
                var partnerRoster = _rosterService.Get(followerModel.FollowerId);

                if (partnerRoster == null)
                {
                    return NotFound($"Roster with Id = {followerModel.FollowerId} not found");
                }
                else
                {

                    var follower = partnerRoster.Follower.FirstOrDefault(follower => follower.UserId == userId);
                    partnerRoster.Follower.Remove(follower);

                    partnerRoster.Follower.Add(new Follower
                    {
                        UserId = userId,
                        IsFriend = followerModel.FollowBack
                    });
                }

                _rosterService.UpdateFollower(followerModel.FollowerId, partnerRoster);
            }

            return NoContent();
        }

        [HttpPut("unfollower/{userId}")]
        public ActionResult RemoveFollower(string userId, [FromBody] FollowerModel followerModel)
        {

            if (followerModel == null || string.IsNullOrWhiteSpace(followerModel.FollowerId))
            {
                return BadRequest("Invalid Request Body");
            }

            var existingRoster = _rosterService.Get(userId);
            string[] result;
            if (existingRoster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }

            Follower followerToRemove = existingRoster.Follower.FirstOrDefault(x => x.UserId == followerModel.FollowerId);

            if(followerToRemove != null && !followerToRemove.IsFriend)
            {
                existingRoster.Follower.Remove(followerToRemove);
            }

            return NoContent();
        }

        [HttpPut("updateNick/{userId}")]
        public ActionResult UpdateNick(string userId, [FromBody] NickModel nickModel)
        {
            if(nickModel == null || string.IsNullOrWhiteSpace(nickModel.NickName))
            {
                return BadRequest("Invalid Request Body");
            }

            var existingRoster = _rosterService.Get(userId);
            
            if(existingRoster == null)
            {
                return NotFound($"Roster with ID = {userId} Not Found");
            }

            existingRoster.NickName = nickModel.NickName;
            _rosterService.Update(userId, existingRoster);

            return NoContent();
        }

        [HttpPut("updatePhoto/{userId}")]
        public ActionResult UpdatePhoto(string userId, [FromBody] PhotoModel photoModel)
        {

            if(photoModel == null || string.IsNullOrWhiteSpace(photoModel.Photo))
            {
                return BadRequest("Invalid Request Body");
            }

            var existingRoster = _rosterService.Get(userId);

            if (existingRoster == null)
            {
                return NotFound($"Roster with ID = {userId} Not Found");
            }

            existingRoster.Photo = photoModel.Photo;
            _rosterService.Update(userId, existingRoster);

            return NoContent();
        }

        [HttpPut("updatePresence/{userId}")]
        public ActionResult UpdatePresence(string userId, [FromBody] PresenceModel presence)
        {

            if (presence == null)
            {
                return BadRequest("Invalid Request Body");
            }

            var existingRoster = _rosterService.Get(userId);

            if (existingRoster == null)
            {
                return NotFound($"Roster with ID = {userId} Not Found");
            }

            existingRoster.IsActive = presence.Presence;

            _rosterService.Update(userId, existingRoster);

            return NoContent();
        }

        // DELETE api/<RostersController>/5
        [HttpDelete("{userId}")]
        public ActionResult Delete(string userId)
        {
            var roster = _rosterService.Get(userId);

            if (roster == null)
            {
                return NotFound($"Roster with Id = {userId} not found");
            }

            _rosterService.Remove(userId);

            return Ok($"Roster with id = {userId} is deleted");
        }
    }
}
