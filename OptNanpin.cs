using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace BTCSIM2
{
    public class OptNanpin
    {
        public ConcurrentDictionary<int, double> res_total_capital {get;set;}
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
        public ConcurrentDictionary<int, int> para_num_split { get; set; }
        public ConcurrentDictionary<int, int> para_func { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, double[]> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, double[]> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, double[]> para_min_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }


        private void initializer()
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
            para_num_split = new ConcurrentDictionary<int, int>();
            para_func = new ConcurrentDictionary<int, int>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_nanpin_timing = new ConcurrentDictionary<int, double[]>();
            para_nanpin_lot = new ConcurrentDictionary<int, double[]>();
            para_min_lot = new ConcurrentDictionary<int, double[]>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();
        }





        public void startOptMADivNanpin(int from, int to, string lev_fixed_trading, int num_opt_calc)
        {
            initializer();
            var opt_para_gen = new OptNanpinParaGenerator();
            opt_para_gen.generateParameters(0.005, 0.1, 0.005, 0.1, 15, num_opt_calc);
            //do optimization search
            using (StreamWriter writer = new StreamWriter("opt nanpin.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                var progress = 0.0;
                var n = 0.0;
                sw.WriteLine("No.,num trade,win rate,total pl,realized pl,realzied pl sd,total capital sd,sharp ratio,total capital gradient,window pl ratio,pt,lc,num_split,func,ma_term,strategy id,rapid side change ratio,nanpin timing,lot splits");
                var ac_list = new ConcurrentDictionary<int, Account>();
                Parallel.For(0, num_opt_calc, i =>
                {
                    ac_list.TryAdd(i, doSim(ref lev_fixed_trading,
                        opt_para_gen.para_strategy[i],
                        ref from,
                        ref to,
                        opt_para_gen.para_pt[i],
                        opt_para_gen.para_lc[i],
                        opt_para_gen.para_nanpin_timing[i],
                        opt_para_gen.para_nanpin_lot[i],
                        opt_para_gen.para_ma_term[i],
                        opt_para_gen.para_rapid_side_change_ratio[i]));
                    res_total_capital.TryAdd(i,ac_list[i].performance_data.total_capital);
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
                    res_realized_pl_sd.TryAdd(i,ac_list[i].performance_data.realized_pl_ratio_sd);
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
                    opt_para_gen.para_pt[i].ToString() + "," +
                    opt_para_gen.para_lc[i].ToString() + "," +
                    opt_para_gen.para_num_split[i].ToString() + "," +
                    opt_para_gen.para_func[i].ToString() + "," +
                    opt_para_gen.para_ma_term[i].ToString() + "," +
                    opt_para_gen.para_strategy[i].ToString() +","+
                    opt_para_gen.para_rapid_side_change_ratio[i].ToString()+","+
                    string.Join(":",opt_para_gen.para_nanpin_timing[i]) + "," +
                    string.Join(":",opt_para_gen.para_nanpin_lot[i]));
                    sw.WriteLine(res_write_contents[i]);
                    n++;
                    progress = Math.Round(100.0 * n / Convert.ToDouble(num_opt_calc), 2);
                    Console.WriteLine(n.ToString() +"/"+num_opt_calc.ToString() + " - " + progress.ToString() + "%"+
                        ": pl ratio="+ ac_list[i].performance_data.total_pl_ratio.ToString() +
                        ", sharp ratio=" + ac_list[i].performance_data.sharp_ratio.ToString() +
                        ", win rate=" + ac_list[i].performance_data.win_rate.ToString());

                    while (ac_list.TryRemove(i, out var d) == false) ;
                    while (res_write_contents.TryRemove(i, out var dd)) ;
                    //ac_list.TryRemove(i, out var d);
                    //res_write_contents.TryRemove(i, out var dd);
                });
            }
        }


        private Account doSim(ref string lev_or_fixed, int strategy, ref int from, ref int to, double pt, double lc, List<double> nanpint_timing, List<double> nanpin_lot, int ma_term, double rapid_side_change_ratio)
        {
            var sim = new Sim();
            if (strategy == 0)
                return sim.sim_madiv_nanpin_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, true);
            else
                return sim.sim_madiv_nanpin_rapid_side_change_ptlc(ref from, ref to, new Account(lev_or_fixed, true), ref pt, ref lc, ref nanpint_timing, ref nanpin_lot, ref ma_term, ref rapid_side_change_ratio);
        }

        Dictionary<string, List<double[]>> getNanpinParam(double pt, double lc, int num_splits, int select_func_no)
        {
            var nanpin_lots = new Dictionary<string, List<double[]>>(); //napin lot name, nanpin timing, lot splilit
            if (num_splits > 1)
            {
                //nanpin timing
                var timing = new List<double>();
                var unit = (lc - 0.001) / Convert.ToDouble(num_splits);
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
    }
}