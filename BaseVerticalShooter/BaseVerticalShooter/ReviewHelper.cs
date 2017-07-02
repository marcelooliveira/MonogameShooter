﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Store;

namespace BaseVerticalShooter
{
    public class ReviewHelper : BaseVerticalShooter.IReviewHelper
    {
        public async void MarketPlaceReviewTask()
        {
            var uri = new Uri(string.Format("ms-windows-store:navigate?appid={0}", CurrentApp.AppId));
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
