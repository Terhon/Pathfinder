
using Microsoft.AspNetCore.Mvc;

namespace Pathfinder.Controllers
{
    [Route("/")]
    public class CountryController : Controller
    {
        private const string destination = "USA";

        [HttpGet("{id}")]
        public async Task<IEnumerable<string>> GetPathAsync(string id)
        {
            return destination.Equals(id.ToUpper()) ? new List<string>(){ destination } : await DbConnection.GetPath(destination, id);
        }
    }
}
