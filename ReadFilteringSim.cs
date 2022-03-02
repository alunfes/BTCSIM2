using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace BTCSIM2
{
    public class ReadFilteringSim
    {
        public ConcurrentDictionary<int, double> res_total_capital { get; set; }
        public ConcurrentDictionary<int, double> res_total_pl { get; set; }
        public ConcurrentDictionary<int, double> res_total_pl_ratio { get; set; }
        public ConcurrentDictionary<int, double> res_win_rate { get; set; }
        public ConcurrentDictionary<int, int> res_num_trade { get; set; }
        public ConcurrentDictionary<int, int> res_num_buy { get; set; }
        public ConcurrentDictionary<int, int> res_num_sell { get; set; }
        public ConcurrentDictionary<int, double> res_ave_buy_pl { get; set; }
        public ConcurrentDictionary<int, double> res_ave_sell_pl { get; set; }
        public ConcurrentDictionary<int, double> res_realized_pl { get; set; }
        public ConcurrentDictionary<int, double> res_realized_pl_sd { get; set; }
        public ConcurrentDictionary<int, double> res_total_capital_sd { get; set; }
        public ConcurrentDictionary<int, double> res_sharp_ratio { get; set; }
        public ConcurrentDictionary<int, double> res_total_capital_gradient { get; set; }
        public ConcurrentDictionary<int, List<double>> res_total_capital_list { get; set; }
        public ConcurrentDictionary<int, List<int>> res_num_trade_list { get; set; }
        public ConcurrentDictionary<int, double> res_window_pl_ratio { get; set; }
        public ConcurrentDictionary<int, string> res_write_contents { get; set; }

        public ConcurrentDictionary<int, double> opt_total_pl { get; set; }
        public ConcurrentDictionary<int, double> opt_realized_pl { get; set; }
        public ConcurrentDictionary<int, double> opt_win_rate { get; set; }
        public ConcurrentDictionary<int, double> opt_num_trade { get; set; }
        public ConcurrentDictionary<int, double> opt_realized_pl_sd { get; set; }
        public ConcurrentDictionary<int, double> opt_total_capital_sd { get; set; }
        public ConcurrentDictionary<int, double> opt_sharp_ratio { get; set; }
        public ConcurrentDictionary<int, double> opt_total_capital_gradient { get; set; }
        public ConcurrentDictionary<int, double> opt_window_pl_ratio { get; set; }


        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, int> para_num_split { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }
        public ConcurrentDictionary<int, int> para_filter_id { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_window { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_suspension { get; set; }
        public ConcurrentDictionary<int, double> para_kijun_change { get; set; }


        public ReadFilteringSim()
        {
            res_total_capital = new ConcurrentDictionary<int, double>();
            res_total_pl = new ConcurrentDictionary<int, double>();
            res_total_pl_ratio = new ConcurrentDictionary<int, double>();
            res_win_rate = new ConcurrentDictionary<int, double>();
            res_num_trade = new ConcurrentDictionary<int, int>();
            res_num_buy = new ConcurrentDictionary<int, int>();
            res_num_sell = new ConcurrentDictionary<int, int>();
            res_ave_buy_pl = new ConcurrentDictionary<int, double>();
            res_ave_sell_pl = new ConcurrentDictionary<int, double>();
            res_realized_pl = new ConcurrentDictionary<int, double>();
            res_realized_pl_sd = new ConcurrentDictionary<int, double>();
            res_total_capital_sd = new ConcurrentDictionary<int, double>();
            res_total_capital_list = new ConcurrentDictionary<int, List<double>>();
            res_num_trade_list = new ConcurrentDictionary<int, List<int>>();
            res_sharp_ratio = new ConcurrentDictionary<int, double>();
            res_total_capital_gradient = new ConcurrentDictionary<int, double>();
            res_window_pl_ratio = new ConcurrentDictionary<int, double>();
            res_write_contents = new ConcurrentDictionary<int, string>();

            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_num_split = new ConcurrentDictionary<int, int>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();

            opt_total_pl = new ConcurrentDictionary<int, double>();
            opt_realized_pl = new ConcurrentDictionary<int, double>();
            opt_win_rate = new ConcurrentDictionary<int, double>();
            opt_num_trade = new ConcurrentDictionary<int, double>();
            opt_realized_pl_sd = new ConcurrentDictionary<int, double>();
            opt_total_capital_sd = new ConcurrentDictionary<int, double>();
            opt_sharp_ratio = new ConcurrentDictionary<int, double>();
            opt_total_capital_gradient = new ConcurrentDictionary<int, double>();
            opt_window_pl_ratio = new ConcurrentDictionary<int, double>();

            para_filter_id = new ConcurrentDictionary<int, int>();
            para_kijun_time_window = new ConcurrentDictionary<int, int>();
            para_kijun_time_suspension = new ConcurrentDictionary<int, int>();
            para_kijun_change = new ConcurrentDictionary<int, double>();
        }



        public void startReadSim(int from, int to, string lev_or_fixed)
        {
            //read param data
            
            //do sim
            using (StreamWriter writer = new StreamWriter("read filter sim.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                sw.WriteLine("No,pt,lc,ma term,strategy id,rapid side change ratio,nanpin timing,nanpin lot,filter id,kijun time window,kijun change,kijun time suspension,opt total pl,opt realized pl,opt realized pl sd,opt total capital sd,opt num trade,opt win rate," +
                    "opt sharp ratio,opt total capital gradient,opt window pl ratio,test total pl,test realized pl,test realized pl sd,test total capital sd,test num trade,test win rate," +
                    "test sharp ratio,test total capital gradient,test window pl ratio");
                var progress = 0.0;
                var no = 0;
                var ac_list = new ConcurrentDictionary<int, Account>();
                Parallel.For(0, para_pt.Count, i =>
                {
                    ac_list.TryAdd(i, doSim(ref lev_or_fixed,
                            para_strategy[i],
                            ref from,
                            ref to,
                            para_pt[i],
                            para_lc[i],
                            para_nanpin_timing[i],
                            para_nanpin_lot[i],
                            para_ma_term[i],
                            para_rapid_side_change_ratio[i],
                            para_filter_id[i],
                            para_kijun_time_window[i],
                            para_kijun_change[i],
                            para_kijun_time_suspension[i]));
                    res_total_capital.TryAdd(i, ac_list[i].performance_data.total_capital);
                    res_total_pl.TryAdd(i, ac_list[i].performance_data.total_pl);
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
                    res_realized_pl.TryAdd(i, ac_list[i].performance_data.realized_pl);
                    res_realized_pl_sd.TryAdd(i, ac_list[i].performance_data.realized_pl_ratio_sd);
                    res_total_capital_sd.TryAdd(i, ac_list[i].performance_data.total_capital_sd);
                    res_sharp_ratio.TryAdd(i, ac_list[i].performance_data.sharp_ratio);
                    res_total_capital_gradient.TryAdd(i, ac_list[i].performance_data.total_capital_gradient);
                    res_window_pl_ratio.TryAdd(i, ac_list[i].performance_data.window_pl_ratio);

                    res_write_contents.TryAdd(i, i.ToString() + "," + para_pt[i].ToString() + "," +
                    para_lc[i].ToString() + "," + para_ma_term[i].ToString() + "," + para_strategy[i].ToString() + "," + para_rapid_side_change_ratio[i].ToString() + "," +
                    string.Join(":", para_nanpin_timing[i]) + "," + string.Join(":", para_nanpin_lot[i]) + "," +
                    para_filter_id[i].ToString()+","+para_kijun_time_window[i].ToString()+","+para_kijun_change[i].ToString()+","+para_kijun_time_suspension[i].ToString()+","+
                    opt_total_pl[i].ToString() + "," + opt_realized_pl[i].ToString() + "," +
                    opt_realized_pl_sd[i].ToString() + "," + opt_total_capital_sd[i].ToString() + "," +
                    opt_num_trade[i].ToString() + "," + opt_win_rate[i].ToString() + "," +
                    opt_sharp_ratio[i].ToString() + "," + opt_total_capital_gradient[i].ToString() + "," + opt_window_pl_ratio[i].ToString() + "," +
                    res_total_pl[i].ToString() + "," + res_realized_pl[i].ToString() + "," +
                    res_realized_pl_sd[i].ToString() + "," + res_total_capital_sd[i].ToString() + "," +
                    res_num_trade[i].ToString() + "," + res_win_rate[i].ToString() + "," +
                    res_sharp_ratio[i].ToString() + "," + res_total_capital_gradient[i].ToString() + "," +
                    res_window_pl_ratio[i].ToString());
                    progress = Math.Round(100.0 * Convert.ToDouble(no) / Convert.ToDouble(para_lc.Count), 2);
                    sw.WriteLine(res_write_contents[i]);
                    Console.WriteLine(no.ToString() + "/" + para_lc.Count.ToString() + " - " + progress.ToString() + "%" +
                        ": test total pl=" + res_total_pl[i].ToString() +
                        ", test sharp ratio=" + res_sharp_ratio[i].ToString() +
                        ", test win rate=" + res_win_rate[i].ToString());
                    ac_list.TryRemove(i, out var d);
                    res_write_contents.TryRemove(i, out var dd);
                    no++;
                });
            }
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


        private void readOptFilteringData()
        {
            using (var sr = new StreamReader("opt param filter.csv"))
            {
                var data = sr.ReadLine();
                int target_no = 0;
                while ((data = sr.ReadLine()) != null)
                {
                    var ele = data.Split(',');
                    //"No.,num trade,win rate,total pl,realized pl,realzied pl var,total capital var,sharp ratio,total capital gradient,window pl ratio,pt,lc,num_split,func,ma_term,strategy id,nanpin timing,lot splits"
                    opt_num_trade.TryAdd(target_no, Convert.ToInt32(ele[1]));
                    opt_win_rate.TryAdd(target_no, Convert.ToDouble(ele[2]));
                    opt_total_pl.TryAdd(target_no, Convert.ToDouble(ele[3]));
                    opt_realized_pl.TryAdd(target_no, Convert.ToDouble(ele[4]));
                    opt_realized_pl_sd.TryAdd(target_no, Convert.ToDouble(ele[5]));
                    opt_total_capital_sd.TryAdd(target_no, Convert.ToDouble(ele[6]));
                    opt_sharp_ratio.TryAdd(target_no, Convert.ToDouble(ele[7]));
                    opt_total_capital_gradient.TryAdd(target_no, Convert.ToDouble(ele[8]));
                    opt_window_pl_ratio.TryAdd(target_no, Convert.ToDouble(ele[9]));

                    para_pt.TryAdd(target_no, Convert.ToDouble(ele[10]));
                    para_lc.TryAdd(target_no, Convert.ToDouble(ele[11]));
                    para_num_split.TryAdd(target_no, Convert.ToInt32(ele[12]));
                    para_ma_term.TryAdd(target_no, Convert.ToInt32(ele[14]));
                    para_strategy.TryAdd(target_no, Convert.ToInt32(ele[15]));
                    para_rapid_side_change_ratio.TryAdd(target_no, Convert.ToDouble(ele[16]));
                    para_nanpin_timing.TryAdd(target_no, ele[17].Split(':').Select(double.Parse).ToArray().ToList());
                    para_nanpin_lot.TryAdd(target_no, ele[18].Split(':').Select(double.Parse).ToArray().ToList());
                    para_filter_id.TryAdd(target_no, Convert.ToInt32(ele[19]));
                    para_kijun_time_window.TryAdd(target_no, Convert.ToInt32(ele[20]));
                    para_kijun_change.TryAdd(target_no, Convert.ToDouble(ele[21]));
                    para_kijun_time_suspension.TryAdd(target_no, Convert.ToInt32(ele[22]));
                    target_no++;
                }
            }
        }
    }
}
