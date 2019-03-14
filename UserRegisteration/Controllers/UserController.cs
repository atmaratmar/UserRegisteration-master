using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserRegisteration.Models;

namespace UserRegisteration.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        [HttpGet]
        public ActionResult AddorEdit(int id = 0)
        {
            User userModel = new User();

            return View(userModel);

        }

        [HttpPost]
        public ActionResult AddorEdit(User userModel)
        {
            


            string streetName = userModel.StreetName;
            string postNr = userModel.ZipCode;
            string city = userModel.ZipCity;
            string streetNr = userModel.StreetNumber;

            string url = "https://dawa.aws.dk/autocomplete?caretpos=28&fuzzy=&q=" + streetName + " " + streetNr + "," +
                         " " + postNr + " " + city + "&startfra=adresse&type=adresse";
            string urlResult = url;
            string data = "";

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse) req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                Stream recStream = resp.GetResponseStream();
                StreamReader readStream = null;
                if (resp.CharacterSet == null)
                {
                    readStream = new StreamReader(recStream);
                }
                else
                {
                    readStream = new StreamReader(recStream, Encoding.GetEncoding(resp.CharacterSet));
                }

                data = readStream.ReadToEnd();
                //resp.Close();
                //readStream.Close();
            }
            //Creating List

            var postcodeList = JArray.Parse(data).Select(p =>
                new
                {

                    post_code = p["data"]["postnr"],
                    city_name = p["data"]["postnrnavn"],

                });

            foreach (var post in postcodeList)
            {
                if (city.ToLower() == post.city_name.ToString().ToLower() &&
                    (postNr.ToLower() == post.post_code.ToString().ToLower()))
                {


                    using (UserRegisterationEntities UserRegisteration = new UserRegisterationEntities())


                    {

                        if (UserRegisteration.Users.Any(x => x.UserName == userModel.UserName))
                        {
                            ViewBag.DublicateMessage = "Username already exist.";
                            return View("AddorEdit", userModel);

                        }


                        if (UserRegisteration.postnrs.Any(x =>
                            x.ZipCode == userModel.ZipCode &&
                            UserRegisteration.postnrs.Any(z => z.ZipCity == userModel.ZipCity)))
                        {

                            UserRegisteration.Users.Add(userModel);

                            UserRegisteration.SaveChanges();


                        }
                        else
                        {
                            ViewBag.DublicateMessage = "This post number does not exist.";
                            return View("AddorEdit", userModel);
                        }



                    }

                }
                else if (city.ToLower() != post.city_name.ToString().ToLower() &&
                         (postNr.ToLower() == post.post_code.ToString().ToLower())) {}
                else
                {

                    ViewBag.DublicateMessage = "This post number or city name does not exist.";
                    return View("AddorEdit", userModel);
                }

                break;
            }
            ModelState.Clear();
            ViewBag.SuccessMessage = "Registeration Successfull.";
            return View("AddorEdit", new User());
        }
       
        
        

        //[HttpGet]

        //public JsonResult EmpDetails()
        //{
        //    //Creating List
        //    List<Address> ObjEmp = new List<Address>()
        //    {
        ////Adding records to list
        //new Address {StreetName="Odinsvej",StreetNumber="7",ZipCode="4270",ZipCity="høng" },


        //    };
        //    //return list as Json
        //    return Json(ObjEmp, JsonRequestBehavior.AllowGet);
        //}
    }

}










        