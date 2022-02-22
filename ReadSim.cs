using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace BTCSIM2
{
    public class ReadSim
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
        public ConcurrentDictionary<int, string> res_write_contents { get; set; }

        public ConcurrentDictionary<int, double> opt_total_pl { get; set; }
        public ConcurrentDictionary<int, double> opt_realized_pl { get; set; }
        public ConcurrentDictionary<int, double> opt_win_rate { get; set; }
        public ConcurrentDictionary<int, double> opt_num_trade { get; set; }
        public ConcurrentDictionary<int, double> opt_realized_pl_sd { get; set; }
        public ConcurrentDictionary<int, double> opt_total_capital_sd { get; set; }
        public ConcurrentDictionary<int, double> opt_sharp_ratio { get; set; }
        public ConcurrentDictionary<int, double> opt_total_capital_gradient { get; set; }

        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, int> para_num_split { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }


        public ReadSim()
        {
        }

        public void initialize()
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
            res_write_contents = new ConcurrentDictionary<int, string>();

            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_num_split = new ConcurrentDictionary<int, int>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();
            para_strategy = new ConcurrentDictionary<int, int>();

            opt_total_pl = new ConcurrentDictionary<int, double>();
            opt_realized_pl = new ConcurrentDictionary<int, double>();
            opt_win_rate = new ConcurrentDictionary<int, double>();
            opt_num_trade = new ConcurrentDictionary<int, double>();
            opt_realized_pl_sd = new ConcurrentDictionary<int, double>();
            opt_total_capital_sd = new ConcurrentDictionary<int, double>();
            opt_sharp_ratio = new ConcurrentDictionary<int, double>();
            opt_total_capital_gradient = new ConcurrentDictionary<int, double>();            
        }


        /*
         * read opt nanpin.csv and conduct multi sim for top inscope_for_sim strategies
         */
        public void startMultiReadSim(int from, int to, int inscope_for_sim, string lev_or_fixed)
        {
            /*
            initialize();
            var opt_realized_pl = new Dictionary<int, double>();
            var n = 0;
            var selected_inds = generateBestPlIndList(inscope_for_sim);
            
            //read only for top inscope_for_sim
            n = 0;
            int m = 0;
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                while ((data = sr.ReadLine()) != null)
                {
                    if (selected_inds.Contains(n))
                    {
                        var ele = data.Split(',');
                        para_pt.Add(m, Convert.ToDouble(ele[6]));
                        para_lc.Add(m, Convert.ToDouble(ele[7]));
                        para_ma_term.Add(m, Convert.ToInt32(ele[10]));
                        para_nanpin_timing.Add(m, ele[11].Split(':').Select(double.Parse).ToArray());
                        para_nanpin_lot.Add(m, ele[12].Split(':').Select(double.Parse).ToArray());
                        m++;
                    }
                    n++;
                }
            }

            //do sim for all selected strategies
            
            var capital_list_ave = new double[to-from];
            var numtrade_list_ave = new int[to-from];
            var start_ind = 0;
            var end_ind = 0;
            var close_log = new List<double>();
            for (int i=0; i<m; i++)
            {
                
                var ac = new Account(lev_or_fixed, true);
                var sim = new Sim();
                ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, para_pt[i], para_lc[i], para_nanpin_timing[i].ToList(), para_nanpin_lot[i].ToList(), para_ma_term[i], true);
                start_ind = ac.start_ind;
                end_ind = ac.end_ind;
                close_log = ac.performance_data.log_close;
                res_total_capital.Add(i, ac.performance_data.total_capital);
                res_total_pl_ratio.Add(i, ac.performance_data.total_pl_ratio);
                res_win_rate.Add(i, ac.performance_data.win_rate);
                res_num_trade.Add(i, ac.performance_data.num_trade);
                res_num_buy.Add(i, ac.performance_data.num_buy);
                res_num_sell.Add(i, ac.performance_data.num_sell);
                res_ave_buy_pl.Add(i, ac.performance_data.buy_pl_ratio_list.Average());
                res_ave_sell_pl.Add(i, ac.performance_data.sell_pl_ratio_list.Average());
                res_realized_pl_var.Add(i, ac.performance_data.realized_pl_ratio_variance);
                res_total_capital_var.Add(i, ac.performance_data.total_capital_variance);
                res_total_capital_list.Add(i, ac.performance_data.total_capital_list);
                res_num_trade_list.Add(i, ac.performance_data.num_trade_list);
                for (int j = 0; j < ac.performance_data.total_capital_list.Count; j++)
                    capital_list_ave[j] += ac.performance_data.total_capital_list[j];
                for (int j = 0; j < ac.performance_data.num_trade_list.Count; j++)
                    numtrade_list_ave[j] += ac.performance_data.num_trade_list[j];
                Console.WriteLine("No."+i.ToString()+", pl="+ac.performance_data.realized_pl+", win rate="+ac.performance_data.win_rate+", num trade="+ac.performance_data.num_trade);
            }
            Console.WriteLine("Num Multi Sim=" + m);
            Console.WriteLine("Ave PL ratio=" + Math.Round(res_total_pl_ratio.Values.ToList().Average(),4));
            Console.WriteLine("Ave Num Trade=" + Math.Round(res_num_trade.Values.ToList().Average(), 4));
            Console.WriteLine("Ave Win Rate=" + Math.Round(res_num_trade.Values.ToList().Average(), 4));
            Console.WriteLine("Ave Num buy=" + Math.Round(res_num_buy.Values.ToList().Average(), 4));
            Console.WriteLine("Ave Num sell=" + Math.Round(res_num_sell.Values.ToList().Average(), 4));
            Console.WriteLine("Ave Buy pl=" + Math.Round(res_ave_buy_pl.Values.ToList().Average(), 4));
            Console.WriteLine("Ave Sell pl=" + Math.Round(res_ave_sell_pl.Values.ToList().Average(), 4));
            displayMultiSimResult(start_ind, end_ind, "Multi MA Div Sim", m, capital_list_ave, numtrade_list_ave, close_log);
                */
        }


        public void startReadSim(int from, int to, int opt_term, string lev_or_fixed, int num_best_pl_for_test)
        {
            initialize();
            var selected_ind = generateBestPlIndList(num_best_pl_for_test); //get index of test targets
            //read param data
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                int no = 0;
                int target_no = 0;
                while ((data = sr.ReadLine()) != null)
                {
                    if (selected_ind.Contains(no))
                    {
                        var ele = data.Split(',');
                        //"No.,num trade,win rate,total pl,realized pl,realzied pl var,total capital var,sharp ratio,total capital gradient,pt,lc,num_split,func,ma_term,strategy id,nanpin timing,lot splits"
                        opt_num_trade[target_no] = Convert.ToInt32(ele[1]);
                        opt_win_rate[target_no] = Convert.ToDouble(ele[2]);
                        opt_total_pl[target_no] = Convert.ToDouble(ele[3]);
                        opt_realized_pl[target_no] = Convert.ToDouble(ele[4]);
                        opt_realized_pl_sd[target_no] = Convert.ToDouble(ele[5]);
                        opt_total_capital_sd[target_no] = Convert.ToDouble(ele[6]);
                        opt_sharp_ratio[target_no] = Convert.ToDouble(ele[7]);
                        opt_total_capital_gradient[target_no] = Convert.ToDouble(ele[8]);
                        para_pt[target_no] = Convert.ToDouble(ele[9]);
                        para_lc[target_no] = Convert.ToDouble(ele[10]);
                        para_num_split[target_no] = Convert.ToInt32(ele[11]);
                        para_ma_term[target_no] = Convert.ToInt32(ele[13]);
                        para_nanpin_timing[target_no] = ele[15].Split(':').Select(double.Parse).ToArray().ToList();
                        para_nanpin_lot[target_no] = ele[16].Split(':').Select(double.Parse).ToArray().ToList();
                        para_strategy[target_no] = Convert.ToInt32(ele[14]);
                        target_no++;
                    }
                    no++;
                }
            }
            //do sim
            using (StreamWriter writer = new StreamWriter("read sim.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                sw.WriteLine("No,pt,lc,ma term,strategy id,nanpin timing,nanpin lot,opt total pl,opt realized pl,opt realized pl sd,opt total capital sd,opt num trade,opt win rate," +
                    "opt sharp ratio,opt total capital gradient,test total pl,test realized pl,test realized pl sd,test total capital sd,test num trade,test win rate," +
                    "test sharp ratio,test total capital gradient");
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
                            para_ma_term[i]));
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

                    res_write_contents.TryAdd(i, i.ToString() + "," + para_pt[i].ToString() + "," +
                    para_lc[i].ToString() + "," + para_ma_term[i].ToString() + "," + para_strategy[i].ToString() +","+
                    string.Join(":", para_nanpin_timing[i]) + "," + string.Join(":", para_nanpin_lot[i]) + "," +
                    opt_total_pl[i].ToString() + "," + opt_realized_pl[i].ToString() + "," +
                    opt_realized_pl_sd[i].ToString() + "," + opt_total_capital_sd[i].ToString() + "," +
                    opt_num_trade[i].ToString() + "," + opt_win_rate[i].ToString() + "," +
                    opt_sharp_ratio[i].ToString() + "," + opt_total_capital_gradient[i].ToString() + "," +
                    res_total_pl[i].ToString() + "," + res_realized_pl[i].ToString() + "," +
                    res_realized_pl_sd[i].ToString() + "," + res_total_capital_sd[i].ToString() + "," +
                    res_num_trade[i].ToString() + "," + res_win_rate[i].ToString() + "," +
                    res_sharp_ratio[i].ToString() + "," + res_total_capital_gradient[i].ToString());
                    progress = Math.Round(100.0 * Convert.ToDouble(no) / Convert.ToDouble(para_lc.Count), 2);
                    sw.WriteLine(res_write_contents[i]);
                    Console.WriteLine(no.ToString() + "/" + para_lc.Count.ToString() + " - " + progress.ToString() + "%" +
                        ": test total pl=" + res_total_pl[i].ToString()+
                        ", test sharp ratio="+res_sharp_ratio[i].ToString()+
                        ", test win rate="+res_win_rate[i].ToString());
                    ac_list.TryRemove(i, out var d);
                    res_write_contents.TryRemove(i, out var dd);
                    no++;
                });
            }
        }

        private Account doSim(ref string lev_or_fixed, int strategy, ref int from, ref int to, double pt, double lc, List<double> nanpint_timing, List<double> nanpin_lot, int ma_term)
        {
            var sim = new Sim();
            if (strategy == 0)
                return sim.sim_madiv_nanpin_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, true);
            else
                return sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term);
        }

        public List<int> generateBestPlIndList(int num_select)
        {
            var pl = new Dictionary<int, double>();
            var n = 0;
            using (var sr = new StreamReader("opt nanpin.csv"))
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

        private void displayMultiSimResult(int start_ind, int end_ind, string title, int num_multi_sim, double[] capital_list, int[] num_trade_list, List<double> close)
        {
            Console.WriteLine("Ave pl ratio=" + Math.Round(res_total_pl_ratio.Values.Average(), 6));
            Console.WriteLine("Ave num trade=" + Math.Round(res_num_trade.Values.Average(), 6));
            Console.WriteLine("Ave win rate=" + Math.Round(res_win_rate.Values.Average(), 6));
            Console.WriteLine("Ave num_buy=" + Math.Round(res_num_buy.Values.Average(), 6));
            Console.WriteLine("Ave num_sell=" + Math.Round(res_num_sell.Values.Average(), 6));
            Console.WriteLine("Ave buy_pl=" + Math.Round(res_ave_buy_pl.Values.Average(), 6));
            Console.WriteLine("Ave sell_pl=" + Math.Round(res_ave_sell_pl.Values.Average(), 6));
            var table_labels = new List<string>() { "PL Ratio", "Num Trade", "Win Rate", "Ave num Buy", "Ave num Sell", "Ave Buy PL", "Ave Sell PL"};
            var table_data = new List<string>() { Math.Round(res_total_pl_ratio.Values.Average(), 6).ToString(), Math.Round(res_num_trade.Values.Average(), 6) .ToString(),
            Math.Round(res_win_rate.Values.Average(), 6).ToString(), Math.Round(res_num_buy.Values.Average(), 6).ToString(), Math.Round(res_num_sell.Values.Average(), 6).ToString(),
            Math.Round(res_ave_buy_pl.Values.Average(), 6).ToString(), Math.Round(res_ave_sell_pl.Values.Average(), 6).ToString()};
            for (int i = 0; i < capital_list.Length; i++)
                capital_list[i] = capital_list[i] / Convert.ToDouble(num_multi_sim);
            for(int j=0; j<num_trade_list.Length; j++)
                num_trade_list[j] = Convert.ToInt32(Math.Round(Convert.ToDouble(num_trade_list[j]) / Convert.ToDouble(num_multi_sim)));
            LineChart.DisplayLineChart3(capital_list.ToList(), close, num_trade_list.ToList(), table_labels, table_data, title + ": from=" + start_ind.ToString() + ", to=" + end_ind.ToString());
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }

    }
}
