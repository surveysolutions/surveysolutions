Supervisor.VM.Users = function (listViewUrl) {
    Supervisor.VM.Users.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);
    self.UsersCount = ko.observable(0);
    
    self.onDataTableDataReceived = function(data) {
        self.UsersCount(data.recordsTotal);
    };
    
    self.load = function () {

        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
    };

    self.onTableInitCompleteExtra = function () { };

    self.onTableInitComplete = function () {
        $('.dataTables_filter label')
            .on('click',
                function (e) {
                    if (e.target !== this)
                        return;
                    if ($(this).hasClass("active")) {
                        $(this).removeClass("active");
                    } else {
                        $(this).addClass("active");
                        $(this)
                            .children("input[type='search']")
                            .delay(200)
                            .queue(function () {
                                $(this).focus();
                                $(this).dequeue();
                            });
                    }
                });

        self.onTableInitCompleteExtra();
    };

};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Users, Supervisor.VM.ListView);
