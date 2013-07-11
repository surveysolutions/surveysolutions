SummaryListViewModel = function (templateApiUrl) {
    var self = this;

    self.ServiceUrl = templateApiUrl;

    self.Templates = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');

    self.load = function () {

        $.ajax({
            type: 'POST',
            url: self.ServiceUrl,
            data: {},
            context: this,
            success: function (data) {
                self.Templates(data);
            },
            dataType: 'json'
        });
    };

    self.load();
};