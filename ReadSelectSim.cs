using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace BTCSIM2
{
    public class ReadSelectSim
    {
        public ReadSelectSim()
        {
        }


        public List<int> generateBestPlIndList(int num_select)
        {
            var pl = new Dictionary<int, double>();
            var n = 0;
            using (var sr = new StreamReader("opt nanpin select.csv"))
            {
                var data = sr.ReadLine();
                while ((data = sr.ReadLine()) != null)
                {
                    pl[n] = Convert.ToDouble(data.Split(',')[3]);
                    n++;
                }
            }
            IOrderedEnumerable<KeyValuePair<int, double>> sorted = pl.OrderByDescending(pair => pair.Value);
            var selected_inds = new List<int>();
            foreach (var data in sorted)
            {
                selected_inds.Add(data.Key);
                if (selected_inds.Count >= num_select)
                    break;
            }
            return selected_inds;
        }
    }
}
