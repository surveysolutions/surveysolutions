DocumentListViewModel = function (listViewUrl, commandUrl, users) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);

    self.ListView.mappingOptions = {
        'Items': {
            create: function (item) {
                //item.data.IsSelected = ko.observable(false);
                return new myChildModel(item.data);
            }
        }
    };

    var myChildModel = function(data) {
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

        return count==0;
    });

    self.Assign = function (user) {
        var selectedRawInterviews = ko.utils.arrayFilter(self.ListView.Items(), function (item) {
            return item.IsSelected();
        });
        var commands = ko.utils.arrayMap(selectedRawInterviews, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                UserId: user.UserId,
                InterviewId: item.InterviewId
            });
        });
        var command = {
            type: "AssignInterviewToUserCommand",
            commands:  commands
        };
        self.ListView.IsPageLoaded(true);
        
        $.ajax({
            type: "POST",
            url: commandUrl,
            data: command,
            success: function (data) {
                if (data.status == "ok") {
                    ko.utils.arrayFilter(selectedRawInterviews, function(item) {
                        item.IsSelected(false);
                        item.Responsible.Id(user.UserId);
                        item.Responsible.Name(user.UserName);
                    });
                }
                if (data.status == "error") {
                    
                }
                self.ListView.IsPageLoaded(true);
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
    self.OnlyAssigned = ko.observable(false);

    self.load = function () {
        self.ListView.GetFilterMethod = function () {
            return {
                TemplateId: self.SelectedTemplate,
                ResponsibleId: self.SelectedResponsible,
                StatusId: self.SelectedStatus,
                OnlyAssigned: self.OnlyAssigned
            };
        };
        
        self.SelectedTemplate(location.queryString['templateid']);
        self.SelectedStatus(location.queryString['status']);

        self.SelectedTemplate.subscribe(self.ListView.filter);
        self.SelectedResponsible.subscribe(self.ListView.filter);
        self.SelectedStatus.subscribe(self.ListView.filter);
        self.OnlyAssigned.subscribe(self.ListView.filter);

        self.ListView.search();
    };
};