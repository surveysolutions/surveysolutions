Supervisor.VM.SurveysAndStatuses = function (listViewUrl) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, arguments);
    self.Url = new Url(window.location.href);
    
    var self = this;

    self.SelectedUser = ko.observable('');
    
    self.GetFilterMethod = function () {
            
            self.Url.query['interviewerId'] = self.SelectedUser() || "";

            window.history.pushState({}, "interviewerId", self.Url.toString());
            
        return { UserId: self.SelectedUser };
    };

        self.SelectedUser(self.QueryString['interviewerId']);
        
        self.SelectedUser.subscribe(self.filter);
        self.Url.query['interviewerId'] = self.QueryString['interviewerId'] || "";
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);