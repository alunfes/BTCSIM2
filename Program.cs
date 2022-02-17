using System;
using System.Collections.Generic;
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
            Console.WriteLine("sharp_ratio=" + ac.performance_data.sharp_ratio);
            Console.WriteLine("num_buy=" + ac.performance_data.num_buy);
            Console.WriteLine("num_sell=" + ac.performance_data.num_sell);
            Console.WriteLine("buy_pl=" + ac.performance_data.buy_pl_list.Sum());
            Console.WriteLine("sell_pl=" + ac.performance_data.sell_pl_list.Sum());
            Console.WriteLine("max_dd=" + ac.performance_data.max_dd);
            Console.WriteLine("max_pl=" + ac.performance_data.max_pl);
            Console.WriteLine("ave_holding_period=" + ac.holding_data.holding_period_list.Average());
            Console.WriteLine("total_capital_gradient=" + ac.performance_data.total_capital_gradient);
            var table_labels = new List<string>() { "PL Ratio", "Num Trade", "Win Rate", "Sharp Ratio", "Max DD", "Max PL", "Ave Buy PL", "Ave Sell PL", "Ave Holding Period", "Num Force Exit", "Total Capital Gradient" };
            var table_data = new List<string>() {Math.Round(ac.performance_data.total_pl_ratio,4).ToString(), ac.performance_data.num_trade.ToString(), Math.Round(ac.performance_data.win_rate,4).ToString(), ac.performance_data.sharp_ratio.ToString(), Math.Round(ac.performance_data.max_dd,4).ToString(),
            Math.Round(ac.performance_data.max_pl,4).ToString(), Math.Round(ac.performance_data.buy_pl_list.Sum() / Convert.ToDouble(ac.performance_data.buy_pl_list.Count), 4).ToString(),
                Math.Round(ac.performance_data.sell_pl_list.Sum() / Convert.ToDouble(ac.performance_data.sell_pl_list.Count), 4).ToString(), Math.Round(ac.holding_data.holding_period_list.Average(),1).ToString(),
                ac.performance_data.num_force_exit.ToString(), ac.performance_data.total_capital_gradient.ToString()};
            LineChart.DisplayLineChart3(ac.performance_data.total_capital_list, ac.performance_data.log_close, ac.performance_data.num_trade_list, table_labels, table_data, title + ": from=" + ac.start_ind.ToString() + ", to=" + ac.end_ind.ToString());
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }

        static void Main(string[] args)
        {
            var key = "";
            while (true)
            {
                Console.WriteLine("\"test\" : test");
                Console.WriteLine("\"ptlc\" : PTLC");
                Console.WriteLine("\"nanpin\" : PTLC Nanpin");
                Console.WriteLine("\"opt nanpin\" : Optimize nanpin parameters");
                Console.WriteLine("\"rand\" : Randome generated param nanpin sim");
                Console.WriteLine("\"multi nanpin\" : Multi param nanpin sim");
                Console.WriteLine("\"madiv nanpin\" : MA div nanpin sim");
                Console.WriteLine("\"read sim\" : Read MA div nanpin sim");
                Console.WriteLine("\"read multi\" : Read multi MA div nanpin sim");
                Console.WriteLine("\"opt select\" : Select Opt Nanpin id in best opt pl oreder");
                key = Console.ReadLine();
                if (key == "nanpin" || key == "ptlc" || key == "test" || key == "opt nanpin" || key == "rand" || key =="multi nanpin" || key == "madiv nanpin" ||
                    key == "read sim" || key == "read multi" || key == "opt select")
                    break;
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("started program.");
            RandomSeed.initialize();
            List<int> terms = new List<int>() { 2, 3, 4, 5, 7, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 180, 190, 200, 220, 240, 260, 280};
            //List<int> terms = new List<int>() { 2, 3, 4, 5, 7, 10, 14};
            MarketData.initializer(terms);

            var from = 1000;
            //var to = 500000;
            var to = MarketData.Close.Count - 1;
            //var leveraged_or_fixed_trading = "leveraged";
            var leveraged_or_fixed_trading = "fixed";
            var num_opt_calc = 1000;


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
                    for (int i = 0; i < num_splits-1; i++)
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
                            func_val.Add(Math.Pow(i * sampling_unit, 2.0)+minl);
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
                            func_val.Add(-0.1 * Math.Pow((i * sampling_unit)-10,2.0)+10+minl);
                        for (int i = 0; i < func_val.Count(); i++)
                            alloc.Add(Math.Round(func_val[i] / func_val.Sum(), 6));
                    }
                    else if (select_func_no == 5) //y=-0.1x^2 + 10 (x= -10 -> 10)
                    {
                        var sampling_unit = Convert.ToDouble(20) / Convert.ToDouble(num_splits - 1);
                        for (int i = 0; i < num_splits; i++)
                            func_val.Add(0.1 * Math.Pow((i * sampling_unit)-10, 2.0)+10+minl);
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
                    nanpin_lots.Add(num_splits.ToString() + "-" + select_func_no.ToString(), new List<double[]> { new double[] {}, new double[] { 1.0 } });
                return nanpin_lots;
            }

            if (key == "test")
            {
                Console.WriteLine("test");

                using (StreamWriter sw = new StreamWriter(@"./Data/test.csv"))
                    sw.WriteLine(string.Join(",", terms));
                Console.WriteLine("");
            }
            if (key == "ptlc")
            {
                Console.WriteLine("PTLC");
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                var pt_ratio = 0.01;
                var lc_ratio = 0.05;
                ac = sim.sim_ptlc(from, to, ac, pt_ratio, lc_ratio);
                displaySimResult(ac, "ptlc");
            }
            else if (key == "nanpin")
            {
                Console.WriteLine("Nanpin PT/LC random buy sell entry");
                var nanpin_timing = new List<double>() { 0.01, 0.02, 0.03 }; 
                var lot_splits = new List<double>() { 0.1}; 
                var pt_ratio = 0.013;
                var lc_ratio = 0.045;
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                ac = sim.sim_nanpin_ptlc(from, to, ac, pt_ratio, lc_ratio, nanpin_timing, lot_splits);
                displaySimResult(ac, "nanpin");
            }
            else if (key == "opt nanpin")
            {
                Console.WriteLine("optimize nanpin strategy parameter");
                var o = new OptNanpin();
                o.startOptMADivNanpin(from, to, true, leveraged_or_fixed_trading, num_opt_calc);
            }
            else if (key == "rand")
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
            else if(key == "multi nanpin")
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
                for (int i=0; i<pt.Count; i++)
                {
                    var ac = new Account(leveraged_or_fixed_trading, true);
                    var ac_res = sim.sim_nanpin_ptlc(from, to, ac, pt[i], lc[i], nanpin_timing[i].ToList(), lot_splits[i].ToList());
                    ac_list.Add(ac_res);
                }
                //consolidate multi nanpin results
                var consolidated_total_capital_list = new List<double>();
                var consolidated_num_trade = new List<int>();
                for(int i=0; i<ac_list[0].performance_data.total_capital_list.Count; i++)
                {
                    var sum = 0.0;
                    var n = 0.0;
                    for(int j=0; j<ac_list.Count; j++)
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
                for (int i=0; i<ac_list.Count; i++)
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
                Console.WriteLine("pl=" + Math.Round(ave_pl.Average(),4));
                Console.WriteLine("pl ratio=" + Math.Round(ave_pl_ratio.Average(), 4));
                Console.WriteLine("num trade=" + Math.Round(ave_num_trade.Average(), 4));
                Console.WriteLine("win rate=" + Math.Round(ave_win_rate.Average(), 4));
                Console.WriteLine("num_buy=" + Math.Round(ave_num_buy.Average(), 4));
                Console.WriteLine("num_sell=" + Math.Round(ave_num_sell.Average(), 4));
                Console.WriteLine("ave_holding_period=" + Math.Round(ave_holding_period.Average(), 4));
                var table_labels = new List<string>() {"Ave PL", "Ave PL Ratio", "Ave Num Trade", "Ave Win Rate", "Ave Holding Period"};
                var table_data = new List<string>() { Math.Round(ave_pl.Average(), 4).ToString(), Math.Round(ave_pl_ratio.Average(), 4).ToString(), Math.Round(ave_num_trade.Average(), 4).ToString(),
                Math.Round(ave_win_rate.Average(), 4).ToString(), Math.Round(ave_holding_period.Average(), 4).ToString()};
                LineChart.DisplayLineChart3(consolidated_total_capital_list, MarketData.Close.GetRange(ac_list[0].start_ind, ac_list[0].end_ind).ToList(), consolidated_num_trade, table_labels, table_data, "from=" + ac_list[0].start_ind.ToString() + ", to=" + ac_list[0].end_ind.ToString());
                System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
            }
            else if (key == "madiv nanpin")
            {
                Console.WriteLine("MA div Nanpin PT/LC");
                var nanpin_timing = new List<double>() { 0.0068,0.0136,0.0205,0.0273,0.0341,0.041,0.0478,0.0546,0.0614,0.0682,0.0751 };
                var lot_splits = new List<double>() { 0.318844,0.218536,0.149785,0.102663,0.070365,0.048228,0.033056,0.022656,0.015529,0.010643,0.007295,0.005 };
                var pt_ratio = 0.036;
                var lc_ratio = 0.082;
                var ma_term = 150;    
                var contrarian = true;
                var ac = new Account(leveraged_or_fixed_trading, false);
                var sim = new Sim();
                //ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term, contrarian);
                ac = sim.sim_madiv_nanpin_rapid_side_change_ptlc(from, to, ac, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term);
                displaySimResult(ac, "MA Div nanpin");
            }
            else if( key == "read sim")
            {
                Console.WriteLine("Read MA div nanpin Sim");
                var read_sim_from = MarketData.Close.Count - 300000;
                var read_sim_to = MarketData.Close.Count-1;
                var num_best_pl_for_test = 10;
                var rsim = new ReadSim();
                rsim.startReadSim(read_sim_from, read_sim_to, to - from, leveraged_or_fixed_trading, num_best_pl_for_test);

            }
            else if(key == "read multi")
            {
                Console.WriteLine("Read multi MA div nanpin Sim");
                var rs = new ReadSim();
                rs.startMultiReadSim(from, to, 5, leveraged_or_fixed_trading);
            }
            else if (key == "opt select")
            {
                Console.WriteLine("Please input opt strategy id for test.");
                var d = Console.ReadLine();
                var opt_ind = 0;
                if (int.TryParse(d, out opt_ind) == false)
                    Console.WriteLine("invalid input !");
                else
                {
                    double pt, lc;
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
                        pt = Convert.ToDouble(ele[9]);
                        lc = Convert.ToDouble(ele[10]);
                        ma_term = Convert.ToInt32(ele[13]);
                        nanpin_timing = ele[15].Split(':').Select(double.Parse).ToList();
                        nanpin_lot = ele[16].Split(':').Select(double.Parse).ToList();
                        strategy = Convert.ToInt32(ele[14]);

                    }
                    var ac = new Account(leveraged_or_fixed_trading, false);
                    var sim = new Sim();
                    if (strategy ==0)
                        ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, pt, lc, nanpin_timing, nanpin_lot, ma_term, true);
                    else
                        ac = sim.sim_madiv_nanpin_rapid_side_change_ptlc(from, to, ac, pt, lc, nanpin_timing, nanpin_lot, ma_term);
                    displaySimResult(ac, "Opt select sim");
                }
            }
        }
    }
}
