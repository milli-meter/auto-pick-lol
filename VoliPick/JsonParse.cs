using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VoliPick
{
    public class ChampList
    {
        private List<JSParseChampList> getChampList;
        private List<string> champNameList = new List<string>();
        private Dictionary<string, int> champNameIDList = new Dictionary<string, int>();

        public List<string> ChampNameList { get => champNameList; }
        public Dictionary<string, int> ChampNameIDList { get => champNameIDList; }
        public void GetChampList(string json)
        {
            getChampList = JsonConvert.DeserializeObject<List<JSParseChampList>>(json);
            champNameIDList.Clear();
            champNameList.Clear();
            foreach (var item in getChampList)
            {
                champNameIDList.Add(item.name, item.id);
                champNameList.Add(item.name);
            }
        }
    }
    class JSParseChampList
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
