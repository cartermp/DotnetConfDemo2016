using System;
using System.Web.Http;
using System.Web.Http.Description;
using MusicStore.Cassandra;
using System.Configuration;

namespace AnalyticsApi.Controllers
{
    [RoutePrefix("api")]
    public class AnalyticsController : ApiController
    {
        private readonly ICassandraRepo _repo;

        public AnalyticsController()
            : this(new CassandraRepo(ConfigurationManager.AppSettings["ipaddress"],
                                     int.Parse(ConfigurationManager.AppSettings["port"])))
        { }

        public AnalyticsController(ICassandraRepo repo)
        {
            _repo = repo;
        }

        // api/send
        [HttpPut]
        [ResponseType(typeof(AlbumDetails))]
        [Route("send")]
        public IHttpActionResult Put(int id, string name, bool cached, DateTimeOffset time)
        {
            var details = new AlbumDetails(id, name, cached, time);

            _repo.Send(details);

            return CreatedAtRoute("send", null, details);
        }
    }
}