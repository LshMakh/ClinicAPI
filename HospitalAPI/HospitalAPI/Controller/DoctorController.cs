using AuthProjWebApi.Auth;
using HospitalAPI.Models;
using HospitalAPI.Packages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAPI.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        IPKG_DOCTOR package;
        private readonly ILogger<DoctorController> _logger;
        private readonly IJwtManager jwtManager;

        public DoctorController(IPKG_DOCTOR package, IJwtManager jwtManager, ILogger<DoctorController> logger)
        {
            this.package = package;
            this.jwtManager = jwtManager;
            _logger = logger;
        }
        [HttpPost]
        public IActionResult RegisterDoctor(Doctor doctor)
        {
            try
            {
                package.RegisterDoctor(doctor);
                return Ok(new { message = "Doctor registered successfully" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return Conflict(new { message = ex.Message });
                }
                _logger.LogError(ex, "Error registering doctor");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while registering the doctor" });
            }
        }
        [HttpGet]
        public IActionResult GetDoctorCards()
        {
            List<Doctor> docs = new List<Doctor>();
            docs = package.GetDoctorCards();
            return Ok(docs);

        }
    }                                                                                                                                                               
}
