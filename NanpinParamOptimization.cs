using System;

using System.Collections.Concurrent;


namespace BTCSIM2
{
    public class NanpinParamOptimization
    {
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

        public NanpinParamOptimization()
        {
        }

        public void initialize()
        {
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


        //param opt range=0.01 - 0.1
        public void startNanpinParamOpt(int from, int to, int opt_strategy_id, string lev_fixed_trading, int num_opt_calc, double param_opt_range)
        {
            
        }


        

    }
}
