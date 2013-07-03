SurveyListViewModel = function (templateApiUrl) {
    var self = this;

    self.ServiceUrl = templateApiUrl;

    self.Users = ko.observableArray([]);

    self.SelectedUser = ko.observable('');

    self.load = function () {

        $.ajax({
            type: 'POST',
            url: self.ServiceUrl,
            data: {},
            context: this,
            success: function (data) {
                self.Users(data);
            },
            dataType: 'json'
        });
    };

    self.load();
};