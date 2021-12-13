
using Microsoft.AspNetCore.Mvc;

namespace Pathfinder.Controllers
{
    [Route("/")]
    public class CountryController : Controller
    {
        private const string destination = "USA";

        [HttpGet("{id}")]
        public ActionResult GetPath(string id)
        {
            id = id.Replace("'", "''");
            return Ok(DbConnection.GetPath(destination, id).Result);
        }

        [HttpGet()]
        public ActionResult Index()
        {
            return Ok();
        }
    }
}
