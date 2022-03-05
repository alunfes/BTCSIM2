using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;



namespace BTCSIM2
{
    public class OptNanpinSelect
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

        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }

        public ConcurrentDictionary<int, int> select_timing_time_window { get; set; }
        public ConcurrentDictionary<int, int> select_timing_pre_time_window { get; set; }
        public ConcurrentDictionary<int, double> select_timing_subordinate_ratio { get; set; }
        public ConcurrentDictionary<int, int> select_strategy_time_window { get; set; }


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

            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();

            select_timing_time_window = new ConcurrentDictionary<int, int>();
            select_timing_pre_time_window = new ConcurrentDictionary<int, int>();
            select_timing_subordinate_ratio = new ConcurrentDictionary<int, double>();
            select_strategy_time_window = new ConcurrentDictionary<int, int>();
        }

        public OptNanpinSelect()
        {
            initialize();
        }

        ~OptNanpinSelect()
        {
            initialize();
        }

        //from should be higher than 10000
        public void startOptNanpinSelect(int from, int to, int num_opt, string lev_or_fixed)
        {
            readStrategyData();
            select_timing_time_window = generateTimingWindow();
            select_timing_pre_time_window = generateTimingWindow();
            select_strategy_time_window = generateTimingWindow();
            select_timing_subordinate_ratio = generateSubordinateRatio();
            var selected_param_combination = generateParamIndCombination(num_opt); //[select_timing_time_window, select_timing_pre_time_window, select_timing_subordinate_ratio, select_strategy_time_window]

            /*
             * do sim for all strategies from　i=max ma terms till from.
             * 
             */

            var strategy_ac_list = new List<Account>();
            var strategy_from = 500; //sim result should be availavle before the actual from of select strategy
            for (int i = 0; i < para_lc.Count; i++)
                strategy_ac_list.Add(doSim(ref lev_or_fixed, para_strategy[i], ref strategy_from, ref to, para_pt[i], para_lc[i], para_nanpin_timing[i], para_nanpin_lot[i], para_ma_term[i], para_rapid_side_change_ratio[i]));
            using (StreamWriter writer = new StreamWriter("opt nanpin select.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                var progress = 0.0;
                var n = 0.0;
                sw.WriteLine("No.,num trade,win rate,total pl,realized pl,realzied pl sd,total capital sd,sharp ratio,total capital gradient,window pl ratio,num select change," +
                    "timing time window,timing pre time window,strategy time window,timing subordinate ratio");
                var ac_list = new ConcurrentDictionary<int, Account>();
                Parallel.For(0, num_opt, i =>
                {
                    ac_list.TryAdd(i, doSelectSim(ref lev_or_fixed, from, to, strategy_ac_list, para_pt, para_lc, para_ma_term, para_strategy, para_rapid_side_change_ratio,
                        para_nanpin_timing, para_nanpin_lot, select_timing_time_window[selected_param_combination[i][0]], select_timing_pre_time_window[selected_param_combination[i][1]],
                        select_strategy_time_window[selected_param_combination[i][3]], select_timing_subordinate_ratio[selected_param_combination[i][2]]));
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
                    ac_list[i].performance_data.window_pl_ratio.ToString() + "," +
                    Convert.ToString(ac_list[i].passsing_info_from_sim["num select change"]) + "," +
                    select_timing_time_window[selected_param_combination[i][0]].ToString() + "," +
                    select_timing_pre_time_window[selected_param_combination[i][1]].ToString() + "," +
                    select_strategy_time_window[selected_param_combination[i][3]].ToString() + "," +
                    select_timing_subordinate_ratio[selected_param_combination[i][2]].ToString());
                    sw.WriteLine(res_write_contents[i]);
                    n++;
                    progress = Math.Round(100.0 * n / Convert.ToDouble(num_opt), 2);
                    Console.WriteLine(n.ToString() + "/" + num_opt.ToString() + " - " + progress.ToString() + "%" +
                        ": pl ratio=" + ac_list[i].performance_data.total_pl_ratio.ToString() +
                        ", sharp ratio=" + ac_list[i].performance_data.sharp_ratio.ToString() +
                        ", win rate=" + ac_list[i].performance_data.win_rate.ToString());
                    ac_list.TryRemove(i, out var d);
                    res_write_contents.TryRemove(i, out var dd);
                });
            }
        }

        private void readStrategyData()
        {
            var rs = new ReadSim();
            using (var sr = new StreamReader("nanpin select.csv"))
            {
                var data = sr.ReadLine();
                var i = 0;
                while ((data = sr.ReadLine()) != null)
                {
                    var ele = data.Split(',');
                    para_pt.TryAdd(i, Convert.ToDouble(ele[0]));
                    para_lc.TryAdd(i, Convert.ToDouble(ele[1]));
                    para_ma_term.TryAdd(i, Convert.ToInt32(ele[2]));
                    para_strategy.TryAdd(i, Convert.ToInt32(ele[3]));
                    para_rapid_side_change_ratio.TryAdd(i, Convert.ToDouble(ele[4]));
                    para_nanpin_timing.TryAdd(i, ele[5].Split(':').Select(double.Parse).ToArray().ToList());
                    para_nanpin_lot.TryAdd(i, ele[6].Split(':').Select(double.Parse).ToArray().ToList());
                    i++;
                }
            }
        }

        private ConcurrentDictionary<int, int> generateTimingWindow()
        {
            var res = new ConcurrentDictionary<int, int>();
            int start = 10;
            int end = 10000;
            int skip = 10;
            var i = 0;
            var d = start;
            while(true)
            {
                res.TryAdd(i, d);
                d += skip;
                i++;
                if (d > end)
                    break;
            }
            return res;
        }
        private ConcurrentDictionary<int, double> generateSubordinateRatio()
        {
            var res = new ConcurrentDictionary<int, double>();
            var start = 0.005;
            var end = 0.5;
            var skip = 0.005;
            var i = 0;
            var d = start;
            while (true)
            {
                res.TryAdd(i, d);
                d += skip;
                i++;
                if (d > end)
                    break;
            }
            return res;
        }



        //[select_timing_time_window, select_timing_pre_time_window, select_timing_subordinate_ratio, select_strategy_time_window]
        private ConcurrentDictionary<int, List<int>> generateParamIndCombination(int num)
        {
            var res = new ConcurrentDictionary<int, List<int>>();
            var ran = new Random();
            for (int i = 0; i < num; i++)
            {
                var ind_combi = new List<int> { ran.Next(0, select_timing_time_window.Count), ran.Next(0, select_timing_pre_time_window.Count), ran.Next(0, select_timing_subordinate_ratio.Count), ran.Next(0, select_strategy_time_window.Count)};
                res.TryAdd(i, ind_combi);
            }
            return res;
        }







        private Account doSim(ref string lev_or_fixed, int strategy, ref int from, ref int to, double pt, double lc, List<double> nanpint_timing, List<double> nanpin_lot, int ma_term, double rapid_side)
        {
            var sim = new Sim();
            if (strategy == 0)
                return sim.sim_madiv_nanpin_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, true);
            else
                return sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, ref rapid_side);
        }

        private Account doSelectSim(ref string lev_or_fixed, int from, int to, List<Account> strategy_ac_list, ConcurrentDictionary<int, double> para_pt,
            ConcurrentDictionary<int, double> para_lc, ConcurrentDictionary<int, int> para_ma_term, ConcurrentDictionary<int, int> para_strategy_id,
            ConcurrentDictionary<int, double> para_rapid_side_change_ratio, ConcurrentDictionary<int, List<double>> para_nanpin_timing,
            ConcurrentDictionary<int, List<double>> para_nanpin_lot, int time_window, int pre_time_window, int strategy_time_window, double subordinate_ratio)
        {
            var sim = new Sim();
            return sim.sim_select_strategy(from, to, ref strategy_ac_list, new Account(lev_or_fixed, true), ref para_pt, ref para_lc, ref para_ma_term, ref para_strategy_id, ref para_rapid_side_change_ratio, ref para_nanpin_timing, ref para_nanpin_lot,
                ref time_window, ref pre_time_window, ref strategy_time_window, ref subordinate_ratio);
            
        }


    }
}
