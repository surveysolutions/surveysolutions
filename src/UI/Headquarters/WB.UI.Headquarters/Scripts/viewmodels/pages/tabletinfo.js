Supervisor.VM.TabletInfo = function (listViewUrl, responsiblesUrl) {
    Supervisor.VM.TabletInfo.superclass.constructor.apply(this, arguments);

    var self = this;

    self.load = function () {
        self.search();
    };
    
    /*self.load = function () {
        if (self.QueryString['responsible']) {
            self.SelectedResponsible({ UserName: self.QueryString['responsible'] });
        }
        self.SelectedResponsible.subscribe(self.filter);
        self.search();
    };*/
    
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.TabletInfo, Supervisor.VM.ListView);