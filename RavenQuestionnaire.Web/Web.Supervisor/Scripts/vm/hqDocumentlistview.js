DocumentListViewModel = function (currentUser, listViewUrl, commandUrl, users) {
    var self = this;

    self.CurrentUser = currentUser;

    self.ListView = new ListViewModel(listViewUrl);

    self.ListView.mappingOptions = {
        'Items': {
            create: function (item) {
                //item.data.IsSelected = ko.observable(false);
                return new myChildModel(item.data);
            }
        }
    };

    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };

    var myChildModel = function (data) {
        ko.mapping.fromJS(data, {}, this);

        this.IsSelected = ko.observable(false);
    };

    self.ListView.IsNothingSelected = ko.computed(function () {
        var count = 0;
        ko.utils.arrayForEach(self.ListView.Items(), function (item) {
            var value = item.IsSelected();
            if (!isNaN(value) && value) {
                count++;
            }
        });

        return count == 0;
    });

    self.deleteInterview = function () {
        self.ListView.CheckForRequestComplete();

        var selectedRawInterviews = ko.utils.arrayFilter(self.ListView.Items(), function (item) {
            return item.IsSelected();
        });

        var request = { Interviews: [] };
        for (var i = 0; i < selectedRawInterviews.length; i++) {
            request.Interviews.push(selectedRawInterviews[i]["InterviewId"]());
        }

        var commands = ko.utils.arrayMap(selectedRawInterviews, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                InterviewId: item.InterviewId,
                UserId: self.CurrentUser.UserId
            });
        });

        var command = {
            type: "DeleteInterviewCommand",
            commands: commands
        };

        self.ListView.IsAjaxComplete(false);


        $.ajax({
            type: "POST",
            url: commandUrl,
            data: command,
            success: function (data) {
                if (data.status == "ok") {
                    for (var i = 0; i < selectedRawInterviews.length; i++) {
                        self.ListView.Items.remove(selectedRawInterviews[i]);
                    }

                    self.ListView.TotalCount(self.ListView.TotalCount() - selectedRawInterviews.length);
                }
                if (data.status == "error") {
                    var faildInterviews = ko.utils.arrayMap(data.failedCommands, function (failedCommand) {
                        return failedCommand.InterviewId;
                    });
                    var deletedInterviews = ko.utils.arrayFilter(selectedRawInterviews, function (item) {
                        return $.inArray(item["InterviewId"](), faildInterviews) == -1;
                    });

                    for (var i = 0; i < deletedInterviews.length; i++) {
                        self.ListView.Items.remove(deletedInterviews[i]);
                    }

                    self.ListView.TotalCount(self.ListView.TotalCount() - deletedInterviews.length);
                }
                self.ListView.IsAjaxComplete(true);
            },
            error: function () {

            },
            dataType: "json",
            traditional: true
        });

    };

    self.Users = users;

    self.Templates = ko.observableArray([]);
    self.Responsibles = ko.observableArray([]);
    self.Statuses = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');
    self.SelectedResponsible = ko.observable('');
    self.SelectedStatus = ko.observable('');

    self.load = function () {
        self.ListView.GetFilterMethod = function () {
            return {
                TemplateId: self.SelectedTemplate,
                ResponsibleId: self.SelectedResponsible,
                Status: self.SelectedStatus
            };
        };

        self.SelectedTemplate(location.queryString['templateid']);
        self.SelectedStatus(location.queryString['status']);

        self.SelectedTemplate.subscribe(self.ListView.filter);
        self.SelectedResponsible.subscribe(self.ListView.filter);
        self.SelectedStatus.subscribe(self.ListView.filter);

        self.ListView.search();
    };


};