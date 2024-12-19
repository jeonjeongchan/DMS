using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DMS.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            //var items = CreateMenu();
            return View();
        }

        public JArray CreateJson(List<T_Member> peopleList)
        {

            JArray people = new JArray();
            for (int i = 0; i < peopleList.Count; i++)
            {
                var jsonModel1 = JObject.FromObject(peopleList[i]);
                people.Add(jsonModel1);
            }

            return people;
        }

        //public List<Member> CreateMenu()
        //{
        //    List<Member> peopleList = new List<Member>();
        //    JArray people = new JArray();

        //    Member person1 = new Person { name = "jeongchan", age = 30, gender = "male" };
        //    Person person2 = new Person { name = "dayeon", age = 40, gender = "female" };

        //    peopleList.Add(person1);
        //    peopleList.Add(person2);

        //    if (peopleList.Count > 0)
        //    {
        //        people = CreateJson(peopleList);
        //    }

        //    return peopleList;
        //}

    }

}

