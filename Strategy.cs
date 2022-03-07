using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BTCSIM2
{
    public class StrategyActionData
    {
        public List<string> action; //entry, cancel, ptlc
        public List<string> order_side;
        public List<string> order_type;
        public List<double> order_price;
        public List<double> order_size;
        public List<int> order_serial_num;
        public List<string> order_message;
        public double pt_price;
        public double lc_price;

        public StrategyActionData()
        {
            action = new List<string>();
            order_price = new List<double>();
            order_side = new List<string>();
            order_size = new List<double>();
            order_type = new List<string>();
            order_serial_num = new List<int>();
            order_message = new List<string>();
            pt_price = 0;
            lc_price = 0;
        }
        public void add_action(string action, string side, string type, double price, double size, double pt, double lc, int serial_num, string message)
        {
            this.action.Add(action);
            order_side.Add(side);
            order_type.Add(type);
            order_price.Add(price);
            order_size.Add(size);
            order_serial_num.Add(serial_num);
            order_message.Add(message);
            pt_price = pt;
            lc_price = lc;
        }
    }

    public class Strategy
    {
        public Strategy()
        {
        }

        /*
         * market entry and place a pt / lc order.
         * entry side will be decided randomly 50% long and 50% short
         * pt, lc order should be checked and processed, and pt or lc order should be cancelled immediaterly.
         * pt / lc price should be updated when holding price is changed.
         */
        public StrategyActionData PtlcStrategy(int i, double pt_ratio, double lc_ratio, Account ac)
        {
            var ad = new StrategyActionData();
            var r = RandomSeed.rnd.NextDouble();
            var side = r > 0.5 ? "buy" : "sell";
            var opposite_side = side == "buy" ? "sell" : "buy";
            if (ac.holding_data.holding_side == "" && ac.order_data.getNumOrders() == 0) //no holding no order
            {
                var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                ad.add_action("entry", side, "market", 0, 1, 0, 0, -1, "entry order"); //entry
                ad.add_action("ptlc", opposite_side, "", 0, 1, pt, lc, -1, "pt lc order"); //pt lc order
            }
            else if (ac.holding_data.holding_side == "" && ac.order_data.getNumOrders() > 0) //no holding, limit order
            {
                ad.add_action("cancel", "", "", 0, 0, -1, 0, 0, "cancel all orders");
                Console.WriteLine("PtlcStrategy - no holding but limit order exist, unexpected situation !");
            }
            else if (ac.holding_data.holding_side == "" && (ac.order_data.pt_order == 0 && ac.order_data.lc_order == 0)) //holding position, no pt lc order
            {
                var pt = (side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                ad.add_action("ptlc", opposite_side, "", 0, 1, pt, lc, -1, "pt lc order"); //pt lc order
                Console.WriteLine("PtlcStrategy - holding position but no pt lc order exist, unexpected situation !");
            }
            else if (ac.holding_data.holding_side == "" && (ac.order_data.pt_order > 0 && ac.order_data.lc_order > 0)) //holding position, pt lc order
            {
                //update pt lc price if holding price is updated
            }
            return ad;
        }

        /*
         * 
         */
        public StrategyActionData NanpinPtlcStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, Account ac)
        {
            var ad = new StrategyActionData();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var r = RandomSeed.rnd.NextDouble();
                    var side = r > 0.5 ? "buy" : "sell";
                    var opposite_side = side == "buy" ? "sell" : "buy";
                    var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));

                    ad.add_action("entry", side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
            }

            if (ac.holding_data.holding_size > 1.01)
                Console.WriteLine("Holding size is larger than 1.0!");

            return ad;
        }

        /*
         * 
         */
        public StrategyActionData NanpinPtlcMADivStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, bool flg_contrarian,  Account ac)
        {
            var ad = new StrategyActionData();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var side = "";
                    if (flg_contrarian)
                    {
                        if (MarketData.Divergence[ma_term][i] > 0)
                            side = "sell";
                        else
                            side = "buy";
                    }
                    else
                    {
                        if (MarketData.Divergence[ma_term][i] > 0)
                            side = "buy";
                        else
                            side = "sell";
                    }
                    var opposite_side = side == "buy" ? "sell" : "buy";
                    var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
            }

            //if (ac.holding_data.holding_size > 1.01)
            //    Console.WriteLine("Holding size is larger than 1.0!");

            return ad;
        }



        public StrategyActionData NanpinPtlcMADivRapidSideChangeStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, double rapid_side_change_ratio, Account ac)
        {
            var ad = new StrategyActionData();
            var ma_side = MarketData.Divergence[ma_term][i] > 0 ? "sell":"buy";
            var opposite_side = ma_side == "buy" ? "sell" : "buy";

            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else //proceed for nanpin entry
                {
                    var pt = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", ma_side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", ma_side, "limit", ma_side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                //最初のエントリーサイズに必要なコストの２倍以上の利益が出ていたら売って逆のsideで再エントリーする
                var min_amount_for_side_change = ac.holding_data.holding_volume * (pt_ratio * rapid_side_change_ratio);
                if (ac.holding_data.holding_side != ma_side && ac.performance_data.unrealized_pl > min_amount_for_side_change)
                {
                    var pt = Math.Abs(MarketData.Close[i] - ac.holding_data.holding_price);
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //revise pt lc order
                }
                else
                {
                    var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                        ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            return ad;
        }



        public StrategyActionData NanpinPtlcMADivRapidSideChangeFilterStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, double rapid_side_change_ratio, int filter_id, int kijun_time_window, double kijun_change, int kijun_time_suspension, Account ac)
        {
            var ad = new StrategyActionData();
            var ma_side = MarketData.Divergence[ma_term][i] > 0 ? "sell" : "buy";
            var opposite_side = ma_side == "buy" ? "sell" : "buy";
            //var filter = new StrategyFilter();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else if(StrategyFilter.applyFilter(i, ref ac, filter_id, kijun_time_window, kijun_change, kijun_time_suspension)==false) //proceed for nanpin entry
                {
                    var pt = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (ma_side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", ma_side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", ma_side, "limit", ma_side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                //最初のエントリーサイズに必要なコストの２倍以上の利益が出ていたら売って逆のsideで再エントリーする
                var min_amount_for_side_change = ac.holding_data.holding_volume * (pt_ratio * rapid_side_change_ratio);
                if (ac.holding_data.holding_side != ma_side && ac.performance_data.unrealized_pl > min_amount_for_side_change)
                {
                    var pt = Math.Abs(MarketData.Close[i] - ac.holding_data.holding_price);
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //revise pt lc order
                }
                else
                {
                    var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                    var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                    if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                        ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            return ad;
        }


        public StrategyActionData NanpinPtlcMADivFilterStrategy(int i, double pt_ratio, double lc_ratio, List<double> nanpin_timing, List<double> lot_splits, int ma_term, int filter_id, int kijun_time_window, double kijun_change, int kijun_time_suspension, Account ac)
        {
            var ad = new StrategyActionData();
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() > 0) //pt or lc was executed and nanpin limit order is remained
                {
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                }
                else if((StrategyFilter.applyFilter(i, ref ac, filter_id, kijun_time_window, kijun_change, kijun_time_suspension) == false))
                {
                    var side = "";
                    if (MarketData.Divergence[ma_term][i] > 0)
                        side = "sell";
                    else
                        side = "buy";
                    var opposite_side = side == "buy" ? "sell" : "buy";
                    var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * pt_ratio) : Math.Round(MarketData.Close[i] * pt_ratio));
                    var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * lc_ratio) : Math.Round(MarketData.Close[i] * lc_ratio));
                    ad.add_action("entry", side, "market", 0, lot_splits[0], 0, 0, -1, "entry order"); //first entry
                    for (int j = 0; j < nanpin_timing.Count(); j++)
                        ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * nanpin_timing[j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * nanpin_timing[j]), lot_splits[j + 1], 0, 0, 0, "nanpin entry");
                    ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                }
            }
            else //update pt lc price
            {
                var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * pt_ratio) : Math.Round(ac.holding_data.holding_price * pt_ratio));
                var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * lc_ratio) : Math.Round(ac.holding_data.holding_price * lc_ratio));
                if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                    ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
            }
            return ad;
        }


        //(ad, selected_strategy_id, strategy_applied_point)
        //
        public (StrategyActionData, int, int) SelectNanpinStrategy(int i, int current_selected_strategy, int strategy_aplied_point, ref List<Account> strategy_ac_list, Account ac, ref ConcurrentDictionary<int, double> para_pt,
            ref ConcurrentDictionary<int, double> para_lc, ref ConcurrentDictionary<int, int> para_ma_term, ref ConcurrentDictionary<int, int> para_strategy_id,
            ref ConcurrentDictionary<int, double> para_rapid_side_change_ratio, ref ConcurrentDictionary<int, List<double>> para_nanpin_timing,
            ref ConcurrentDictionary<int, List<double>> para_nanpin_lot, ref int select_time_window, ref int pre_time_window, ref int strategy_time_window, ref double subordinate_ratio)
        {
            var ad = new StrategyActionData();
            var best_id = current_selected_strategy;
            if (ac.holding_data.holding_side == "") //no holding place a first entry order
            {
                if (ac.order_data.getNumOrders() == 0) //new entry
                {
                    best_id = getBestStrategy(i, strategy_ac_list, ref strategy_time_window);
                    ad = placeNanpinOrder(i, best_id, ad, ref para_pt, ref para_lc, ref para_ma_term, ref para_nanpin_timing, ref para_nanpin_lot);
                    strategy_aplied_point = i;
                }
            }
            else
            {
                var if_change = checkPerformance(i, current_selected_strategy, strategy_aplied_point, strategy_ac_list, ref select_time_window, ref pre_time_window, ref subordinate_ratio);
                if (if_change) //change current strategy
                {
                    best_id = getBestStrategy(i, strategy_ac_list, ref strategy_time_window);
                    ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                    ad.add_action("ptlc", "", "", 0, 0, 0, 0, -1, "cancel pt lc order");
                    ad.add_action("entry", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "market", 0, ac.holding_data.holding_size, 0, 0, -1, "exit order as new strategy was selected");
                    ad = placeNanpinOrder(i, best_id, ad, ref para_pt, ref para_lc, ref para_ma_term, ref para_nanpin_timing, ref para_nanpin_lot);
                    strategy_aplied_point = i;
                }
                else //continue with the current strategy
                {
                    if (para_strategy_id[current_selected_strategy] == 1) //rapide side change strategy
                    {
                        var min_amount_for_side_change = ac.holding_data.holding_volume * (para_pt[current_selected_strategy] * para_rapid_side_change_ratio[current_selected_strategy]);
                        var ma_side = MarketData.Divergence[para_ma_term[current_selected_strategy]][i] > 0 ? "sell" : "buy";
                        if (ac.holding_data.holding_side != ma_side && ac.performance_data.unrealized_pl > min_amount_for_side_change)
                        {
                            var pt = Math.Abs(MarketData.Close[i] - ac.holding_data.holding_price);
                            var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]) : Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]));
                            ad.add_action("cancel", "", "", 0, 0, 0, 0, -1, "cancel all orders");
                            ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //revise pt lc order
                        }
                        else
                        {
                            var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * para_pt[current_selected_strategy]) : Math.Round(ac.holding_data.holding_price * para_pt[current_selected_strategy]));
                            var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]) : Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]));
                            if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                                ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                        }
                    }
                    else //non rapid side change strategy
                    {
                        var pt = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * para_pt[current_selected_strategy]) : Math.Round(ac.holding_data.holding_price * para_pt[current_selected_strategy]));
                        var lc = (ac.holding_data.holding_side == "buy" ? Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]) : Math.Round(ac.holding_data.holding_price * para_lc[current_selected_strategy]));
                        if (ac.order_data.pt_order != pt || ac.order_data.lc_order != lc)
                            ad.add_action("ptlc", ac.holding_data.holding_side == "buy" ? "sell" : "buy", "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                    }
                }
            }


            /*
             * どの戦略に切り替えるべきか：
             * ->過去x時間(strategy time window)におけるplが一番高いもの。（plが高くても直近でマイナスパフォーマンスになっているものを避ける工夫が必要）
             */
            int getBestStrategy(int i, List<Account> strategy_ac_list, ref int strategy_time_window)
            {
                var pl_list = new List<double>();
                for(int n=0; n<strategy_ac_list.Count; n++)
                {
                    var latest_total_capital = strategy_ac_list[n].performance_data.total_capital_list[i];
                    var pre_total_capital = strategy_ac_list[n].performance_data.total_capital_list[i-strategy_time_window];
                    pl_list.Add((latest_total_capital - pre_total_capital) / pre_total_capital);
                }
                return pl_list.IndexOf(pl_list.Max());
            }

            /*
             * 切り替えタイミング：
             * ->過去x時間(select_time_window)における時間あたりのplが、採用開始前のy時間(pre_time_window)の時間あたりplよりもz％(subordinate_ratio)以上劣後した場合。
             */
            bool checkPerformance(int i, int current_selected_strategy, int strategy_aplied_point, List<Account> strategy_ac_list, ref int select_time_window, ref int pre_time_window, ref double subordinate_ratio)
            {
                if (i - strategy_aplied_point >= select_time_window)
                {
                    var select_current_total_capital = strategy_ac_list[current_selected_strategy].performance_data.total_capital_list[i];
                    var select_pre_total_capital = strategy_ac_list[current_selected_strategy].performance_data.total_capital_list[i - select_time_window];
                    var select_ave_pl = (select_current_total_capital - select_pre_total_capital) / Convert.ToDouble(select_time_window);
                    var applied_current_capital = strategy_ac_list[current_selected_strategy].performance_data.total_capital_list[strategy_aplied_point];
                    var applied_pre_capital = strategy_ac_list[current_selected_strategy].performance_data.total_capital_list[strategy_aplied_point - pre_time_window];
                    var applied_ave_pl = (applied_current_capital - applied_pre_capital) / Convert.ToDouble(pre_time_window);

                    if (select_ave_pl <= applied_ave_pl * (1.0 - subordinate_ratio))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }


            StrategyActionData placeNanpinOrder(int i, int best_id, StrategyActionData ad, ref ConcurrentDictionary<int, double> para_pt,
            ref ConcurrentDictionary<int, double> para_lc, ref ConcurrentDictionary<int, int> para_ma_term,
            ref ConcurrentDictionary<int, List<double>> para_nanpin_timing, ref ConcurrentDictionary<int, List<double>> para_nanpin_lot)
            {
                var side = "";
                if (MarketData.Divergence[para_ma_term[best_id]][i] > 0)
                    side = "sell";
                else
                    side = "buy";
                var opposite_side = side == "buy" ? "sell" : "buy";
                var pt = (side == "buy" ? Math.Round(MarketData.Close[i] * para_pt[best_id]) : Math.Round(MarketData.Close[i] * para_pt[best_id]));
                var lc = (side == "buy" ? Math.Round(MarketData.Close[i] * para_lc[best_id]) : Math.Round(MarketData.Close[i] * para_lc[best_id]));
                ad.add_action("entry", side, "market", 0, para_nanpin_lot[best_id][0], 0, 0, -1, "entry order"); //first entry
                for (int j = 0; j < para_nanpin_timing[best_id].Count; j++)
                    ad.add_action("entry", side, "limit", side == "buy" ? Math.Round(MarketData.Close[i] - MarketData.Close[i] * para_nanpin_timing[best_id][j]) : Math.Round(MarketData.Close[i] + MarketData.Close[i] * para_nanpin_timing[best_id][j]), para_nanpin_lot[best_id][j + 1], 0, 0, 0, "nanpin entry");
                ad.add_action("ptlc", opposite_side, "", 0, 0, pt, lc, -1, "pt lc order"); //pt lc order
                return ad;
            }

            return (ad, best_id, strategy_aplied_point);
        }


    }
}
