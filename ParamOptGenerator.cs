using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BTCSIM2
{
    public class ParamOptGenerator
    {
        public ConcurrentDictionary<int, double> para_pt { get; set; }
        public ConcurrentDictionary<int, double> para_lc { get; set; }
        public ConcurrentDictionary<int, int> para_num_split { get; set; }
        public List<int> para_func { get; set; }
        public ConcurrentDictionary<int, int> para_ma_term { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_timing { get; set; }
        public ConcurrentDictionary<int, List<double>> para_nanpin_lot { get; set; }
        public ConcurrentDictionary<int, int> para_strategy { get; set; }
        public ConcurrentDictionary<int, double> para_rapid_side_change_ratio { get; set; }
        public ConcurrentDictionary<int, int> para_filter_id { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_window { get; set; }
        public ConcurrentDictionary<int, int> para_kijun_time_suspension { get; set; }
        public ConcurrentDictionary<int, double> para_kijun_change { get; set; }

        public double opt_para_pt { get; set; }
        public double opt_para_lc { get; set; }
        public int opt_para_num_split { get; set; }
        public int opt_para_ma_term { get; set; }
        public int opt_para_strategy { get; set; }
        public double opt_para_rapid_side_change_ratio { get; set; }
        public List<double> opt_para_nanpin_timing { get; set; }
        public List<double> opt_para_nanpin_lot { get; set; }

        public int opt_num_trade { get; set; }
        public double opt_win_rate { get; set; }
        public double opt_total_pl { get; set; }
        public double opt_realized_pl { get; set; }
        public double opt_realized_pl_sd { get; set; }
        public double opt_total_capital_sd { get; set; }
        public double opt_sharp_ratio { get; set; }
        public double opt_total_capital_gradient { get; set; }
        public double opt_window_pl_ratio { get; set; }
        

        public ParamOptGenerator()
        {
        }

        private void initializer()
        {
            para_pt = new ConcurrentDictionary<int, double>();
            para_lc = new ConcurrentDictionary<int, double>();
            para_num_split = new ConcurrentDictionary<int, int>();
            para_func = new List<int>();
            para_ma_term = new ConcurrentDictionary<int, int>();
            para_nanpin_timing = new ConcurrentDictionary<int, List<double>>();
            para_nanpin_lot = new ConcurrentDictionary<int, List<double>>();
            para_strategy = new ConcurrentDictionary<int, int>();
            para_rapid_side_change_ratio = new ConcurrentDictionary<int, double>();
            para_filter_id = new ConcurrentDictionary<int, int>();
            para_kijun_time_window = new ConcurrentDictionary<int, int>();
            para_kijun_time_suspension = new ConcurrentDictionary<int, int>();
            para_kijun_change = new ConcurrentDictionary<int, double>();

            opt_para_pt = new double();
            opt_para_lc = new double();
            opt_para_num_split = new int();
            opt_para_ma_term = new int();
            opt_para_strategy = new int();
            opt_para_rapid_side_change_ratio = new double();
            opt_para_nanpin_timing = new List<double>();
            opt_para_nanpin_lot = new List<double>();
            opt_num_trade = new int();
            opt_win_rate = new double();
            opt_total_pl = new double();
            opt_realized_pl = new double();
            opt_realized_pl_sd = new double();
            opt_total_capital_sd = new double();
            opt_sharp_ratio = new double();
            opt_total_capital_gradient = new double();
            opt_window_pl_ratio = new double();
        }


        public void generateParameters(int opt_strategy_id, int num_opt_calc, double param_opt_range)
        {
            readOptData(opt_strategy_id);
            var generator = new OptNanpinParaGenerator();
            var ptlc_range_ratio = 0.1;
            para_pt = generator.generatePtList(opt_para_pt * (1.0 - ptlc_range_ratio), opt_para_pt * (1.0 + ptlc_range_ratio));
            para_lc = generator.generateLcList(opt_para_lc * (1.0 - ptlc_range_ratio), opt_para_lc * (1.0 + ptlc_range_ratio));
            para_num_split = generateNuSplit(opt_para_num_split);
            var term_ind = MarketData.terms.IndexOf(opt_para_ma_term);
            para_ma_term = generator.generateMATerm(new List<int> { MarketData.terms[opt_para_ma_term - 1], MarketData.terms[opt_para_ma_term], MarketData.terms[opt_para_ma_term + 1] });
            para_strategy = generator.generateStrategy();
            para_rapid_side_change_ratio = generateRapidSideChange(opt_para_rapid_side_change_ratio);
            para_func = new List<int>() { 0, 1, 2 };

        }

        private void readOptData(int opt_strategy_id)
        {
            var rs = new ReadSim();
            var best_pl_list = rs.generateBestPlIndList(opt_strategy_id + 1);
            using (var sr = new StreamReader("opt nanpin.csv"))
            {
                var data = sr.ReadLine();
                for (int i = 0; i < best_pl_list[opt_strategy_id]; i++)
                    data = sr.ReadLine();
                var ele = data.Split(',');
                opt_num_trade = Convert.ToInt32(ele[1]);
                opt_win_rate = Convert.ToDouble(ele[2]);
                opt_total_pl = Convert.ToDouble(ele[3]);
                opt_realized_pl = Convert.ToDouble(ele[4]);
                opt_realized_pl_sd = Convert.ToDouble(ele[5]);
                opt_total_capital_sd = Convert.ToDouble(ele[6]);
                opt_sharp_ratio = Convert.ToDouble(ele[7]);
                opt_total_capital_gradient = Convert.ToDouble(ele[8]);
                opt_window_pl_ratio = Convert.ToDouble(ele[9]);

                opt_para_pt = Convert.ToDouble(ele[10]);
                opt_para_lc = Convert.ToDouble(ele[11]);
                opt_para_num_split = Convert.ToInt32(ele[12]);
                opt_para_ma_term = Convert.ToInt32(ele[14]);
                opt_para_strategy = Convert.ToInt32(ele[15]);
                opt_para_rapid_side_change_ratio = Convert.ToDouble(ele[16]);
                opt_para_nanpin_timing = ele[17].Split(':').Select(double.Parse).ToArray().ToList();
                opt_para_nanpin_lot = ele[18].Split(':').Select(double.Parse).ToArray().ToList();
            }
        }


        //OPTNanpinGeneratorを見たりして制約を満たせるようにIFで条件分けが必要。
        private ConcurrentDictionary<int, double> generateMinLot(double opt_min_lot)
        {
            var res = new ConcurrentDictionary<int, double>();
            var max_min_lot = opt_min_lot * 2.0;
            var min_min_lot = opt_min_lot * 0.5;
            for (int i = 0; i < 40; i++)
                res.TryAdd(i + 1, Math.Round((i + 1) * 0.005, 4));
            return res;
        }

        private ConcurrentDictionary<int ,int> generateNuSplit(int opt_split)
        {
            var res = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < 5; i++)
            {
                res.TryAdd(i-1, opt_split - 2+i);
            }
            return res;
        }
        private ConcurrentDictionary<int, double> generateRapidSideChange(double opt_change_ratio)
        {
            var res = new ConcurrentDictionary<int, double>();
            var min_ratio = opt_change_ratio * 0.9;
            var max_ratio = opt_change_ratio * 1.1;
            var i = 0;
            while (true)
            {
                var ratio = i * 0.01 + min_ratio;
                res.TryAdd(i, ratio);
                i++;
                if (ratio >= max_ratio)
                    break;
            }
            return res;
        }

    }
}
