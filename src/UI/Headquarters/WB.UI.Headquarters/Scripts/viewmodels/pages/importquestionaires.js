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

        //$('#data_holder_filter label').on('click', function (e) {
        //        if (e.target !== this)
        //            return;
        //        if ($(this).hasClass("active")) {
        //            $(this).removeClass("active");
        //        }
        //        else {
        //            $(this).addClass("active");
        //        }
        //        $(".column-questionnaire-title").toggleClass("padding-left-slide");
        //    });
        
        self.onTableInitCompleteExtra();
    };

};

Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportQuestionnaires, Supervisor.VM.ListView);
