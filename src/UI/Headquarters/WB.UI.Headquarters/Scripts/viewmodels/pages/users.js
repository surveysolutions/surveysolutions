Supervisor.VM.Users = function (listViewUrl) {
    Supervisor.VM.Users.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);
    self.UsersCount = ko.observable(0);
    
    self.onTableInitComplete = function () { }

    self.onDataTableDataReceived = function(data) {
        self.UsersCount(data.recordsTotal);
    };
    
    self.load = function () {

        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
        self.reloadDataTable();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Users, Supervisor.VM.ListView);