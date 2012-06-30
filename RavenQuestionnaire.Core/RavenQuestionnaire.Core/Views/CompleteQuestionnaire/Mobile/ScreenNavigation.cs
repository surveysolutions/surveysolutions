using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class ScreenNavigation
    {
        public ScreenNavigation()
        {
            BreadCumbs = new List<CompleteGroupHeaders>();
        }

        public List<CompleteGroupHeaders> BreadCumbs { get; set; }

        public CompleteGroupHeaders NextScreen { get; set; }

        public CompleteGroupHeaders PrevScreen { get; set; }

        

        public string CurrentScreenTitle { get; set; }

        public Guid PublicKey { get; set; }
    }
}