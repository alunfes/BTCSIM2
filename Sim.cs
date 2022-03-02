using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace BTCSIM2
{
    public class Sim
    {
        public Sim()
        {
        }

        public Account sim_ptlc(int from, int to, Account ac, double pt_ratio, double lc_ratio)
        {
            RandomSeed.initialize();
            var strategy = new Strategy();
            for (int i=from; i<to-1; i++)
            {
                var actions = strategy.PtlcStrategy(i, pt_ratio, lc_ratio, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


        public Account sim_nanpin_ptlc(int from, int to, Account ac, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits)
        {
            RandomSeed.initialize();
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var actions = strategy.NanpinPtlcStrategy(i, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }

        public Account sim_madiv_nanpin_ptlc(ref int from, ref int to, Account ac, ref double pt_ratio, ref double lc_ratio, ref List<double> nanpin_timing, ref List<double> lot_splits, ref int ma_term, bool flg_contrarian)
        {
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var actions = strategy.NanpinPtlcMADivStrategy(i, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term, flg_contrarian, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }

        public Account sim_madiv_nanpin_rapid_side_change_ptlc(ref int from, ref int to, Account ac, ref double pt_ratio, ref double lc_ratio, ref List<double> nanpin_timing, ref List<double> lot_splits, ref int ma_term, ref double rapid_side_change_ratio)
        {
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var actions = strategy.NanpinPtlcMADivRapidSideChangeStrategy(i, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term, rapid_side_change_ratio, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


        public Account sim_madiv_nanpin_filter_ptlc(ref int from, ref int to, Account ac, ref double pt_ratio, ref double lc_ratio, ref List<double> nanpin_timing, ref List<double> lot_splits, ref int ma_term, ref int filter_id, ref int kijun_time_window, ref double kijun_change, ref int kijun_time_suspension)
        {
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var actions = strategy.NanpinPtlcMADivFilterStrategy(i, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term, filter_id, kijun_time_window, kijun_change, kijun_time_suspension, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


        public Account sim_madiv_nanpin_rapid_side_change_filter_ptlc(ref int from, ref int to, Account ac, ref double pt_ratio, ref double lc_ratio, ref List<double> nanpin_timing, ref List<double> lot_splits, ref int ma_term, ref double rapid_side_change_ratio, ref int filter_id, ref int kijun_time_window, ref double kijun_change, ref int kijun_time_suspension)
        {
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var actions = strategy.NanpinPtlcMADivRapidSideChangeFilterStrategy(i, pt_ratio, lc_ratio, nanpin_timing, lot_splits, ma_term, rapid_side_change_ratio, filter_id, kijun_time_window, kijun_change, kijun_time_suspension, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "ptlc")
                        ac.entry_ptlc(actions.pt_price, actions.lc_price);
                }
                ac.move_to_next(i + 1);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


    }
}
