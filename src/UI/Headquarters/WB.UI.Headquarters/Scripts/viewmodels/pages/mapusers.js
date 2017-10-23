Supervisor.VM.MapUsers = function (listViewUrl, notifier, ajax, $deleteMapUserUrl) {
    Supervisor.VM.MapUsers.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);

    self.onDataTableDataReceived = function (data) {};

    self.load = function () {

        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
        self.reloadDataTable();
    };

    self.onTableInitCompleteExtra = function () { };

    self.sendDeleteMapUserCommand = function (item) {
        ajax.sendRequest($deleteMapUserUrl, "post", { map: self.Url.query['mapname'], user:item.userName }, false,
            // onSuccess
            function () {
                setTimeout(function () { self.reloadDataTable(); }, 1000);
            });
    }

    self.GetFilterMethod = function () {
        

        return { MapName: self.Url.query['mapname'] };
    };

    self.onTableInitComplete = function () {

        $('#data_holder_filter label').on('click', function (e) {
                if (e.target !== this)
                    return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                }
                else {
                    $(this).addClass("active");
                }
                $(".column-questionnaire-title").toggleClass("padding-left-slide");
            });
        
        self.onTableInitCompleteExtra();
    };

    self.selectRowAndGetData = function (selectedItem) {
        self.Datatable.rows().deselect();
        var rowIndex = selectedItem.parent().children().index(selectedItem);
        self.Datatable.row(rowIndex).select();
        var selectedRows = self.Datatable.rows({ selected: true }).data()[0];
        return selectedRows;
    }

    self.deleteMapUser = function (key, opt) {
        var selectedRow = self.selectRowAndGetData(opt.$trigger);

        notifier.confirm('Confirmation Needed', input.settings.messages.deleteMapConfirmationMessage,
            // confirm
            function () { self.sendDeleteMapUserCommand(selectedRow); },
            // cancel
            function () { });
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.MapUsers, Supervisor.VM.ListView);