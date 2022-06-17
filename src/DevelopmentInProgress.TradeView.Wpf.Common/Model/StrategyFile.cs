﻿using System.IO;

namespace DevelopmentInProgress.TradeView.Wpf.Common.Model
{
    public class StrategyFile : EntityBase
    {
        private string file;

        public string File
        {
            get { return file; }
            set
            {
                if (file != value)
                {
                    file = value;
                    var fileInfo = new FileInfo(file);
                    DisplayName = fileInfo.Name;
                    OnPropertyChanged(nameof(File));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string DisplayName { get; private set; }

        public StrategyFileType FileType { get; set; }
    }
}