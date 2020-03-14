using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DadJokesAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DadJokesAPI.Controllers
{
    public class HomeController : Controller
    {
        List<Joke> _jokes = new List<Joke>();

        public ActionResult Index()
        {
            return View(GetRandomJoke());
        }

        [HttpGet]
        public ActionResult RandomJoke()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SearchJokes(string searchTerm)
        {
            ViewBag.Message = "Search Term : "+ searchTerm;

            Uri apiUrl = new Uri("https://icanhazdadjoke.com");

            string searchString = "search?term=" + searchTerm + "&limit=30";

            using (var client = new HttpClient())
            {
                client.BaseAddress = apiUrl;
                //HTTP GET
                client.DefaultRequestHeaders.Clear();  
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  

                var responseTask = client.GetAsync(searchString);
                responseTask.Wait();
                var result = responseTask.Result;
                var json = result.Content.ReadAsStringAsync().Result;
                JObject parsedJson = (JObject) JsonConvert.DeserializeObject(json);
                var jokeText = parsedJson["results"];

                _jokes = JsonConvert.DeserializeObject<List<Joke>>(jokeText.ToString());

                _jokes = new List<Joke>(_jokes.OrderBy(x => x.joke.Length));

                _jokes = HighlightTermWordCount(searchTerm);

            }

            ViewBag.Count = "Found " + _jokes.Count + " joke(s)";

            return View("Index",_jokes);
        }

        private List<Joke> HighlightTermWordCount(string searchTerm)
        {
            foreach (var item in _jokes)
            {
                // get word count
                char[] delimiters = new char[] {' ', '\r', '\n' };
                item.wordCount = item.joke.Split(delimiters,StringSplitOptions.RemoveEmptyEntries).Length; 

                // highlight searchTerm in joke
                item.joke = item.joke.Replace(searchTerm, "[" + searchTerm + "]");
            }

            return _jokes;
        }

        public List<Joke> GetRandomJoke()
        {
            Uri apiUrl = new Uri("https://icanhazdadjoke.com");

            using (var client = new HttpClient())
            {
                client.BaseAddress = apiUrl;
                //HTTP GET
                client.DefaultRequestHeaders.Clear();  
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  

                var responseTask = client.GetAsync("");
                responseTask.Wait();
                var result = responseTask.Result;
                
                var json = result.Content.ReadAsStringAsync().Result;
                JObject parsedJson = (JObject) JsonConvert.DeserializeObject(json);
                var jokeText = parsedJson["joke"];

                // get word count
                char[] delimiters = new char[] {' ', '\r', '\n' };
                var wordCount = jokeText.ToString().Split(delimiters,StringSplitOptions.RemoveEmptyEntries).Length; 

                var joke = new Joke { id = "999", joke = jokeText.ToString(), wordCount = wordCount};
                if (joke != null)
                {
                    _jokes.Add(joke);
                }
            }
            return _jokes;
        }
    }
}