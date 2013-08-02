DocumentListViewModel = function (listViewUrl, deleteInterviewUrl, commandUrl, users) {
    var self = this;
    
    self.deleteInterviewUrl = deleteInterviewUrl;
    
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
        
        self.ListView.CheckForRequestComplete();
        
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
        
        self.ListView.IsAjaxComplete(false);
        
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
                        item.Status("Initial");
                    });
                }
                if (data.status == "error") {
                    
                }
                self.ListView.IsAjaxComplete(true);
            },
            error :function() {
                
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
                StatusId: self.SelectedStatus
            };
        };
        
        self.SelectedTemplate(location.queryString['templateid']);
        self.SelectedStatus(location.queryString['status']);

        self.SelectedTemplate.subscribe(self.ListView.filter);
        self.SelectedResponsible.subscribe(self.ListView.filter);
        self.SelectedStatus.subscribe(self.ListView.filter);

        self.ListView.search();
    };

    self.deleteInterview = function () {
        self.ListView.CheckForRequestComplete();

        var selectedRawInterviews = ko.utils.arrayFilter(self.ListView.Items(), function (item) {
            return item.IsSelected();
        });

        var request = { Interviews: [] };
        for (var i = 0; i < selectedRawInterviews.length; i++) {
            request.Interviews.push(selectedRawInterviews[i]["InterviewId"]());
        }

        self.ListView.IsAjaxComplete(false);

        $.post(self.deleteInterviewUrl, request, null, "json")
            .done(function (data) {
                var deletedInterviews = ko.utils.arrayFilter(selectedRawInterviews, function (item) {
                    return $.inArray(item["InterviewId"](), data["BlockedInterviews"]) == -1;
                });
                for (var i = 0; i < deletedInterviews.length; i++) {
                    self.ListView.Items.remove(deletedInterviews[i]);
                }
                self.ListView.TotalCount(self.ListView.TotalCount() - deletedInterviews.length);
                self.ListView.IsAjaxComplete(true);
            });
    };
};