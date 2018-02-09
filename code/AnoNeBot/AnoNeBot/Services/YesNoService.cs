using AnoNeBot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnoNeBot.Services
{
    public class YesNoService
    {
        public static async Task<YesNoModel> GetYesNoAsync(bool translate = false)
        {
            HttpResponseMessage res;

            using (HttpClient hc = new HttpClient())
            {
                res = await hc.GetAsync("https://yesno.wtf/api/");
            }

            if (res.IsSuccessStatusCode)
            {
                var yesNo = JsonConvert.DeserializeObject<YesNoModel>(await res.Content.ReadAsStringAsync());
                if (translate)
                    yesNo.Answer = Translate(yesNo.Answer);

                return yesNo;
            }
            else
                return null;
        }

        public static string Translate(string yesNo)
        {
            string translation;

            switch (yesNo.ToLower())
            {
                case Answers.Yes:
                    translation = "Ano";
                    break;
                case Answers.No:
                    translation = "Ne";
                    break;
                default:
                    translation = "Nejsem si jistý";
                    break;
            }

            return translation;
        }
    }
}