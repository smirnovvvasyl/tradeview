﻿using DevelopmentInProgress.TradeView.Core.Enums;
using System;

namespace DevelopmentInProgress.TradeView.Wpf.Common.Model
{
    public class Order : EntityBase
    {
        private string status;
        private decimal executedQuantity;
        
        public string Symbol { get; set; }
        public Exchange Exchange { get; set; }
        public new string Id { get; set; }
        public string ClientOrderId { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalQuantity { get; set; }
        public string TimeInForce { get; set; }
        public string Type { get; set; }
        public string Side { get; set; }
        public decimal StopPrice { get; set; }
        public decimal IcebergQuantity { get; set; }
        public DateTime Time { get; set; }
        public bool IsWorking { get; set; }

        public string Status
        {
            get { return status; }
            set
            {
                if(status != value)
                {
                    status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public decimal ExecutedQuantity
        {
            get { return executedQuantity; }
            set
            {
                if (executedQuantity != value)
                {
                    executedQuantity = value;
                    OnPropertyChanged(nameof(ExecutedQuantity));
                    OnPropertyChanged(nameof(FilledPercent));
                }
            }
        }

        public int FilledPercent
        {
            get
            {
                if (OriginalQuantity == 0)
                {
                    return 0;
                }

                return (int)((ExecutedQuantity / OriginalQuantity) * 100);
            }
        }
    }
}