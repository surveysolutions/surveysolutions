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

    self.HideOutput = function () {
        self.ListView.HideOutput();
    };

    self.ShowOutput = function () {
        self.ListView.ShowOutput();
    };

    self.Errors = ko.computed(function () {
        return self.ListView.Errors();
    });

    var myChildModel = function (data) {
        ko.mapping.fromJS(data, {}, this);

        this.IsSelected = ko.observable(false);
    };

    var countSelectedItems = function () {
        var count = 0;
        ko.utils.arrayForEach(self.ListView.Items(), function (item) {
            var value = item.IsSelected();
            if (!isNaN(value) && value) {
                count++;
            }
        });
        return count;
    };

    self.ListView.IsNothingSelected = ko.computed(function () {
        return countSelectedItems() == 0;
    });

    self.ListView.IsOnlyOneSelected = ko.computed(function () {
        return countSelectedItems() == 1;
    });

    self.Assign = function (user) {

        self.ListView.CheckForRequestComplete();

        var selectedRawInterviews = ko.utils.arrayFilter(self.ListView.Items(), function (item) {
            return item.IsSelected();
        });

        var commands = ko.utils.arrayMap(selectedRawInterviews, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                InterviewerId: user.UserId,
                InterviewId: item.InterviewId,
                UserId: self.CurrentUser.UserId
            });
        });

        var command = {
            type: "AssignInterviewerCommand",
            commands: commands
        };

        self.ListView.IsAjaxComplete(false);

        $.ajax({
            type: "POST",
            url: commandUrl,
            data: command,
            success: function (data) {
                if (data.status == "ok") {
                    ko.utils.arrayFilter(selectedRawInterviews, function (item) {
                        item.IsSelected(false);
                        item.ResponsibleId(user.UserId);
                        item.ResponsibleName(user.UserName);
                        if (item.Status() != "RejectedBySupervisor") {
                            item.Status("InterviewerAssigned");
                        }
                    });
                }
                if (data.status == "error") {
                    self.ListView.ShowErrors(data);
                }
                self.ListView.IsAjaxComplete(true);
            },
            error: function () {
                self.ListView.ShowErrors(data);
            },
            dataType: "json",
            traditional: true
        });
    };

    self.Url = new Url($interviewsPageUrl);

    self.Users = users;

    self.Templates = ko.observableArray([]);
    self.Responsibles = ko.observableArray([]);
    self.Statuses = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');
    self.SelectedResponsible = ko.observable('');
    self.SelectedStatus = ko.observable('');

    self.load = function () {
        self.ListView.GetFilterMethod = function () {
            var selectedTemplate = _.isEmpty(self.SelectedTemplate())
                ? { templateId: '', version: '' }
                : JSON.parse(self.SelectedTemplate());

            self.Url.query['templateId'] = selectedTemplate.templateId;
            self.Url.query['templateVersion'] = selectedTemplate.version;
            self.Url.query['status'] = self.SelectedStatus() || "";
            self.Url.query['interviewerId'] = self.SelectedResponsible() || "";

            if (Modernizr.history) {
                window.history.pushState({}, "Interviews", self.Url.toString());
            }
            
            return {
                TemplateId: selectedTemplate.templateId,
                TemplateVersion: selectedTemplate.version,
                ResponsibleId: self.SelectedResponsible,
                Status: self.SelectedStatus
            };
        };

        self.SelectedTemplate("{\"templateId\": \"" + location.queryString['templateId'] + "\",\"version\": \"" + location.queryString['templateVersion'] + "\"}");
        self.SelectedStatus(location.queryString['status']);
        self.SelectedResponsible(location.queryString['interviewerId']);

        self.Url.query['templateId'] = location.queryString['templateId'] || "";
        self.Url.query['templateVersion'] = location.queryString['templateVersion'] || "";
        self.Url.query['status'] = location.queryString['status'] || "";
        self.Url.query['interviewerId'] = location.queryString['interviewerId'] || "";

        self.SelectedTemplate.subscribe(self.ListView.filter);
        self.SelectedResponsible.subscribe(self.ListView.filter);
        self.SelectedStatus.subscribe(self.ListView.filter);

        self.ListView.search();
    };


};