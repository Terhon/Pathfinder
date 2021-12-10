
using Microsoft.AspNetCore.Mvc;
using Pathfinder.Domain.Persistence.Contexts;

namespace Pathfinder.Controllers
{
    [Route("/")]
    public class CountryController : Controller
    {
        private const string destination = "USA";

        [HttpGet("{id}")]
        public async Task<IEnumerable<string>> GetPathAsync(string id)
        {
            var path = await DbConnection.GetPath(destination, id);
            return path;
        }
    }
}
