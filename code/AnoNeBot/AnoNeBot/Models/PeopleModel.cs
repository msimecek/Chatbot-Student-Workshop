using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnoNeBot.Models
{
    public class PeopleModel
    {
        public static Dictionary<string, string> People = new Dictionary<string, string>()
        {
            { "Assets/jarda.jpg", "Jarda" },
            { "Assets/martin.jpg", "Martin" },
            { "Assets/satya.jpg", "Satya" }
        };
    }
}