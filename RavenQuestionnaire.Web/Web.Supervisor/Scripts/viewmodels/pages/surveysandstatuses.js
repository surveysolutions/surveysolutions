Supervisor.VM.SurveysAndStatuses = function (listViewUrl) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, arguments);
    
    var self = this;

    self.SelectedUser = ko.observable('');
    
    self.GetFilterMethod = function () {
        return { UserId: self.SelectedUser };
    };

    self.load = function () {
        self.SelectedUser(self.QueryString['interviewerId']);
        self.SelectedUser.subscribe(self.filter);
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);