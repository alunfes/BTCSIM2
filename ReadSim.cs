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

        public Dictionary<int, double> opt_pl { get; set; }
        public Dictionary<int, double> opt_win_rate { get; set; }
        public Dictionary<int, double> opt_num_trade { get; set; }
        public Dictionary<int, double> opt_realized_pl_var { get; set; }
        public Dictionary<int, double> opt_total_capital_var { get; set; }

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
            opt_pl = new Dictionary<int, double>();
            opt_win_rate = new Dictionary<int, double>();
            opt_num_trade = new Dictionary<int, double>();
            opt_realized_pl_var = new Dictionary<int, double>();
            opt_total_capital_var = new Dictionary<int, double>();
        }

        /*
         * 
         */
        public void startReadSim(int from, int to, int opt_term)
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
                    //"No.,num trade,win rate,realized pl,realzied pl var,total capital var,pt,lc,num_split,func,ma_term,nanpin timing,lot splits"
                    opt_num_trade.Add(no, Convert.ToDouble(ele[1]));
                    opt_win_rate.Add(no, Convert.ToDouble(ele[2]));
                    opt_pl.Add(no, Convert.ToDouble(ele[3]));
                    opt_realized_pl_var.Add(no, Convert.ToDouble(ele[4]));
                    opt_total_capital_var.Add(no, Convert.ToDouble(ele[5]));
                    para_pt.Add(no, Convert.ToDouble(ele[6]));
                    para_lc.Add(no, Convert.ToDouble(ele[7]));
                    para_ma_term.Add(no, Convert.ToInt32(ele[10]));
                    para_nanpin_timing.Add(no, ele[11].Split(':').Select(double.Parse).ToArray());
                    para_nanpin_lot.Add(no, ele[12].Split(':').Select(double.Parse).ToArray());
                    no++;
                }
            }
            //do sim
            using (var sw = new StreamWriter("read sim.csv",false))
            {
                sw.WriteLine("i,pt,lc,ma term,nanpin timing,nanpin lot,opt pl,opt realized pl var,opt total capital var,opt num trade,opt win rate,earning performance,num trade performance,win rate performance,sim realized pl var,sim total capital var");
                var progress = 0.0;
                var term_adjust = Convert.ToDouble(opt_term) / Convert.ToDouble((to - from));
                for (int i = 0; i < no; i++)
                {
                    var ac = new Account();
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
                        string.Join(":", para_nanpin_lot[i]) + "," + opt_pl[i].ToString()+","+ opt_realized_pl_var[i].ToString() +","+opt_total_capital_var[i].ToString()+","+
                        opt_num_trade[i].ToString() +","+opt_win_rate[i].ToString() +","+ pl_performance.ToString() +","+num_trade_performance.ToString()+ "," + win_rate_performance.ToString()
                        +","+ac.performance_data.realized_pl_ratio_variance.ToString()+","+ac.performance_data.total_capital_variance.ToString();
                    progress = Math.Round(100.0 * Convert.ToDouble(i) / Convert.ToDouble(no), 2);
                    sw.WriteLine(res);
                    Console.WriteLine(res);
                    Console.WriteLine(i.ToString() + "/" + no.ToString() + " - " + progress.ToString() + "%" + ": pl performance=" + pl_performance
                        +" ,win rate performance="+ win_rate_performance +" ,num trade parformance="+ num_trade_performance);
                }
            }
        }

    }
}
