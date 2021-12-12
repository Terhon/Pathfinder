
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
            var path = destination.Equals(id.ToUpper()) ? new List<string>() { destination } : DbConnection.GetPath(destination, id).Result;

            return Ok(path);
        }

        [HttpGet()]
        public ActionResult Index()
        {
            return Ok();
        }
    }
}
