using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigsarkiv.Athena.DummyData
{
    public class FauxData
    {
        public Dictionary<String, List<String>> Tabl1 { get; set; }
        public Dictionary<String, List<String>> Tabl2 { get; set; }

        /// <summary>
        /// Creates fake data for the datagrid.
        /// </summary>
        public FauxData()
        {
            Tabl1 = new Dictionary<string, List<string>>();
            Tabl2 = new Dictionary<string, List<string>>();
            PopulateTabl1();
            PopulateTabl2();
        }

        private void PopulateTabl1()
        {
            Tabl1.Add("Køn", new List<String> { "M", "K", "K" });
            Tabl1.Add("Arbejde", new List<String> { "Pilot", "Bager", "Læge" });
            Tabl1.Add("Hobby", new List<String> { "Golf", "Bøger", "Løbe i skoven" });
        }

        private void PopulateTabl2()
        {
            Tabl2.Add("Post nr.", new List<String> { "3200", "3400", "2860" });
            Tabl2.Add("Land", new List<String> { "DK", "DK", "DK" });
        }
    
    }   
}
