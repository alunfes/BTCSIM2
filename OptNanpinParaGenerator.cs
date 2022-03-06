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
    public class OptNanpinParaGenerator
    {
        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_num_split { get; set; }
        public ConcurrentDictionary<int, int> para_func { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }


        private void initializer()
        {
            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_num_split = new ConcurrentDictionary<int, int>();
            para_func = new ConcurrentDictionary<int, int>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();
        }

        public void generateParametersAllPriceNanpin(double min_pt, double max_pt, double min_lc, double max_lc, int num_select_params)
        {
            initializer();
            var pt = generatePtList(min_pt, max_pt);
            var lc = generateLcList(min_lc, max_lc);
            var ma_term = generateMATerm(MarketData.terms);
            var strategy = generateStrategy();
            var rapid_side = generateRapidSideChange();

            var ran = new Random();
            for (int i=0; i<num_select_params; i++)
            {
                para_pt.TryAdd(i, pt[ran.Next(0, pt.Count)]);                
                para_func.TryAdd(i, -1);
                para_num_split.TryAdd(i, 49);
                para_ma_term.TryAdd(i, ma_term[ran.Next(0, ma_term.Count)]);
                para_strategy.TryAdd(i, strategy[ran.Next(0, strategy.Count)]);
                para_rapid_side_change_ratio.TryAdd(i, rapid_side[ran.Next(0, rapid_side.Count)]);
                var lc_d = lc[ran.Next(0, lc.Count)];
                para_lc.TryAdd(i, lc_d);
                var lot_timing = getAllPriceNanpinParam(lc_d);
                para_nanpin_timing.TryAdd(i, lot_timing[0][0].ToList());
                para_nanpin_lot.TryAdd(i, lot_timing[0][1].ToList());
            }
        }



            public void generateParameters(double min_pt, double max_pt, double min_lc, double max_lc, int max_split, int num_select_params)
        {
            initializer();
            var func = new List<int>() { 0, 1, 2 };
            var pt = generatePtList(min_pt, max_pt);
            var lc = generateLcList(min_lc, max_lc);
            var num_split = generateNumSplit(max_split);
            var ma_term = generateMATerm(MarketData.terms);
            var min_lot = generateMinLot();
            var strategy = generateStrategy();
            var rapid_side = generateRapidSideChange();
            var para_ind_combination = generateParamIndCombination(num_select_params, pt, lc, num_split, func, ma_term, min_lot, strategy, rapid_side); //generate combination of all parameters
            //randomly select the parameter combinations and get nanpin timing / lot
            var random_params_ind = new ConcurrentDictionary<int, int>();
            var ran = new Random();
            var ind_list = new List<int>(para_ind_combination.Keys.ToList());
            for(int i=0; i<num_select_params; i++) //randomly select the parameter combination
            {
                var s = ran.Next(0,ind_list.Count);
                random_params_ind[i] = ind_list[s];
                ind_list.RemoveAt(s);
            }
            var nanpin_dict = new ConcurrentDictionary<int, ConcurrentDictionary<int, List<double[]>>>();
            Parallel.For(0, num_select_params, i => //generate nanpin timinig / lot using geometric series function
            {
                nanpin_dict.TryAdd(i,
                    getNanpinParam2(
                        lc[para_ind_combination[random_params_ind[i]][1]],
                        num_split[para_ind_combination[random_params_ind[i]][2]],
                        func[para_ind_combination[random_params_ind[i]][3]],
                        min_lot[para_ind_combination[random_params_ind[i]][5]]
                        ));
                para_pt.TryAdd(i, pt[para_ind_combination[random_params_ind[i]][0]]);
                para_lc.TryAdd(i, lc[para_ind_combination[random_params_ind[i]][1]]);
                para_num_split.TryAdd(i, num_split[para_ind_combination[random_params_ind[i]][2]]);
                para_func.TryAdd(i, func[para_ind_combination[random_params_ind[i]][3]]);
                para_ma_term.TryAdd(i, ma_term[para_ind_combination[random_params_ind[i]][4]]);
                para_nanpin_timing.TryAdd(i, nanpin_dict[i].Values.ToList()[0].ToList()[0].ToList());
                para_nanpin_lot.TryAdd(i, nanpin_dict[i].Values.ToList()[0].ToList()[1].ToList());
                para_strategy.TryAdd(i, strategy[para_ind_combination[random_params_ind[i]][6]]);
                para_rapid_side_change_ratio.TryAdd(i, rapid_side[para_ind_combination[random_params_ind[i]][7]]);
            });
            Console.WriteLine("Completed OptNanpinParaGenerator");
        }

        public ConcurrentDictionary<int,double> generatePtList(double min_pt, double max_pt)
        {
            var res = new ConcurrentDictionary<int, double>();
            var pt = new List<double>();
            var n = 0;
            while (true)
            {
                pt.Add(Math.Round(min_pt + (n * 0.001),4));
                n++;
                if (pt.Last() > max_pt)
                {
                    pt.RemoveAt(pt.Count - 1);
                    break;
                }
            }
            for (int i = 0; i < pt.Count; i++)
                res.TryAdd(i,pt[i]);
            return res;
        }
        public ConcurrentDictionary<int, double> generateLcList(double min_lc, double max_lc)
        {
            var lc = new List<double>();
            var res = new ConcurrentDictionary<int, double>();
            var n = 0;
            while (true)
            {
                lc.Add(Math.Round(min_lc + (n * 0.001),4));
                n++;
                if (lc.Last() > max_lc)
                {
                    lc.RemoveAt(lc.Count - 1);
                    break;
                }
            }
            for (int i = 0; i < lc.Count; i++)
                res.TryAdd(i,lc[i]);
            return res;
        }
        public ConcurrentDictionary<int, int> generateNumSplit(int max_split)
        {
            var res = new ConcurrentDictionary<int, int>();
            for(int i=1; i<max_split; i++)
            {
                res.TryAdd(i-1,i+1);
            }
            return res;
        }
        public ConcurrentDictionary<int, double> generateMinLot() //minlot=0.001, maxlot=0.2, total lot is always 1.0
        {
            var res = new ConcurrentDictionary<int, double>();
            res.TryAdd(0,0.001);
            for (int i = 0; i < 40; i++)
                res.TryAdd(i+1, Math.Round((i + 1) * 0.005, 4));
            return res;
        }
        public ConcurrentDictionary<int, int> generateMATerm(List<int> ma_term)
        {
            var res = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < ma_term.Count; i++)
                res.TryAdd(i, ma_term[i]);
            return res;
        }
        public ConcurrentDictionary<int, int> generateStrategy()
        {
            var res = new ConcurrentDictionary<int, int>();
            res.TryAdd(0, 0);
            res.TryAdd(1, 1);
            return res;
        }
        public ConcurrentDictionary<int, double> generateRapidSideChange()
        {
            var res = new ConcurrentDictionary<int, double>();
            for (int i = 0; i < 95; i++)
                res.TryAdd(i, (i + 1) * 0.01);
            return res;
        }


        //各パラメータのランダムな組み合わせを生成する。
        private ConcurrentDictionary<int, List<int>> generateParamIndCombination(int num_select, ConcurrentDictionary<int, double> pt, ConcurrentDictionary<int, double> lc,
            ConcurrentDictionary<int, int> num_split, List<int> funcs, ConcurrentDictionary<int, int> ma_term, ConcurrentDictionary<int, double> min_lot, ConcurrentDictionary<int, int> strategy, ConcurrentDictionary<int, double> rapid_side)
        {
            var res = new ConcurrentDictionary<string, List<int>>();
            var ran = new Random();
            for(int i=0; i<num_select; i++)
            {
                var ind_combi = new List<int> { ran.Next(0, pt.Count), ran.Next(0, lc.Count), ran.Next(0, num_split.Count), ran.Next(0, funcs.Count), ran.Next(0, ma_term.Count), ran.Next(0, min_lot.Count), ran.Next(0,strategy.Count), ran.Next(0, rapid_side.Count) };
                res.TryAdd(string.Join("", ind_combi), ind_combi);
            }
            var true_res = new ConcurrentDictionary<int, List<int>>();
            var reskeys = new List<string>(res.Keys);
            for (int i = 0; i < reskeys.Count; i++)
                true_res.TryAdd(i, res[reskeys[i]]);
            return true_res;
        }


        //place 50 nanapin orders as fixed till lc price
        public ConcurrentDictionary<int, List<double[]>> getAllPriceNanpinParam(double lc)
        {
            var nanpin_lots = new ConcurrentDictionary<int, List<double[]>>(); //napin lot name, nanpin timing, lot splilit
            var minlot = 0.001;
            var skip = (lc - minlot) / 50.0;
            var lot = Math.Round(1.0 / 50.0, 4);
            var timing = new List<double>();
            var lots = new List<double>();
            for (int i=0; i<50; i++)
                lots.Add(lot);
            for (int i = 0; i < 49; i++)
                timing.Add((i+1) * skip);
            nanpin_lots.TryAdd(0, new List<double[]> { timing.ToArray(), lots.ToArray() });
            return nanpin_lots;
        }


        /*
         * 関数は同値分割（total_size / num）、x%ずつlotを上げる、x%ずつlotを下げるから選択する。
         * 
         */
        public ConcurrentDictionary<int, List<double[]>> getNanpinParam2(double lc, int num_splits, int select_func_no, double minl)
        {
            var nanpin_lots = new ConcurrentDictionary<int, List<double[]>>(); //napin lot name, nanpin timing, lot splilit
            if (num_splits > 1)
            {
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
                    alloc = get_geometric_series(num_splits, minl);
                }
                else if (select_func_no == 2)//decrease with x% from min_lot, sum lot should be equal to max_size
                {
                    var realloc = get_geometric_series(num_splits, minl);
                    for (int i = 0; i < realloc.Count; i++)
                        alloc.Add(realloc[realloc.Count - i - 1]);
                }
                else
                    Console.WriteLine("OptNanpin: Invalid select_func_no !");
                nanpin_lots.TryAdd(0, new List<double[]> { timing.ToArray(), alloc.ToArray() });
            }
            else
            {
                nanpin_lots.TryAdd(0, new List<double[]> { new double[] { }, new double[] { 1.0 } });
            }
            List<double> get_geometric_series(int n, double minl)
            {
                var data = new List<double>();
                var r = calc_r(n, minl);
                for (int i = 0; i < n; i++)
                    data.Add(Math.Round(minl * Math.Pow(r, i), 6));
                return data;
            }
            double calc_wa(double r, int n)
            {
                double wa = 0.0;
                for (int i = 0; i < n - 1; i++)
                    wa += Math.Pow(r, i + 1);
                return wa;
            }
            double calc_r(int n, double minl)
            {
                var correctwa = calc_correctwa(minl);//minlotとtotal lotが決まれば一意に決まる。この場合はtotal size=1.0
                var r = 0.1;
                while (true)
                {
                    if (calc_wa(r, n) >= correctwa)
                    {
                        return Math.Round(r, 5);
                    }
                    r += 0.001;
                }
            }
            double calc_correctwa(double minl) // minl = 0.001 - 0.2 //max num split=15
            {
                return (1.0 - minl) / minl;
            }

            return nanpin_lots;
        }
    }
}
