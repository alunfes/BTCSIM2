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
        public Dictionary<int, double> res_total_capital {get;set;}
        public Dictionary<int, double> res_total_pl_ratio { get; set; }
        public Dictionary<int, double> res_win_rate { get; set; }
        public Dictionary<int, int> res_num_trade { get; set; }
        public Dictionary<int, int> res_num_buy { get; set; }
        public Dictionary<int, int> res_num_sell { get; set; }
        public Dictionary<int, double> res_ave_buy_pl { get; set; }
        public Dictionary<int, double> res_ave_sell_pl { get; set; }
        public Dictionary<int, double> res_realized_pl_variance { get; set; }
        public Dictionary<int, double> res_total_capital_variance { get; set; }

        public Dictionary<int, double> para_pt { get; set; }
        public Dictionary<int, double> para_lc { get; set; }
        public Dictionary<int, int> para_num_split { get; set; }
        public Dictionary<int, int> para_func { get; set; }
        public Dictionary<int, int> para_ma_term { get; set; }
        public Dictionary<int, double[]> para_nanpin_timing { get; set; }
        public Dictionary<int, double[]> para_nanpin_lot { get; set; }


        private void initializer()
        {
            res_total_capital = new Dictionary<int, double>();
            res_total_pl_ratio = new Dictionary<int, double>();
            res_win_rate = new Dictionary<int, double>();
            res_num_trade = new Dictionary<int, int>();
            res_num_buy = new Dictionary<int, int>();
            res_num_sell = new Dictionary<int, int>();
            res_ave_buy_pl = new Dictionary<int, double>();
            res_ave_sell_pl = new Dictionary<int, double>();
            res_realized_pl_variance = new Dictionary<int, double>();
            res_total_capital_variance = new Dictionary<int, double>();
            para_pt = new Dictionary<int, double>();
            para_lc = new Dictionary<int, double>();
            para_num_split = new Dictionary<int, int>();
            para_func = new Dictionary<int, int>();
            para_ma_term = new Dictionary<int, int>();
            para_nanpin_timing = new Dictionary<int, double[]>();
            para_nanpin_lot = new Dictionary<int, double[]>();
        }


        public void startOptMADivNanpin(int from, int to, bool flg_paralell, string lev_fixed_trading)
        {
            initializer();
            var pt = new List<double>() { 0.002, 0.005, 0.007, 0.009, 0.011, 0.013, 0.015, 0.017, 0.019, 0.021, 0.023, 0.025, 0.027, 0.03, 0.035, 0.04, 0.045, 0.05, 0.055, 0.06, 0.065, 0.07, 0.08, 0.09 };
            var lc = new List<double>() { 0.002, 0.005, 0.007, 0.009, 0.011, 0.013, 0.015, 0.017, 0.019, 0.021, 0.023, 0.025, 0.027, 0.03, 0.035, 0.04, 0.045, 0.05, 0.055, 0.06, 0.065, 0.07, 0.08, 0.09, 0.12, 0.15, 0.17, 0.2 };
            var num_split = new List<int>() {2, 3, 5, 7, 9, 11, 13, 15};
            var func = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };//{ 0, 1, 2};
            var ma_term = MarketData.terms;
            var no = 0;

            
            //generate parameter combination data
            for (int i = 0; i < pt.Count; i++)
            {
                for (int j = 0; j < lc.Count; j++)
                {
                    for (int k = 0; k < num_split.Count; k++)
                    {
                        for (int l = 0; l < func.Count; l++)
                        {
                            for (int m = 0; m < ma_term.Count; m++)
                            {
                                //var d = getNanpinParam2(pt[i], lc[j], num_split[k], func[l]);
                                var d = getNanpinParam(pt[i], lc[j], num_split[k], func[l]);
                                para_pt.Add(no, pt[i]);
                                para_lc.Add(no, lc[j]);
                                para_num_split.Add(no, num_split[k]);
                                para_func.Add(no, func[l]);
                                para_ma_term.Add(no, ma_term[m]);
                                para_nanpin_timing.Add(no, d.Values.ToList()[0].ToList()[0]);
                                para_nanpin_lot.Add(no, d.Values.ToList()[0].ToList()[1]);
                                no++;
                            }
                        }
                    }
                }
            }
            //do optimization search
            using (StreamWriter writer = new StreamWriter("opt nanpin.csv", false))
            using (var sw = TextWriter.Synchronized(writer))
            {
                var progress = 0.0;
                var n = 0.0;
                sw.WriteLine("No.,num trade,win rate,total pl,realized pl,realzied pl var,total capital var,sharp ratio,total capital gradient,pt,lc,num_split,func,ma_term,nanpin timing,lot splits");
                if (flg_paralell)
                {
                    Parallel.For(0, no, i =>
                    {
                        var sim = new Sim();
                        var ac = new Account(lev_fixed_trading,true);
                        ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, para_pt[i], para_lc[i],
                            para_nanpin_timing[i].ToList(), para_nanpin_lot[i].ToList(), para_ma_term[i], true);
                        res_total_capital.Add(i, ac.performance_data.total_capital);
                        res_total_pl_ratio.Add(i, ac.performance_data.total_pl_ratio);
                        res_win_rate.Add(i, ac.performance_data.win_rate);
                        res_num_trade.Add(i, ac.performance_data.num_trade);
                        res_num_buy.Add(i, ac.performance_data.num_buy);
                        res_num_sell.Add(i, ac.performance_data.num_sell);
                        res_ave_buy_pl.Add(i, ac.performance_data.buy_pl_ratio_list.Average());
                        res_ave_sell_pl.Add(i, ac.performance_data.sell_pl_ratio_list.Average());
                        res_realized_pl_variance.Add(i, ac.performance_data.realized_pl_ratio_variance);
                        res_total_capital_variance.Add(i, ac.performance_data.total_capital_variance);
                        var res = n.ToString() + "," +ac.performance_data.num_trade.ToString()+","+ac.performance_data.win_rate.ToString()+","+ac.performance_data.total_pl.ToString() +","+
                        ac.performance_data.realized_pl.ToString()+","+ac.performance_data.realized_pl_ratio_variance.ToString()+","+ac.performance_data.total_capital_variance.ToString()+","+ac.performance_data.sharp_ratio.ToString()+","+
                        ac.performance_data.total_capital_gradient.ToString()+","+para_pt[i].ToString()+","+para_lc[i].ToString()+","+para_num_split[i].ToString()+","+
                        para_func[i].ToString()+","+para_ma_term[i].ToString()+","+ string.Join(":", para_nanpin_timing[i]) + "," + string.Join(":", para_nanpin_lot[i]);
                        sw.WriteLine(res);
                        n++;
                        progress = Math.Round(100.0 * n / Convert.ToDouble(no), 2);
                        Console.WriteLine(i.ToString() +"/"+no.ToString() + " - " + progress.ToString() + "%"+ ": pl ratio="+ac.performance_data.total_pl_ratio.ToString() + ", sharp ratio=" + ac.performance_data.sharp_ratio.ToString() + ", win rate=" + ac.performance_data.win_rate.ToString());
                    });
                }
                else
                {
                    //randomly select param combination from list
                    var r = new Random();
                    var lock_r = new Object();
                    var ind_list = new List<int>();
                    for (int i = 0; i < no; i++)
                        ind_list.Add(i);
                    int getNextInd()
                    {
                        lock(lock_r)
                        {
                            var d = r.Next(0, ind_list.Count);
                            var res = ind_list[d];
                            ind_list.RemoveAt(d);
                            return res;
                        }
                    }
                    for (int i = 0; i < no; i++)
                    {
                        var sim = new Sim();
                        var ac = new Account(lev_fixed_trading, true);
                        var ind = getNextInd();
                        ac = sim.sim_madiv_nanpin_ptlc(from, to, ac, para_pt[ind], para_lc[ind],
                            para_nanpin_timing[ind].ToList(), para_nanpin_lot[ind].ToList(), para_ma_term[ind], true);
                        res_total_capital.Add(ind, ac.performance_data.total_capital);
                        res_total_pl_ratio.Add(ind, ac.performance_data.total_pl_ratio);
                        res_win_rate.Add(ind, ac.performance_data.win_rate);
                        res_num_trade.Add(ind, ac.performance_data.num_trade);
                        res_num_buy.Add(ind, ac.performance_data.num_buy);
                        res_num_sell.Add(ind, ac.performance_data.num_sell);
                        res_realized_pl_variance.Add(ind, ac.performance_data.realized_pl_ratio_variance);
                        res_total_capital_variance.Add(ind, ac.performance_data.total_capital_variance);
                        if (ac.performance_data.buy_pl_ratio_list.Count > 0)
                            res_ave_buy_pl.Add(ind, ac.performance_data.buy_pl_ratio_list.Average());
                        else
                            res_ave_buy_pl.Add(ind, 0);
                        if (ac.performance_data.sell_pl_ratio_list.Count > 0)
                            res_ave_sell_pl.Add(ind, ac.performance_data.sell_pl_ratio_list.Average());
                        else
                            res_ave_sell_pl.Add(ind, 0);
                        var res = i.ToString() + "," + ac.performance_data.num_trade.ToString() + "," + ac.performance_data.win_rate.ToString() + "," + ac.performance_data.total_pl.ToString() +","+ac.performance_data.realized_pl.ToString() + "," +
                            ac.performance_data.realized_pl_ratio_variance.ToString()+","+ac.performance_data.total_capital_variance.ToString()+","+
                            ac.performance_data.sharp_ratio.ToString() + "," + ac.performance_data.total_capital_gradient.ToString() + "," +para_pt[ind].ToString() + "," +
                            para_lc[ind].ToString() + "," + para_num_split[ind].ToString() + "," + para_func[ind].ToString() + "," + para_ma_term[ind].ToString() + "," +
                            string.Join(":", para_nanpin_timing[ind]) + "," + string.Join(":", para_nanpin_lot[ind]);
                        sw.WriteLine(res);
                        progress = Math.Round(100.0 * Convert.ToDouble(i) / Convert.ToDouble(no), 2);
                        Console.WriteLine(i.ToString() + "/" + no.ToString() + " - " + progress.ToString() + "%" + ": pl ratio=" + ac.performance_data.total_pl_ratio.ToString() +", sharp ratio="+ac.performance_data.sharp_ratio.ToString()+", win rate="+ac.performance_data.win_rate.ToString());
                    }
                }
            }
        }



        /*
         * 関数は同値分割（total_size / num）、x%ずつlotを上げる、x%ずつlotを下げるから選択する。
         * 
         */
        public Dictionary<string, List<double[]>> getNanpinParam2(double pt, double lc, int num_splits, int select_func_no)
        {
            var nanpin_lots = new Dictionary<string, List<double[]>>(); //napin lot name, nanpin timing, lot splilit
            if (num_splits > 1)
            {
                var min_lot = 0.001;
                var total_size = 0.05;
                //nanpin timing
                var timing = new List<double>();
                var unit = (lc - 0.0001) / Convert.ToDouble(num_splits);
                for (int i = 0; i < num_splits - 1; i++)
                    timing.Add(Math.Round(unit * (i + 1), 4));
                var alloc = new List<double>();
                if (select_func_no == 0)
                {
                    var sampling_unit = total_size / Convert.ToDouble(num_splits);
                    for (int i = 0; i < num_splits; i++)
                        alloc.Add(Math.Round(sampling_unit, 6));
                }
                else if (select_func_no == 1)//increase with x% from min_lot, sum lot should be equal to max_size
                {
                    var r = calc_r(num_splits);
                    alloc.Add(min_lot);
                    for (int i = 1; i < num_splits; i++)
                        alloc.Add(min_lot * Math.Pow(r, i));
                }
                else if (select_func_no == 2)//decrease with x% from min_lot, sum lot should be equal to max_size
                {
                    var r = calc_r(num_splits);
                    var realloc = new List<double>();
                    realloc.Add(min_lot);
                    for (int i = 1; i < num_splits; i++)
                        realloc.Add(min_lot * Math.Pow(r, i));
                    for (int i = 0; i < realloc.Count; i++)
                        alloc.Add(realloc[realloc.Count - i - 1]);
                }
                else
                    Console.WriteLine("OptNanpin: Invalid select_func_no !");
                nanpin_lots.Add(num_splits.ToString() + "-" + select_func_no.ToString(), new List<double[]> { timing.ToArray(), alloc.ToArray() });
            }
            else
            {
                nanpin_lots.Add(num_splits.ToString() + "-" + select_func_no.ToString(), new List<double[]> { new double[] { }, new double[] { 1.0 } });
            }
            double calc_wa(double r, int n)
            {
                double wa = 0.0;
                for (int i = 0; i < n-1; i++)
                    wa += Math.Pow(r, i + 1);
                return wa;
            }
            double calc_r(int n)
            {
                var correctwa = 49.0;//minlot= 0.001, maxlot=0.05の場合に等比数列では常に成立する。
                var r = 0.1;
                while(true)
                {
                    if (calc_wa(r, n) >= correctwa)
                    {
                        return Math.Round(r, 5);
                    }
                    r += 0.001;
                }
            }
            return nanpin_lots;
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