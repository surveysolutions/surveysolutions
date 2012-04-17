using System;
using System.Collections.Generic;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class HomeDetailData : ModuleData {
        Home home;
        IList<HomePhoto> photos;
        byte[] layout;
        Agent agent;

        public HomeDetailData(Home home) {
            Home = home;
        }
        public override void Load() {
            base.Load();
            if(Home == null) return;
            Photos = DataSource.Current.GetPhotos(Home);
            Layout = DataSource.Current.GetLayout(Home);
            Agent = DataSource.Current.GetHomeAgent(Home);
        }
        public Home Home {
            get { return home; }
            private set { SetValue<Home>("Home", ref home, value); }
        }
        public IList<HomePhoto> Photos {
            get { return photos; }
            private set { SetValue<IList<HomePhoto>>("Photos", ref photos, value); }
        }
        public byte[] Layout {
            get { return layout; }
            private set { SetValue<byte[]>("Layout", ref layout, value); }
        }
        public Agent Agent {
            get { return agent; }
            private set { SetValue<Agent>("Agent", ref agent, value); }
        }
    }
    public class HomeDetail : Module {
        List<object> photos;

        public override void InitData(object parameter) {
            base.InitData(parameter);
            List<object> list = new List<object>();
            list.Add(HomeDetailData.Home);
            list.AddRange(HomeDetailData.Photos);
            Photos = list;
        }
        public HomeDetailData HomeDetailData { get { return (HomeDetailData)Data; } }
        public List<object> Photos {
            get { return photos; }
            private set { SetValue<List<object>>("Photos", ref photos, value); }
        }
    }
}
