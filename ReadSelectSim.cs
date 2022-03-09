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
        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }

        public int select_timing_time_window { get; set; }
        public double select_timing_subordinate_ratio { get; set; }
        public int select_strategy_time_window { get; set; }

        private void initialize()
        {
            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();

            select_timing_time_window = new int();
            select_timing_subordinate_ratio = new double();
            select_strategy_time_window = new int();
        }

        public ReadSelectSim()
        {
            initialize();
        }

        ~ReadSelectSim()
        {
            initialize();
        }
         

        public Account startReadSelectSim(int from, int to, string lev_or_fixed, List<int> selected_opt_ids, int selected_opt_select_id)
        {
            readNanpinStrategyData(selected_opt_ids);
            readSelectStrategyData(selected_opt_select_id);
            var strategy_ac_list = new List<Account>();
            var strategy_from = from - 10100; //sim result should be availavle before the actual from of select strategy
            for (int i = 0; i < para_lc.Count; i++)
                strategy_ac_list.Add(doSim(ref lev_or_fixed, para_strategy[i], ref strategy_from, ref to, para_pt[i], para_lc[i], para_nanpin_timing[i], para_nanpin_lot[i], para_ma_term[i], para_rapid_side_change_ratio[i]));
            
            var ac = doSelectSim(lev_or_fixed, from, to, strategy_ac_list, para_pt, para_lc, para_ma_term, para_strategy, para_rapid_side_change_ratio, para_nanpin_timing,
                para_nanpin_lot, select_timing_time_window, select_strategy_time_window, select_timing_subordinate_ratio);
            return ac;
        }


        public void readNanpinStrategyData(List<int> select_strategy_ids)
        {
            var selected_strategy_file_ind = getOptNanpinPLOrder(select_strategy_ids, "opt nanpin.csv");
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                int no = 0;
                int target_no = 0;
                while ((data = sr.ReadLine()) != null)
                {
                    if (selected_strategy_file_ind.Values.Contains(no))
                    {
                        var ele = data.Split(',');
                        //"No.,num trade,win rate,total pl,realized pl,realzied pl var,total capital var,sharp ratio,total capital gradient,window pl ratio,pt,lc,num_split,func,ma_term,strategy id,nanpin timing,lot splits"
                        para_pt[target_no] = Convert.ToDouble(ele[10]);
                        para_lc[target_no] = Convert.ToDouble(ele[11]);
                        para_ma_term[target_no] = Convert.ToInt32(ele[14]);
                        para_strategy[target_no] = Convert.ToInt32(ele[15]);
                        para_rapid_side_change_ratio[target_no] = Convert.ToDouble(ele[16]);
                        para_nanpin_timing[target_no] = ele[17].Split(':').Select(double.Parse).ToArray().ToList();
                        para_nanpin_lot[target_no] = ele[18].Split(':').Select(double.Parse).ToArray().ToList();
                        target_no++;
                    }
                    no++;
                }
            }
        }

        private Dictionary<int, int> getOptNanpinPLOrder(List<int> select_strategy_ids, string file_path)
        {
            var res = new Dictionary<int, int>();
            var pl = new Dictionary<int, double>();
            var n = 0;
            using (var sr = new StreamReader(file_path))
            {
                sr.ReadLine();
                var data = "";
                while ((data = sr.ReadLine()) != null)
                {
                    pl[n] = Convert.ToDouble(data.Split(',')[3]);
                    n++;
                }
            }
            IOrderedEnumerable<KeyValuePair<int, double>> sorted = pl.OrderByDescending(pair => pair.Value);
            var selected_inds = new List<int>();
            n = 0;
            foreach (var data in sorted)
            {
                if (select_strategy_ids.Contains(data.Key))
                    res[data.Key] = n;
                n++;
            }
            return res;
        }

        private void readSelectStrategyData(int id)
        {
            var id_map = getOptNanpinPLOrder(new List<int>() { id }, "opt nanpin select.csv");
            using (var sr = new StreamReader("opt nanpin select.csv"))
            {
                //No.	num trade	win rate	total pl	realized pl	realzied pl sd	total capital sd	sharp ratio	total capital gradient	window pl ratio	num select change	timing time window	strategy time window	timing subordinate ratio
                var data = sr.ReadLine();
                int no = 0;
                while ((data = sr.ReadLine()) != null)
                {
                    if (id_map.Values.Contains(no))
                    {
                        var ele = data.Split(',');
                        Console.WriteLine("total pl =" + ele[3] + ", win rate=" + ele[2] + ", num select change=" + ele[10]);
                        select_strategy_time_window = Convert.ToInt32(ele[11]);
                        select_timing_time_window = Convert.ToInt32(ele[12]);
                        select_timing_subordinate_ratio = Convert.ToDouble(ele[13]);
                        break;
                    }
                    no++;
                }
            }
        }


        private Account doSim(ref string lev_or_fixed, int strategy, ref int from, ref int to, double pt, double lc, List<double> nanpint_timing, List<double> nanpin_lot, int ma_term, double rapid_side)
        {
            var sim = new Sim();
            if (strategy == 0)
                return sim.sim_madiv_nanpin_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, true);
            else
                return sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, ref rapid_side);
        }


        private Account doSelectSim(string lev_or_fixed, int from, int to, List<Account> strategy_ac_list, ConcurrentDictionary<int, double> para_pt,
            ConcurrentDictionary<int, double> para_lc, ConcurrentDictionary<int, int> para_ma_term, ConcurrentDictionary<int, int> para_strategy_id,
            ConcurrentDictionary<int, double> para_rapid_side_change_ratio, ConcurrentDictionary<int, List<double>> para_nanpin_timing,
            ConcurrentDictionary<int, List<double>> para_nanpin_lot, int time_window, int strategy_time_window, double subordinate_ratio)
        {
            var sim = new Sim();
            return sim.sim_select_strategy(from, to, ref strategy_ac_list, new Account(lev_or_fixed, false), ref para_pt, ref para_lc, ref para_ma_term, ref para_strategy_id, ref para_rapid_side_change_ratio, ref para_nanpin_timing, ref para_nanpin_lot,
                ref time_window, ref strategy_time_window, ref subordinate_ratio);

        }
    }
}
