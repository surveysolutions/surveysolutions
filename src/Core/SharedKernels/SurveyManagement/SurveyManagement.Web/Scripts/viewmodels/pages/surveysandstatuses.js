Supervisor.VM.SurveysAndStatuses = function (listViewUrl, responsibles) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);

    self.Responsibles = responsibles;
    self.SelectedResponsible = ko.observable();

    self.GetFilterMethod = function() {
        self.Url.query['interviewerId'] = _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserId;
        if (Modernizr.history) {
            window.history.pushState({}, "interviewerId", self.Url.toString());
        }

        return { UserId: self.Url.query['interviewerId'] };
    };
    self.load = function() {
        var selectedResponsible = _.find(self.Responsibles, function (responsible) { return responsible.UserId == self.QueryString['interviewerId'] });
        self.SelectedResponsible(selectedResponsible);
        self.SelectedResponsible.subscribe(self.filter);
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);