using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCSIM2
{
    class Program
    {
        private static void displaySimResult(Account ac, string title)
        {
            Console.WriteLine("pl=" + ac.performance_data.total_pl);
            Console.WriteLine("pl ratio=" + ac.performance_data.total_pl_ratio);
            Console.WriteLine("num trade=" + ac.performance_data.num_trade);
            Console.WriteLine("num market order=" + ac.performance_data.num_maker_order);
            Console.WriteLine("win rate=" + ac.performance_data.win_rate);
            Console.WriteLine("daily ave return=" + ac.performance_data.ave_daily_pl);
            Console.WriteLine("daily ave sd=" + ac.performance_data.daily_pl_sd);
            Console.WriteLine("sharp_ratio=" + ac.performance_data.sharp_ratio);
            Console.WriteLine("num_buy=" + ac.performance_data.num_buy);
            Console.WriteLine("num_sell=" + ac.performance_data.num_sell);
            Console.WriteLine("buy_pl=" + ac.performance_data.buy_pl_list.Sum());
            Console.WriteLine("sell_pl=" + ac.performance_data.sell_pl_list.Sum());
            Console.WriteLine("max_dd=" + ac.performance_data.max_dd);
            Console.WriteLine("max_pl=" + ac.performance_data.max_pl);
            Console.WriteLine("ave_holding_period=" + ac.holding_data.holding_period_list.Average());
            Console.WriteLine("total_capital_gradient=" + ac.performance_data.total_capital_gradient);
            Console.WriteLine("total_capital_gradient=" + ac.performance_data.window_pl_ratio);
            var table_labels = new List<string>() { "PL Ratio", "Num Trade", "Win Rate", "Daily Ave Return", "Daily Return SD", "Sharp Ratio", "Max DD", "Max PL", "Ave Buy PL", "Ave Sell PL", "Ave Holding Period", "Num Force Exit", "Total Capital Gradient", "Window PL Ratio" };
            var table_data = new List<string>() {Math.Round(ac.performance_data.total_pl_ratio,4).ToString(), ac.performance_data.num_trade.ToString(), Math.Round(ac.performance_data.win_rate,4).ToString(),
                ac.performance_data.ave_daily_pl.ToString(), ac.performance_data.daily_pl_sd.ToString(), ac.performance_data.sharp_ratio.ToString(), Math.Round(ac.performance_data.max_dd,4).ToString(),
            Math.Round(ac.performance_data.max_pl,4).ToString(), ac.performance_data.buy_pl_list.Count > 0 ? Math.Round(ac.performance_data.buy_pl_list.Average(), 4).ToString() : "0",
                ac.performance_data.sell_pl_list.Count >0 ? Math.Round(ac.performance_data.sell_pl_list.Average(), 4).ToString() : "0",
                ac.holding_data.holding_period_list.Count >0 ? Math.Round(ac.holding_data.holding_period_list.Average(),1).ToString() : "0",
                ac.performance_data.num_force_exit.ToString(), ac.performance_data.total_capital_gradient.ToString(), ac.performance_data.window_pl_ratio.ToString()};
            LineChart.DisplayLineChart3(ac.performance_data.total_capital_list.Values.ToList(), ac.performance_data.log_close, ac.performance_data.num_trade_list, table_labels, table_data, title + ": from=" + ac.start_ind.ToString() + ", to=" + ac.end_ind.ToString());
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }



        static void Main(string[] args)
        {

            var key = "";
            while (true)
            {
                Console.WriteLine("\"0: test\" : test");
                Console.WriteLine("\"1: ptlc\" : PTLC");
                Console.WriteLine("\"2: nanpin\" : PTLC Nanpin");
                Console.WriteLine("\"3: opt nanpin\" : Optimize nanpin parameters");
                Console.WriteLine("\"4: rand\" : Randome generated param nanpin sim");
                Console.WriteLine("\"5: multi nanpin\" : Multi param nanpin sim");
                Console.WriteLine("\"6: madiv nanpin\" : MA div nanpin sim");
                Console.WriteLine("\"7: read sim\" : Read MA div nanpin sim");
                Console.WriteLine("\"8: read multi\" : Read multi MA div nanpin sim");
                Console.WriteLine("\"9: opt select\" : Select Opt Nanpin id in best opt pl oreder");
                Console.WriteLine("\"10: opt filtering\" : optimize filtering for selected opt");
                Console.WriteLine("\"11: read filtering sim\" : Read Filtering Sim");
                Console.WriteLine("\"12: opt filtering select\" : Select opt filtering id in best opt pl order");
                Console.WriteLine("\"13: opt select strategy\" : Optimize strategy selection rule");
                Console.WriteLine("\"14: opt select select\" : Select select strategy id in bes opt pl order");
                Console.WriteLine("\"15: Conti Opt Sim\" : Conti Opt Sim");
                key = Console.ReadLine();

                if (Convert.ToInt32(key) >= 0 && Convert.ToInt32(key) <= 15)
                    break;
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("started program.");
            RandomSeed.initialize();
            List<int> terms = new List<int>() { 2, 3, 4, 5, 7, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 180, 190, 200, 220, 240, 260, 280 };
            //List<int> terms = new List<int>() { 2, 3, 4, 5, 7, 10, 14};
            MarketData.initializer(terms);

            var from = 510000;
            //var from = MarketData.Close.Count - 100000;
            //var to = 500000;
            var to = MarketData.Close.Count - 200000;
            var leveraged_or_fixed_trading = "leveraged";
            //var leveraged_or_fixed_trading = "fixed";
            var num_opt_calc = 100;
            var selected_opt_ids = new List<int>() { 0,5,10,15,20};


            /*
             * inner function to produce a nanpin timing and nanpin lot from pt, lc, num splits, select func
             * 
             */
            Dictionary<string, List<double[]>> getNanpinParam(double pt, double lc, int num_splits, int select_func_no)
            {
                var nanpin_lots = new Dictionary<string, List<double[]>>(); //napin lot name, nanpin timing, lot splilit
                if (num_splits > 1)
                {
                    //nanpin timing
                    var timing = new List<double>();
                    var unit = (lc - 0.01) / Convert.ToDouble(num_splits);
                    for (int i = 0; i < num_splits - 1; i++)
                        timing.Add(Math.Round(unit * (i + 1), 4));

                    //nanpin lot
                    var func_val = new List<double>();
                    var alloc = new List<double>();
                    var sampling_id = new List<int>();
                    var minl = 0.001; //minimal lot in each nanpin (not exact val but add to output val of functions)
                    if (select_func_no == 0) //y=x (x=1 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble((10)) / Convert.ToDouble(num_splits);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add((i * sampling_unit) + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 1) //y=-(x-10) (x=1 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble((10)) / Convert.ToDouble(num_splits);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(-(i * sampling_unit - 10) + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 2) //y=x^2 (x=1 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble((10)) / Convert.ToDouble(num_splits);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(Math.Pow(i * sampling_unit, 2.0) + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 3) //y=-x^2+100 (x=1 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble((10)) / Convert.ToDouble(num_splits);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(-Math.Pow(i * sampling_unit, 2.0) + 100 + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 4) //y=0.1x^2 (x= -10 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble(20) / Convert.ToDouble(num_splits - 1);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(-0.1 * Math.Pow((i * sampling_unit) - 10, 2.0) + 10 + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 5) //y=-0.1x^2 + 10 (x= -10 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble(20) / Convert.ToDouble(num_splits - 1);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(0.1 * Math.Pow((i * sampling_unit) - 10, 2.0) + 10 + minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 6) //fair allocation
                    {
                        var sampling_unit = 1.0 / Convert.ToDouble(num_splits);
                        for (int i = 0; i < num_splits; i++)
                            alloc.Add(Math.Round(sampling_unit, 6));
                    }
                    nanpin_lots.Add(num_splits.ToString() + "-" + select_func_no.ToString(), new List<double[]> { timing.ToArray(), alloc.ToArray() });
                }
                else
                    nanpin_lots.Add(num_splits.ToString() + "-" + select_func_no.ToString(), new List<double[]> { new double[] { }, new double[] { 1.0 } });
                return nanpin_lots;
            }

            if (key == "0")
            {
                Console.WriteLine("test");
                var d = new SortedDictionary<int, int>();
                d[0] = 1;
                d[1] = 2;
                d[2] = 3;
                Console.WriteLine(d.ToList()[0].Value);
            }
            if (key == "1")
            {
                Console.WriteLine("PTLC");
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                var pt_ratio = 0.01;
                var lc_ratio = 0.05;
                ac = sim.sim_ptlc(from, to, ac, pt_ratio, lc_ratio);
                displaySimResult(ac, "ptlc");
            }
            else if (key == "2")
            {
                Console.WriteLine("Nanpin PT/LC random buy sell entry");
                var nanpin_timing = new List<double>() { 0.01, 0.02, 0.03 };
                var lot_splits = new List<double>() { 0.1 };
                var pt_ratio = 0.013;
                var lc_ratio = 0.045;
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                ac = sim.sim_nanpin_ptlc(from, to, ac, pt_ratio, lc_ratio, nanpin_timing, lot_splits);
                displaySimResult(ac, "nanpin");
            }
            else if (key == "3")
            {
                Console.WriteLine("optimize nanpin strategy parameter");
                var o = new OptNanpin();
                o.startOptMADivNanpin(from, to, leveraged_or_fixed_trading, num_opt_calc, true);
            }
            else if (key == "4")
            {
                var r = new Random();
                var pt = Convert.ToDouble(r.Next(1, 11)) / 100.0;
                var lc = Convert.ToDouble(r.Next(1, 11)) / 100.0;
                var num_split = r.Next(1, 16);
                var func = r.Next(0, 7);
                var d = getNanpinParam(pt, lc, num_split, func);
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                ac = sim.sim_nanpin_ptlc(from, to, ac, pt, lc, d.Values.ToList()[0].ToList()[0].ToList(), d.Values.ToList()[0].ToList()[1].ToList());
                displaySimResult(ac, "nanpin");
            }
            else if (key == "5")
            {
                Console.WriteLine("Multi Nanpin PT/LC random buy sell entry");
                var nanpin_timing = new List<double[]>() { new double[] { 0.008, 0.016, 0.024, 0.032 }, new double[] { 0.0089,0.0178,0.0267,0.0356,0.0444,0.0533,0.0622,0.0711 }, new double[] { 0.0044,0.0089,0.0133,0.0178,0.0222,0.0267,0.0311,0.0356 },
                new double[] { 0.016,0.032,0.048,0.064}, new double[]{ 0.0089,0.0178,0.0267,0.0356,0.0444,0.0533,0.0622,0.0711} };
                var lot_splits = new List<double[]>() { new double[] { 0.00004,0.29998,0.39996,0.29998,0.00004 }, new double[] { 0.000004,0.004906,0.019611,0.04412,0.078433,0.122549,0.176468,0.240191,0.313718 }, new double[] { 0.15686,0.122548,0.09804,0.083335,0.078434,0.083335,0.09804,0.122548,0.15686 },
                new double[] {0.263157,0.252631,0.221052,0.168421,0.094738 }, new double[]{0.199984,0.177766,0.155548,0.133329,0.111111,0.088893,0.066675,0.044456,0.022238 } };
                var pt = new List<double>() { 0.01, 0.05, 0.01, 0.01, 0.01 };
                var lc = new List<double>() { 0.05, 0.09, 0.05, 0.09, 0.09 };
                var ac_list = new List<Account>();
                var sim = new Sim();
                for (int i = 0; i < pt.Count; i++)
                {
                    var ac = new Account(leveraged_or_fixed_trading, true);
                    var ac_res = sim.sim_nanpin_ptlc(from, to, ac, pt[i], lc[i], nanpin_timing[i].ToList(), lot_splits[i].ToList());
                    ac_list.Add(ac_res);
                }
                //consolidate multi nanpin results
                var consolidated_total_capital_list = new List<double>();
                var consolidated_num_trade = new List<int>();
                for (int i = 0; i < ac_list[0].performance_data.total_capital_list.Count; i++)
                {
                    var sum = 0.0;
                    var n = 0.0;
                    for (int j = 0; j < ac_list.Count; j++)
                    {
                        sum += ac_list[j].performance_data.total_capital_list[i];
                        n += ac_list[j].performance_data.num_trade_list[i];
                    }
                    consolidated_total_capital_list.Add(sum / Convert.ToDouble(ac_list.Count));
                    consolidated_num_trade.Add(Convert.ToInt32(Math.Round(n / Convert.ToDouble(ac_list.Count))));
                }
                var ave_pl = new List<double>();
                var ave_pl_ratio = new List<double>();
                var ave_win_rate = new List<double>();
                var ave_num_trade = new List<double>();
                var ave_buy_pl = new List<double>();
                var ave_sell_pl = new List<double>();
                var ave_holding_period = new List<double>();
                var ave_num_buy = new List<double>();
                var ave_num_sell = new List<double>();
                for (int i = 0; i < ac_list.Count; i++)
                {
                    ave_pl.Add(ac_list[i].performance_data.total_pl);
                    ave_pl_ratio.Add(ac_list[i].performance_data.total_pl_ratio);
                    ave_win_rate.Add(ac_list[i].performance_data.win_rate);
                    ave_num_trade.Add(ac_list[i].performance_data.num_trade);
                    ave_buy_pl.Add(ac_list[i].performance_data.buy_pl_list.Average());
                    ave_sell_pl.Add(ac_list[i].performance_data.sell_pl_list.Average());
                    ave_holding_period.Add(ac_list[i].holding_data.holding_period_list.Average());
                    ave_num_buy.Add(ac_list[i].performance_data.num_buy);
                    ave_num_sell.Add(ac_list[i].performance_data.num_sell);

                }
                Console.WriteLine("pl=" + Math.Round(ave_pl.Average(), 4));
                Console.WriteLine("pl ratio=" + Math.Round(ave_pl_ratio.Average(), 4));
                Console.WriteLine("num trade=" + Math.Round(ave_num_trade.Average(), 4));
                Console.WriteLine("win rate=" + Math.Round(ave_win_rate.Average(), 4));
                Console.WriteLine("num_buy=" + Math.Round(ave_num_buy.Average(), 4));
                Console.WriteLine("num_sell=" + Math.Round(ave_num_sell.Average(), 4));
                Console.WriteLine("ave_holding_period=" + Math.Round(ave_holding_period.Average(), 4));
                var table_labels = new List<string>() { "Ave PL", "Ave PL Ratio", "Ave Num Trade", "Ave Win Rate", "Ave Holding Period" };
                var table_data = new List<string>() { Math.Round(ave_pl.Average(), 4).ToString(), Math.Round(ave_pl_ratio.Average(), 4).ToString(), Math.Round(ave_num_trade.Average(), 4).ToString(),
                Math.Round(ave_win_rate.Average(), 4).ToString(), Math.Round(ave_holding_period.Average(), 4).ToString()};
                LineChart.DisplayLineChart3(consolidated_total_capital_list, MarketData.Close.GetRange(ac_list[0].start_ind, ac_list[0].end_ind).ToList(), consolidated_num_trade, table_labels, table_data, "from=" + ac_list[0].start_ind.ToString() + ", to=" + ac_list[0].end_ind.ToString());
                System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
            }
            //madiv nanpin sim
            else if (key == "6")
            {
                Console.WriteLine("MA div Nanpin PT/LC");
                var nanpintiming = "0.0038:0.0075:0.0113:0.015:0.0188:0.0226:0.0263:0.0301:0.0339:0.0376:0.0414:0.0451";
                var nanpinlot = "0.001:0.001646:0.002709:0.00446:0.00734:0.012082:0.019887:0.032735:0.053881:0.088689:0.145982:0.240286:0.39551";
                var nanpin_timing = nanpintiming.Split(':').Select(a => double.Parse(a)).ToList();
                var lot_splits = nanpinlot.Split(':').Select(a => double.Parse(a)).ToList();
                var pt_ratio = 0.050;
                var lc_ratio = 0.070;
                var ma_term = 100;
                var strategy_id = 1;
                var rapid_side_change_ratio = 0.3;
                //var contrarian = true;
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                if (strategy_id == 0)
                    ac = sim.sim_madiv_nanpin_ptlc(ref from, ref to, ac, ref pt_ratio, ref lc_ratio, ref nanpin_timing, ref lot_splits, ref ma_term, true);
                else
                    ac = sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, ac, ref pt_ratio, ref lc_ratio, ref nanpin_timing, ref lot_splits, ref ma_term, ref rapid_side_change_ratio);
                displaySimResult(ac, "MA Div nanpin");
            }
            else if (key == "7")
            {
                Console.WriteLine("Read MA div nanpin Sim");
                var read_sim_from = MarketData.Close.Count - 1000000;
                var read_sim_to = MarketData.Close.Count - 500000;
                var num_best_pl_for_test = 100;
                var rsim = new ReadSim();
                rsim.startReadSim(read_sim_from, read_sim_to, leveraged_or_fixed_trading, num_best_pl_for_test);

            }
            else if (key == "8")
            {
                Console.WriteLine("Read multi MA div nanpin Sim");
                var rs = new ReadSim();
                rs.startMultiReadSim(from, to, 5, leveraged_or_fixed_trading);
            }
            else if (key == "9")
            {
                Console.WriteLine("Please input opt strategy id for test.");
                var d = Console.ReadLine();
                var opt_ind = 0;
                if (int.TryParse(d, out opt_ind) == false)
                    Console.WriteLine("invalid input !");
                else
                {
                    double pt, lc, rapid_side;
                    List<double> nanpin_timing, nanpin_lot;
                    int strategy, ma_term;

                    var rs = new ReadSim();
                    var ind_list = rs.generateBestPlIndList(num_opt_calc);
                    using (var sr = new StreamReader("opt nanpin.csv"))
                    {
                        var data = sr.ReadLine();
                        for (int i = 0; i < ind_list[opt_ind]; i++)
                            sr.ReadLine();
                        data = sr.ReadLine();
                        var ele = data.Split(',');
                        pt = Convert.ToDouble(ele[10]);
                        lc = Convert.ToDouble(ele[11]);
                        ma_term = Convert.ToInt32(ele[14]);
                        strategy = Convert.ToInt32(ele[15]);
                        rapid_side = Convert.ToDouble(ele[16]);
                        nanpin_timing = ele[17].Split(':').Select(double.Parse).ToList();
                        nanpin_lot = ele[18].Split(':').Select(double.Parse).ToList();
                        Console.WriteLine("Opt pl=" + ele[3] + ", opt num trade=" + ele[1] + ", opt win rate=" + ele[2]);

                    }
                    var ac = new Account(leveraged_or_fixed_trading, false);
                    var sim = new Sim();
                    if (strategy == 0)
                        ac = sim.sim_madiv_nanpin_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, true);
                    else
                        ac = sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, ref rapid_side);
                    displaySimResult(ac, "Opt select sim");
                }
            }
            else if (key == "10") //opt filtering
            {
                Console.WriteLine("Please input opt strategy id for filter optimization.");
                var d = Console.ReadLine();
                var opt_ind = 0;
                if (int.TryParse(d, out opt_ind) == false)
                    Console.WriteLine("invalid input !");
                else
                {
                    var of = new OptFiltering();
                    of.startOptFiltering(opt_ind, num_opt_calc, leveraged_or_fixed_trading, from, to);
                }
            }
            else if (key == "11")
            {
                Console.WriteLine("Read Filtering Sim");
                var read_sim_from = MarketData.Close.Count - 1000000;
                var read_sim_to = MarketData.Close.Count - 1;
                var rsim = new ReadFilteringSim();
                rsim.startReadSim(read_sim_from, read_sim_to, leveraged_or_fixed_trading);

            }
            else if (key == "12")
            {
                Console.WriteLine("Please input opt strategy id for test.");
                var d = Console.ReadLine();
                var opt_ind = 0;
                if (int.TryParse(d, out opt_ind) == false)
                    Console.WriteLine("invalid input !");
                else
                {
                    double pt, lc, rapid_side, kijun_change;
                    List<double> nanpin_timing, nanpin_lot;
                    int strategy, ma_term, filter_id, kijun_time_window, kijun_time_suspension;

                    var rs = new ReadSim();
                    var ind_list = rs.generateBestPlIndList(num_opt_calc);
                    using (var sr = new StreamReader("opt param filter.csv"))
                    {
                        var data = sr.ReadLine();
                        for (int i = 0; i < ind_list[opt_ind]; i++)
                            sr.ReadLine();
                        data = sr.ReadLine();
                        var ele = data.Split(',');
                        pt = Convert.ToDouble(ele[10]);
                        lc = Convert.ToDouble(ele[11]);
                        ma_term = Convert.ToInt32(ele[14]);
                        strategy = Convert.ToInt32(ele[15]);
                        rapid_side = Convert.ToDouble(ele[16]);
                        nanpin_timing = ele[17].Split(':').Select(double.Parse).ToList();
                        nanpin_lot = ele[18].Split(':').Select(double.Parse).ToList();
                        filter_id = Convert.ToInt32(ele[19]);
                        kijun_time_window = Convert.ToInt32(ele[20]);
                        kijun_change = Convert.ToDouble(ele[21]);
                        kijun_time_suspension = Convert.ToInt32(ele[22]);
                        Console.WriteLine("Opt pl=" + ele[3] + ", opt num trade=" + ele[1] + ", opt win rate=" + ele[2]);

                    }
                    var ac = new Account(leveraged_or_fixed_trading, false);
                    var sim = new Sim();
                    if (strategy == 0)
                        ac = sim.sim_madiv_nanpin_filter_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, ref filter_id, ref kijun_time_window, ref kijun_change, ref kijun_time_suspension);
                    else
                        ac = sim.sim_madiv_nanpin_rapid_side_change_filter_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, ref rapid_side, ref filter_id, ref kijun_time_window, ref kijun_change, ref kijun_time_suspension);
                    displaySimResult(ac, "Opt filtering select sim");
                }
            }
            else if (key == "13")
            {
                Console.WriteLine("Start optimize strategy select rule");
                var optselect = new OptNanpinSelect();
                optselect.startOptNanpinSelect(from, to, selected_opt_ids, 100, leveraged_or_fixed_trading);
            }
            else if (key == "14")
            {
                Console.WriteLine("Please input opt select strategy id for test.");
                var d = Console.ReadLine();
                var opt_ind = 0;
                if (int.TryParse(d, out opt_ind) == false)
                    Console.WriteLine("invalid input !");
                else
                {
                    var select_sim = new ReadSelectSim();
                    var ac = select_sim.startReadSelectSim(from, to, leveraged_or_fixed_trading, selected_opt_ids, opt_ind);
                    displaySimResult(ac, "Opt select sim");
                }
            }
            else if (key == "15")
            {
                Console.WriteLine("Conti Opt Nanpin Sim");

                var train_term = 200000;
                var sim_term = 100000;
                var ac = new Account(leveraged_or_fixed_trading, false);
                var current_from = 1000000;
                var current_to = current_from + train_term;
                var num = 50;

                double pt, lc, rapid_side_change_ratio;
                int ma_term, strategy_id;
                List<double> nanpin_lot, nanpin_timing;
                var num_loop = 0;
                while(true)
                {
                    var o = new OptNanpin();
                    o.startOptMADivNanpin(current_from, current_to, leveraged_or_fixed_trading, num, false);
                    readOptData(0);
                    var sim_to = current_to + sim_term < MarketData.Close.Count - 1 ? current_to + sim_term : MarketData.Close.Count - 1;
                    ac = dosim(current_to, sim_to, ac);
                    Console.WriteLine("Loop No.=" + num_loop);
                    Console.WriteLine("sim from="+current_to +", sim to="+ sim_to);
                    Console.WriteLine("Current total pl="+ac.performance_data.total_pl+", num trade"+ac.performance_data.num_trade);
                    current_from = current_to +sim_term - train_term;
                    current_to = current_from + train_term;
                    if (current_from + train_term >= MarketData.Close.Count - 1)
                        break;
                    num_loop++;
                    if (ac.stop_sim)
                    {
                        ac = dosim(current_to, MarketData.Close.Count - 1, ac);
                        break;
                    }
                }

                void readOptData(int opt_ind)
                {
                    var rs = new ReadSim();
                    var ind_list = rs.generateBestPlIndList(num_opt_calc);
                    using (var sr = new StreamReader("opt nanpin.csv"))
                    {
                        var data = sr.ReadLine();
                        for (int i = 0; i < ind_list[opt_ind]; i++)
                            sr.ReadLine();
                        data = sr.ReadLine();
                        var ele = data.Split(',');
                        pt = Convert.ToDouble(ele[10]);
                        lc = Convert.ToDouble(ele[11]);
                        ma_term = Convert.ToInt32(ele[14]);
                        strategy_id = Convert.ToInt32(ele[15]);
                        rapid_side_change_ratio = Convert.ToDouble(ele[16]);
                        nanpin_timing = ele[17].Split(':').Select(double.Parse).ToList();
                        nanpin_lot = ele[18].Split(':').Select(double.Parse).ToList();
                    }
                }
                Account dosim(int from, int to, Account ac)
                {
                    var sim = new Sim();
                    if (strategy_id == 0)
                        return sim.conti_sim_madiv_nanpin_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, true);
                    else
                        return sim.conti_sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, ac, ref pt, ref lc, ref nanpin_timing, ref nanpin_lot, ref ma_term, ref rapid_side_change_ratio);
                }
                displaySimResult(ac, "Conti Opt Nnapin sim");
            }
        }
    }
}
