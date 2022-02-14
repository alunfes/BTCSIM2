using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace BTCSIM2
{
    public class ReadSim
    {
        public Dictionary<int, double> res_total_capital { get; set; }
        public Dictionary<int, double> res_total_pl_ratio { get; set; }
        public Dictionary<int, double> res_win_rate { get; set; }
        public Dictionary<int, int> res_num_trade { get; set; }
        public Dictionary<int, int> res_num_buy { get; set; }
        public Dictionary<int, int> res_num_sell { get; set; }
        public Dictionary<int, double> res_ave_buy_pl { get; set; }
        public Dictionary<int, double> res_ave_sell_pl { get; set; }
        public Dictionary<int, double> res_realized_pl_var { get; set; }
        public Dictionary<int, double> res_total_capital_var { get; set; }
        public Dictionary<int, List<double>> res_total_capital_list { get; set; }
        public Dictionary<int, List<int>> res_num_trade_list { get; set; }

        public Dictionary<int, double> opt_total_pl { get; set; }
        public Dictionary<int, double> opt_realized_pl { get; set; }
        public Dictionary<int, double> opt_win_rate { get; set; }
        public Dictionary<int, double> opt_num_trade { get; set; }
        public Dictionary<int, double> opt_realized_pl_var { get; set; }
        public Dictionary<int, double> opt_total_capital_var { get; set; }
        public Dictionary<int, double> opt_sharp_ratio { get; set; }
        public Dictionary<int, double> opt_total_capital_gradient { get; set; }

        public Dictionary<int, double> para_pt { get; set; }
        public Dictionary<int, double> para_lc { get; set; }
        public Dictionary<int, int> para_ma_term { get; set; }
        public Dictionary<int, double[]> para_nanpin_timing { get; set; }
        public Dictionary<int, double[]> para_nanpin_lot { get; set; }


        public ReadSim()
        {
        }

        public void initialize()
        {
            res_total_capital = new Dictionary<int, double>();
            res_total_pl_ratio = new Dictionary<int, double>();
            res_win_rate = new Dictionary<int, double>();
            res_num_trade = new Dictionary<int, int>();
            res_num_buy = new Dictionary<int, int>();
            res_num_sell = new Dictionary<int, int>();
            res_ave_buy_pl = new Dictionary<int, double>();
            res_ave_sell_pl = new Dictionary<int, double>();
            res_realized_pl_var = new Dictionary<int, double>();
            res_total_capital_var = new Dictionary<int, double>();
            para_pt = new Dictionary<int, double>();
            para_lc = new Dictionary<int, double>();
            para_ma_term = new Dictionary<int, int>();
            para_nanpin_timing = new Dictionary<int, double[]>();
            para_nanpin_lot = new Dictionary<int, double[]>();
            opt_total_pl = new Dictionary<int, double>();
            opt_realized_pl = new Dictionary<int, double>();
            opt_win_rate = new Dictionary<int, double>();
            opt_num_trade = new Dictionary<int, double>();
            opt_realized_pl_var = new Dictionary<int, double>();
            opt_total_capital_var = new Dictionary<int, double>();
            opt_sharp_ratio = new Dictionary<int, double>();
            opt_total_capital_gradient = new Dictionary<int, double>();
            res_total_capital_list = new Dictionary<int, List<double>>();
            res_num_trade_list = new Dictionary<int, List<int>>();
        }


        /*
         * read opt nanpin.csv and conduct multi sim for top inscope_for_sim strategies
         */
        public void startMultiReadSim(int from, int to, int inscope_for_sim, string lev_or_fixed)
        {
            initialize();
            var opt_realized_pl = new Dictionary<int, double>();
            var n = 0;
            //read pl data in opt nanpin and sort by the pl val
            using(var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                while ((data = sr.ReadLine()) != null)
                {
                    opt_realized_pl.Add(n, Convert.ToDouble(data.Split(',')[3]));
                    n++;
                }
            }
            IOrderedEnumerable<KeyValuePair<int, double>> sorted = opt_realized_pl.OrderByDescending(pair => pair.Value);
            var selected_inds = new List<int>();
            foreach (var data in sorted)
            {
                selected_inds.Add(data.Key);
                if (selected_inds.Count >= inscope_for_sim)
                    break;
            }

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
        }


        public void startReadSim(int from, int to, int opt_term, string lev_or_fixed)
        {
            initialize();

            //read param data
            int no = 0;
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                while((data = sr.ReadLine()) != null)
                {
                    var ele = data.Split(',');
                    //"No.,num trade,win rate,total pl,realized pl,realzied pl var,total capital var,sharp ratio,total capital gradient,pt,lc,num_split,func,ma_term,nanpin timing,lot splits"
                    opt_num_trade.Add(no, Convert.ToDouble(ele[1]));
                    opt_win_rate.Add(no, Convert.ToDouble(ele[2]));
                    opt_total_pl.Add(no, Convert.ToDouble(ele[3]));
                    opt_realized_pl.Add(no, Convert.ToDouble(ele[4]));
                    opt_realized_pl_var.Add(no, Convert.ToDouble(ele[5]));
                    opt_total_capital_var.Add(no, Convert.ToDouble(ele[6]));
                    opt_sharp_ratio.Add(no, Convert.ToDouble(ele[7]));
                    opt_total_capital_gradient.Add(no, Convert.ToDouble(ele[8]));
                    para_pt.Add(no, Convert.ToDouble(ele[9]));
                    para_lc.Add(no, Convert.ToDouble(ele[10]));
                    para_ma_term.Add(no, Convert.ToInt32(ele[13]));
                    para_nanpin_timing.Add(no, ele[14].Split(':').Select(double.Parse).ToArray());
                    para_nanpin_lot.Add(no, ele[15].Split(':').Select(double.Parse).ToArray());
                    no++;
                }
            }
            //do sim
            using (var sw = new StreamWriter("read sim.csv",false))
            {
                sw.WriteLine("i,pt,lc,ma term,nanpin timing,nanpin lot,opt total pl,opt realized pl,opt realized pl var,opt total capital var,opt num trade,opt win rate,opt sharp ratio,opt total capital gradient,earning performance,num trade performance,win rate performance,sim realized pl var,sim total capital var");
                var progress = 0.0;
                var term_adjust = Convert.ToDouble(opt_term) / Convert.ToDouble((to - from));
                for (int i = 0; i < no; i++)
                {
                    var ac = new Account(lev_or_fixed, true);
                    var sim = new Sim();
                    ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, para_pt[i], para_lc[i], para_nanpin_timing[i].ToList(), para_nanpin_lot[i].ToList(), para_ma_term[i], true);
                    res_total_capital.Add(i, ac.performance_data.total_capital);
                    res_total_pl_ratio.Add(i, ac.performance_data.unrealized_pl_ratio);
                    res_win_rate.Add(i, ac.performance_data.win_rate);
                    res_num_trade.Add(i, ac.performance_data.num_trade);
                    res_num_buy.Add(i, ac.performance_data.num_buy);
                    res_num_sell.Add(i, ac.performance_data.num_sell);
                    res_ave_buy_pl.Add(i, ac.performance_data.buy_pl_ratio_list.Average());
                    res_ave_sell_pl.Add(i, ac.performance_data.sell_pl_ratio_list.Average());
                    res_realized_pl_var.Add(i, ac.performance_data.realized_pl_ratio_variance);
                    res_total_capital_var.Add(i, ac.performance_data.total_capital_variance);
                    var pl_performance = Math.Round(term_adjust * ac.performance_data.realized_pl,4);//Math.Round(term_adjust * ac.performance_data.realized_pl - opt_pl[i],4);
                    var num_trade_performance = Math.Round(term_adjust * ac.performance_data.num_trade,4); Math.Round(term_adjust * ac.performance_data.num_trade - opt_num_trade[i],4);
                    var win_rate_performance = Math.Round(ac.performance_data.win_rate, 4);//Math.Round(ac.performance_data.win_rate / opt_win_rate[i], 4);
                    var res = i.ToString() + "," + para_pt[i].ToString() + "," + para_lc[i].ToString() + "," + para_ma_term[i].ToString() + "," + string.Join(":", para_nanpin_timing[i]) + "," +
                        string.Join(":", para_nanpin_lot[i]) + "," + opt_total_pl[i].ToString() +","+opt_realized_pl[i].ToString()+","+ opt_realized_pl_var[i].ToString() +","+opt_total_capital_var[i].ToString()+","+
                        opt_num_trade[i].ToString() +","+opt_win_rate[i].ToString() +","+  opt_sharp_ratio[i].ToString() +","+ opt_total_capital_gradient[i].ToString()  +","+
                        pl_performance.ToString() +","+num_trade_performance.ToString()+ "," + win_rate_performance.ToString()
                        +","+ac.performance_data.realized_pl_ratio_variance.ToString()+","+ac.performance_data.total_capital_variance.ToString();
                    progress = Math.Round(100.0 * Convert.ToDouble(i) / Convert.ToDouble(no), 2);
                    sw.WriteLine(res);
                    Console.WriteLine(res);
                    Console.WriteLine(i.ToString() + "/" + no.ToString() + " - " + progress.ToString() + "%" + ": pl performance=" + pl_performance
                        +" ,win rate performance="+ win_rate_performance +" ,num trade parformance="+ num_trade_performance);
                }
            }
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
