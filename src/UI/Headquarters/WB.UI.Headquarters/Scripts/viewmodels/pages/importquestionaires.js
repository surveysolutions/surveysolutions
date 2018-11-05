Supervisor.VM.ImportQuestionnaires = function (listViewUrl) {
    Supervisor.VM.ImportQuestionnaires.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);
    self.defaultOrder = [[1, 'desc']];

    self.onDataTableDataReceived = function (data) {};

    self.load = function () {

        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
        self.reloadDataTable();
    };

    self.onTableInitCompleteExtra = function () { };

    self.onTableInitComplete = function () {
        self.onTableInitCompleteExtra();
    };

};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportQuestionnaires, Supervisor.VM.ListView);
