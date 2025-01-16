using Microsoft.AspNetCore.Mvc;

namespace SabaSimulator.API.Controllers;

public class HomeController : Controller
{
	public IActionResult HealthCheck()
	{
		return Ok(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
	}
}