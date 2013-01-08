﻿using System.Collections.Concurrent;
namespace MyFirstExpert
{
    public class NewsShooterEA : EExpertAdvisor
    {
        private double lotSize = 0.1;
        private double range = 100;
        private double takeProfit = 100;
        private double stopLoss = 100;

        private NewsReminder reminder;

        protected override int DeInit()
        {
            return 0;
        }

        protected override int Init()
        {
            reminder = new NewsReminder();
            reminder.Start();

            return 0;
        }

        protected override int Start()
        {
            while (reminder.IsAvailable)
            {
                MagicBoxOrder result = null;

                reminder.OrderQueue.TryDequeue(out result);

                if (result == null) continue;

                CreateOrderBox(result);
            }

            // 1. One Cancel another
            // 2. Trailing stop and Lock Profit
            // 3. Delete after 10 minutes, Expired 

            return 0;
        }

        private void CreateOrderBox(MagicBoxOrder magicBox)
        {
            PendingBuy(magicBox.Symbol, lotSize,
                        BuyOpenPriceFor(magicBox.Symbol) + range * PointFor(Symbol),
                        BuyClosePriceFor(magicBox.Symbol) + ((range - stopLoss) * PointFor(Symbol)),
                        BuyClosePriceFor(magicBox.Symbol) + ((range + takeProfit) * PointFor(Symbol)));

            PendingSell(magicBox.Symbol, lotSize,
                        SellOpenPriceFor(magicBox.Symbol) - range * PointFor(Symbol),
                        SellClosePriceFor(magicBox.Symbol) - ((range - stopLoss) * PointFor(Symbol)),
                        SellClosePriceFor(magicBox.Symbol) - ((range + takeProfit) * PointFor(Symbol)));
        }
    }
}