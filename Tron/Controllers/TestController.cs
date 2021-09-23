using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Tron.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET: api/Test
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Test/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        //// POST: api/Test
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // POST: api/Test
        [HttpPost]
        public void Post([FromBody] dynamic value)
        {
        }

        // POST: api/Test/Action
        [HttpPost("Action")]
        public void PostAction([FromBody] dynamic value)
        {
        }

        //// POST: api/Test
        //[HttpPost]
        //public void Post([FromBody] TestObject value)
        //{
        //    if (value != null)
        //    {

        //    }
        //}

        // PUT: api/Test/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class TestObject
    {
        public int Value { get; set; }
    }
}
