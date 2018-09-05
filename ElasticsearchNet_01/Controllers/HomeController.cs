using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ElasticsearchNet_01.Controllers
{

    public class HomeController : Controller
    {

       static IConnectionSettingsValues settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("people");

        ElasticClient client = new ElasticClient(settings);
        //
        // GET: /Home/
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult Get() 
        {
            var response = client.Get(new DocumentPath<Person>(0));

            return Content("ok");
        }

        public async Task<ActionResult> Search() 
        {
            var b= client.IndexExists("people");
            var c = client.Count<Person>();

            //不加size 只能取到10条
            var all= client.Search<Person>(s => s
                .Query(q =>q
                    .MatchAll())
                .Size(500)
                    );

            var searchResponse = await client.SearchAsync<Person>(s => s
                //.AllIndices()
                // .AllTypes()
                .From(0)
                .Size(500)
                .Query(q => q
                        .Match(m => m
                        .Field(f => f.FirstName)
                        .Query("Martijn")
                        )
                ));

            //精确
            var firstSearchResponse = client.Search<Person>(s => s
                .Query(q => q
                    .Term(t => t.Id, 1) 
                    //||
                    //q.Term(p => p.LastName, "Laarman9")
                )
            );

            //全文
            var result = client.Search<Person>(s => s
                .Query(q => q.MatchPhrase(m => m.Field(f => f.FirstName)
                                .Query("Martijn")
                                )
                )
                .From(0)
                .Size(200)
                );

           var p=  client.Search<Person>(s => s.Index("people"));


           var result2 = client.Search<Person>(s => s
              .Query(q =>q.
                  MoreLikeThis(mt=>mt.
                      Like(m => m.Text("Martijn"))) 
                  )
              .From(0)
              .Size(15)
              );
            
            return Json(searchResponse.Documents.ToList<Person>(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create()
        {

            for (int i = 0; i < 100; i++)
            {
                var person = new Person
                {
                    Id = i,
                    FirstName = "Martijn"+i,
                    LastName = "Laarman"+i
                };
                var indexResponse = client.IndexDocument(person);
            }


           // var asyncIndexResponse = await client.IndexDocumentAsync(person);
            return Content("ok");
        }


        public class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }


	}
}