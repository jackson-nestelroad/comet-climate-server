using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using WebAPI.Models;

namespace Comet.Climate.Server.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET /test
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load("https://forecast.weather.gov/MapClick.php?lat=32.9607&lon=-96.733&lg=english&&FcstType=digital");
            if(document.ParseErrors != null)
                return new string[] { "bad", web.StatusCode.ToString() };
            
            var forecast = new int[5];
            for(int row = 3; row < 8; row++)
            {
                var node = document.DocumentNode.SelectNodes("//table[6]/tbody/tr[4]/td[{row}]");
                if(node == null)
                    return new string[] { "bad", row.ToString() };
                forecast.Append(int.Parse(node[0].InnerText));
            }
            return new string[] { "good?" };
        }
    }
}
