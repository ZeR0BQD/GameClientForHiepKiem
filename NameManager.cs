using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class NameManager
    {

        public static List<string> FirstNames = new List<string>();
        public static List<string> FemaleNames = new List<string>();
        public static List<string> MaleNames = new List<string>();

        public static void Init()
        {
            FirstNames.Clear();
            FemaleNames.Clear();
            MaleNames.Clear();

            string content = File.ReadAllText("Config/first_name.json");
            var firstNames = JsonConvert.DeserializeObject<List<string>>(content);
            if (firstNames == null)
            {
                Console.WriteLine("Load first name error");
                return;
            }
            FirstNames.AddRange(firstNames);

            content = File.ReadAllText("Config/female_name.json");
            var femaleNames = JsonConvert.DeserializeObject<List<string>>(content);
            if (femaleNames == null)
            {
                Console.WriteLine("Load female names error");
                return;
            }
            FemaleNames.AddRange(femaleNames);

            content = File.ReadAllText("Config/male_name.json");
            var maleNames = JsonConvert.DeserializeObject<List<string>>(content);
            if (maleNames == null)
            {
                Console.WriteLine("Load male names error");
                return;
            }
            MaleNames.AddRange(maleNames);
        }

        public static string RandomName(int sex)
        {
            string firstname = FirstNames.OrderBy(f => Guid.NewGuid()).First();
            string lastname = string.Empty;
            if (sex == 0)
            {
                lastname = MaleNames.OrderBy(f => Guid.NewGuid()).First();
            }
            else
            {
                lastname = FemaleNames.OrderBy(f => Guid.NewGuid()).First();
            }
            return string.Format("{0}{1}", firstname, lastname);
        }
    }
}
