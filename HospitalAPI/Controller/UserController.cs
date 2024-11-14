using AuthProjWebApi.Auth;
using HospitalAPI.DTO_s;
using HospitalAPI.Models;
using HospitalAPI.Packages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAPI.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IPKG_USERS package;
        private readonly IJwtManager jwtManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IPKG_USERS package, IJwtManager jwtManager, ILogger<UserController> logger)
        {
            this.package = package;
            this.jwtManager = jwtManager;
            _logger = logger;
        }

      
        [HttpPost]
        public IActionResult Authenticate([FromBody] UserLoginDto logindata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = package.Authenticate(logindata);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var token = jwtManager.GetToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Email}", logindata.Email);
                return StatusCode(500, new { message = "Authentication failed. Please try again later." });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetUserInfo(int id)
        {
            try
            {
                var user = package.GetUserInfo(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information for ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }

        [HttpGet("check-email/{email}")]
        public IActionResult CheckEmailExists(string email)
        {
            try
            {
                bool exists = package.CheckUserEmailExists(email);
                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence for {Email}", email);
                return StatusCode(500, new { message = "An error occurred while checking email existence" });
            }
        }

    }

}

