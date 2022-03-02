using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BTCSIM2
{
    public class OptFiltering
    {
        public ConcurrentDictionary<int, double> res_total_capital { get; set; }
        public ConcurrentDictionary<int, double> res_total_pl_ratio { get; set; }
        public ConcurrentDictionary<int, double> res_win_rate { get; set; }
        public ConcurrentDictionary<int, int> res_num_trade { get; set; }
        public ConcurrentDictionary<int, int> res_num_buy { get; set; }
        public ConcurrentDictionary<int, int> res_num_sell { get; set; }
        public ConcurrentDictionary<int, double> res_ave_buy_pl { get; set; }
        public ConcurrentDictionary<int, double> res_ave_sell_pl { get; set; }
        public ConcurrentDictionary<int, double> res_realized_pl_sd { get; set; }
        public ConcurrentDictionary<int, double> res_total_capital_sd { get; set; }
        public ConcurrentDictionary<int, double> res_window_pl_ratio { get; set; }
        public ConcurrentDictionary<int, string> res_write_contents { get; set; }

        public ConcurrentDictionary<int, int> para_filter_id { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_window { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_suspension { get; set; }
        public ConcurrentDictionary<int, double> para_kijun_change { get; set; }

        public double opt_para_pt { get; set; }
        public double opt_para_lc { get; set; }
        public int opt_para_num_split { get; set; }
        public int opt_para_func { get; set; }
        public int opt_para_ma_term { get; set; }
        public int opt_para_strategy { get; set; }
        public double opt_para_rapid_side_change_ratio { get; set; }
        public List<double> opt_para_nanpin_timing { get; set; }
        public List<double> opt_para_nanpin_lot { get; set; }


        private void initialize()
        {
            res_total_capital = new ConcurrentDictionary<int, double>();
            res_total_pl_ratio = new ConcurrentDictionary<int, double>();
            res_win_rate = new ConcurrentDictionary<int, double>();
            res_num_trade = new ConcurrentDictionary<int, int>();
            res_num_buy = new ConcurrentDictionary<int, int>();
            res_num_sell = new ConcurrentDictionary<int, int>();
            res_ave_buy_pl = new ConcurrentDictionary<int, double>();
            res_ave_sell_pl = new ConcurrentDictionary<int, double>();
            res_realized_pl_sd = new ConcurrentDictionary<int, double>();
            res_total_capital_sd = new ConcurrentDictionary<int, double>();
            res_window_pl_ratio = new ConcurrentDictionary<int, double>();
            res_write_contents = new ConcurrentDictionary<int, string>();

            para_filter_id = new ConcurrentDictionary<int, int>();
            para_kijun_time_window = new ConcurrentDictionary<int, int>();
            para_kijun_time_suspension = new ConcurrentDictionary<int, int>();
            para_kijun_change = new ConcurrentDictionary<int, double>();

            opt_para_pt = new double();
            opt_para_lc = new double();
            opt_para_num_split = new int();
            opt_para_func = new int();
            opt_para_ma_term = new int();
            opt_para_strategy = new int();
            opt_para_rapid_side_change_ratio = new double();
            opt_para_nanpin_timing = new List<double>();
            opt_para_nanpin_lot = new List<double>();
        }


        public OptFiltering()
        {
            initialize();
        }


        ~OptFiltering()
        {
            initialize();
        }



        public void startOptFiltering(int target_opt_id, int num_opt_loop, string lev_or_fixed, int from, int to)
        {
            readOptData(target_opt_id);
            para_filter_id = generateFilterID();
            para_kijun_time_window = generateKijunTimeWindow();
            para_kijun_time_suspension = generateKijunTimeSuspension();
            para_kijun_change = generateKijunChange();
            var combi = generateParamIndCombination(num_opt_loop, para_filter_id, para_kijun_time_window, para_kijun_time_suspension, para_kijun_change);

            using (StreamWriter writer = new StreamWriter("opt param filter.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                var progress = 0.0;
                var n = 0.0;
                sw.WriteLine("No.,num trade,win rate,total pl,realized pl,realzied pl sd,total capital sd,sharp ratio,total capital gradient,window pl ratio,pt,lc,num_split,func,ma_term,strategy id,rapid side change ratio,nanpin timing,lot splits,filter id,kijun time window,kijun change,kijun time suspension");
                var ac_list = new ConcurrentDictionary<int, Account>();
                Parallel.For(0, num_opt_loop, i =>
                {
                    ac_list.TryAdd(i, doSim(ref lev_or_fixed, opt_para_strategy, ref from, ref to, opt_para_pt, opt_para_lc, opt_para_nanpin_timing, opt_para_nanpin_lot,
                        opt_para_ma_term, opt_para_rapid_side_change_ratio, para_filter_id[combi[i][0]], para_kijun_time_window[combi[i][1]], para_kijun_change[combi[i][2]],
                        para_kijun_time_suspension[combi[i][3]]));
                    res_total_capital.TryAdd(i, ac_list[i].performance_data.total_capital);
                    res_total_pl_ratio.TryAdd(i, ac_list[i].performance_data.total_pl_ratio);
                    res_win_rate.TryAdd(i, ac_list[i].performance_data.win_rate);
                    res_num_trade.TryAdd(i, ac_list[i].performance_data.num_trade);
                    res_num_buy.TryAdd(i, ac_list[i].performance_data.num_buy);
                    res_num_sell.TryAdd(i, ac_list[i].performance_data.num_sell);
                    if (ac_list[i].performance_data.buy_pl_ratio_list.Count > 0)
                        res_ave_buy_pl.TryAdd(i, ac_list[i].performance_data.buy_pl_ratio_list.Average());
                    else
                        res_ave_buy_pl[i] = 0;
                    if (ac_list[i].performance_data.sell_pl_ratio_list.Count > 0)
                        res_ave_sell_pl.TryAdd(i, ac_list[i].performance_data.sell_pl_ratio_list.Average());
                    else
                        res_ave_sell_pl.TryAdd(i, 0);
                    res_realized_pl_sd.TryAdd(i, ac_list[i].performance_data.realized_pl_ratio_sd);
                    res_total_capital_sd.TryAdd(i, ac_list[i].performance_data.total_capital_sd);
                    res_window_pl_ratio.TryAdd(i, ac_list[i].performance_data.window_pl_ratio);
                    res_write_contents.TryAdd(i, i.ToString() + "," + ac_list[i].performance_data.num_trade.ToString() + "," +
                        ac_list[i].performance_data.win_rate.ToString() + "," +
                        ac_list[i].performance_data.total_pl.ToString() + "," +
                        ac_list[i].performance_data.realized_pl.ToString() + "," +
                        ac_list[i].performance_data.realized_pl_ratio_sd.ToString() + "," +
                        ac_list[i].performance_data.total_capital_sd.ToString() + "," +
                        ac_list[i].performance_data.sharp_ratio.ToString() + "," +
                        ac_list[i].performance_data.total_capital_gradient.ToString() + "," +
                        ac_list[i].performance_data.window_pl_ratio.ToString()+","+
                        opt_para_pt.ToString() +","+opt_para_lc.ToString()+","+opt_para_num_split.ToString()+","+ opt_para_func.ToString()+","+ opt_para_ma_term.ToString()+","+opt_para_strategy.ToString()+","+
                        opt_para_rapid_side_change_ratio.ToString()+","+string.Join(":", opt_para_nanpin_timing)+","+string.Join(":", opt_para_nanpin_lot)+","+
                        para_filter_id[combi[i][0]].ToString()+","+ para_kijun_time_window[combi[i][1]].ToString()+","+
                        para_kijun_change[combi[i][2]].ToString()+","+ para_kijun_time_suspension[combi[i][3]].ToString());
                    sw.WriteLine(res_write_contents[i]);
                    n++;
                    progress = Math.Round(100.0 * n / Convert.ToDouble(num_opt_loop), 2);
                    Console.WriteLine(n.ToString() + "/" + num_opt_loop.ToString() + " - " + progress.ToString() + "%" +
                        ": pl ratio=" + ac_list[i].performance_data.total_pl_ratio.ToString() +
                        ", sharp ratio=" + ac_list[i].performance_data.sharp_ratio.ToString() +
                        ", win rate=" + ac_list[i].performance_data.win_rate.ToString());
                    ac_list.TryRemove(i, out var d);
                    res_write_contents.TryRemove(i, out var dd);
                });
            }
        }


        private void readOptData(int opt_strategy_id)
        {
            var rs = new ReadSim();
            var best_pl_list = rs.generateBestPlIndList(opt_strategy_id + 1);
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                for (int i = 0; i < best_pl_list[opt_strategy_id]+1; i++)
                    data = sr.ReadLine();
                var ele = data.Split(',');
                opt_para_pt = Convert.ToDouble(ele[10]);
                opt_para_lc = Convert.ToDouble(ele[11]);
                opt_para_num_split = Convert.ToInt32(ele[12]);
                opt_para_func = Convert.ToInt32(ele[13]);
                opt_para_ma_term = Convert.ToInt32(ele[14]);
                opt_para_strategy = Convert.ToInt32(ele[15]);
                opt_para_rapid_side_change_ratio = Convert.ToDouble(ele[16]);
                opt_para_nanpin_timing = ele[17].Split(':').Select(double.Parse).ToArray().ToList();
                opt_para_nanpin_lot = ele[18].Split(':').Select(double.Parse).ToArray().ToList();
            }
        }

        private ConcurrentDictionary<int, int> generateFilterID()
        {
            var res = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < 3; i++)
                res.TryAdd(i, i);
            return res;
        }

        private ConcurrentDictionary<int, int> generateKijunTimeWindow()
        {
            var res = new ConcurrentDictionary<int, int>();
            int min_window = 5;
            int max_window = 60;
            var window = min_window;
            var skip = 5;
            var i = 0;
            while (true)
            {
                res.TryAdd(i, window);
                window += skip;
                if (window >= max_window)
                    break;
                i++;
            }
            return res;
        }

        private ConcurrentDictionary<int, int> generateKijunTimeSuspension()
        {
            var res = new ConcurrentDictionary<int, int>();
            int min_window = 1;
            int max_window = 60;
            var window = min_window;
            var skip = 1;
            var i = 0;
            while (true)
            {
                res.TryAdd(i, window);
                window += skip;
                if (window >= max_window)
                    break;
                i++;
            }
            return res;
        }

        private ConcurrentDictionary<int, double> generateKijunChange()
        {
            var res = new ConcurrentDictionary<int, double>();
            var min_change = 0.001;
            var max_change = 0.1;
            var skip = 0.001;
            var ratio = min_change;
            var i = 0;
            while (true)
            {
                res.TryAdd(i, ratio);
                ratio += skip;
                if (ratio >= max_change)
                    break;
                i++;
            }
            return res;
        }


        private ConcurrentDictionary<int, List<int>> generateParamIndCombination(int num_select, ConcurrentDictionary<int, int> filter_id, ConcurrentDictionary<int, int> kijun_time_window,
            ConcurrentDictionary<int, int> kijun_time_suspension, ConcurrentDictionary<int, double> kijun_change)
        {
            var all_combination = new ConcurrentDictionary<int, List<int>>();
            var ids = new List<int>();
            var n = 0;
            for(int i=0; i<filter_id.Count; i++)
            {
                for(int j=0; j<kijun_time_window.Count; j++)
                {
                    for(int k=0; k< kijun_change.Count; k++)
                    {
                        for(int l=0; l< kijun_time_suspension.Count; l++)
                        {
                            all_combination.TryAdd(n, new List<int> { i,j,k,l});
                            ids.Add(n);
                            n++;
                        }
                    }
                }
            }
            var res = new ConcurrentDictionary<int, List<int>>();
            var ran = new Random();
            for (int i = 0; i < num_select; i++)
            {
                var selected = ran.Next(ids.Count);
                res.TryAdd(i, all_combination[ids[selected]]);
                ids.RemoveAt(selected);
            }
            return res;
        }


        private Account doSim(ref string lev_or_fixed, int strategy, ref int from, ref int to, double pt, double lc, List<double> nanpint_timing, List<double> nanpin_lot,
            int ma_term, double rapid_side_change_ratio, int filter_id, int kijun_time_window, double kijun_change, int kijun_time_suspension)
        {
            var sim = new Sim();
            if (strategy == 0)
                return sim.sim_madiv_nanpin_filter_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, ref filter_id, ref kijun_time_window, ref kijun_change, ref kijun_time_suspension);
            else
                return sim.sim_madiv_nanpin_rapid_side_change_filter_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot,
                    ref ma_term, ref rapid_side_change_ratio, ref filter_id, ref kijun_time_window, ref kijun_change, ref kijun_time_suspension);
        }
    }
}
