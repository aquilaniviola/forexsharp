﻿using System;
using TradePlatform.MT4.SDK.API;

namespace FXSharp.TradingPlatform.Exts
{
    public class Order
    {
        private int ticket;
        private EExpertAdvisor ea;
        private double lots;
        //private ORDER_TYPE orderType;
        private DateTime NULL_TIME = new DateTime(621355968000000000);
        private double openPrice;
        private string symbol;

        public Order(string symbol, int ticket, double lots, EExpertAdvisor ea)
        {
            this.symbol = symbol;
            this.ticket = ticket;
            this.ea = ea;
            this.lots = lots;
            //this.orderType = orderType;
        }

        public void Close()
        {
            bool success = false;

            if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET) && ea.OrderCloseTime() != NULL_TIME)
                return;

            var orderType = ea.OrderType();
            // should refactor to hierarcy

            if (orderType == ORDER_TYPE.OP_BUY)
            {
                success = ea.OrderClose(ticket, lots, ea.BuyClosePriceFor(symbol), 3);
            }
            else if (orderType == ORDER_TYPE.OP_SELL)
            {
                success = ea.OrderClose(ticket, lots, ea.SellClosePriceFor(symbol), 3);
            }
            else
            {
                success = ea.OrderDelete(ticket);
            }

            if (!success)
                ea.ThrowLatestException();
        }

        public double Profit
        {
            get
            {
                if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                    return ea.OrderProfit();

                ea.ThrowLatestException();

                return -1;
            }
        }

        public bool CloseInProfit()
        {
            if (Profit < 0.0) return false;

            Close();

            return true;
        }

        public void ModifyStopLoss(double newStopLoss)
        {
            bool success = false;
            // should throw exception for each call
            if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                success = ea.OrderModify(ticket, ea.OrderOpenPrice(), newStopLoss, ea.OrderTakeProfit(),
                                         DateTime.Now.AddDays(100), -1);

            if (!success)
            {
                ea.ThrowLatestException();
            }
        }

        public void ModifyTakeProfit(double newTakeProfit)
        {
            bool success = false;
            // should throw exception for each call
            if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                success = ea.OrderModify(ticket, ea.OrderOpenPrice(), ea.OrderStopLoss(), newTakeProfit,
                                         DateTime.Now.AddDays(100), -1);

            if (!success)
            {
                ea.ThrowLatestException();
            }
        }

        public double OpenPrice
        {
            get
            {
                if (openPrice != default(double)) 
                    return openPrice;

                // should throw exception for each call
                if (!ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                    return openPrice;

                openPrice = ea.OrderOpenPrice();
                return openPrice;
            }
        }

        public double StopLoss
        {
            get
            {
                // should cache stop loss 

                bool success = false;
                // should throw exception for each call
                if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                    return ea.OrderStopLoss();

                if (!success)
                {
                    ea.ThrowLatestException();
                }

                return 0;
            }
        }

        public bool IsOpen
        {
            get
            {
                return ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET) && ea.OrderCloseTime() == NULL_TIME;
            }
        }

        public bool IsRunning
        {
            get
            {
                return ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET) && (ea.OrderType() == ORDER_TYPE.OP_BUY || ea.OrderType() == ORDER_TYPE.OP_SELL);
            }
        }

        public string Symbol
        {
            get { return symbol; }
        }

        public double ProfitPoints
        {
            get
            {
                var orderType = ea.OrderType();

                if (orderType == ORDER_TYPE.OP_BUY)
                {
                    return ea.BuyClosePriceFor(this.symbol) - OpenPrice;
                }
                else if (orderType == ORDER_TYPE.OP_SELL)
                {
                    return OpenPrice - ea.SellClosePriceFor(this.symbol);
                }
                else
                {
                    throw  new ApplicationException("Only market order has profit");
                }
            }
        }

        public int Digits
        {
            get { return ea.DigitsFor(symbol); }
        }

        public double Points
        {
            get { return ea.PointFor(symbol); }
        }

        private ORDER_TYPE OrderType
        {
            get
            {
                if (ea.OrderSelect(ticket, SELECT_BY.SELECT_BY_TICKET))
                    return ea.OrderType();

                ea.ThrowLatestException();

                return default(ORDER_TYPE);
            }
        }

        public void ChangeTakeProfitInPoints(double tpPoints)
        {
            var orderType = OrderType;

            var newTp = 0.0;

            if (orderType == ORDER_TYPE.OP_BUY)
            {
                newTp = ea.BuyClosePriceFor(symbol) + (tpPoints * Points);
            }
            else if (orderType == ORDER_TYPE.OP_SELL)
            {
                newTp = ea.SellClosePriceFor(symbol) - (tpPoints * Points);
            }

            ModifyTakeProfit(newTp);
        }

        public void ChangeStopLossInPoints(double slPoints)
        {
            var orderType = OrderType;

            var newSl = 0.0;

            if (orderType == ORDER_TYPE.OP_BUY)
            {
                newSl = ea.BuyClosePriceFor(symbol) - (slPoints * Points);
            }
            else if (orderType == ORDER_TYPE.OP_SELL)
            {
                newSl = ea.SellClosePriceFor(symbol) + (slPoints * Points);
            }

            ModifyStopLoss(newSl);
        }
    }
}
