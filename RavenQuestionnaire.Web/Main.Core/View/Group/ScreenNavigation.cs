using System;
using System.Collections.Generic;

namespace Main.Core.View.Group
{
    /// <summary>
    /// The screen navigation.
    /// </summary>
    public class ScreenNavigation
    {
        public ScreenNavigation()
        {
            this.BreadCumbs = new List<CompleteGroupHeaders>();
        }
        public List<CompleteGroupHeaders> BreadCumbs { get; set; }

        public string CurrentScreenTitle { get; set; }

        public CompleteGroupHeaders NextScreen { get; set; }

        public CompleteGroupHeaders PrevScreen { get; set; }

        public Guid PublicKey { get; set; }
    }
}