Supervisor.VM.SurveysAndStatuses = function (listViewUrl, responsibles) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);

    self.Responsibles = responsibles;
    self.SelectedResponsible = ko.observable();

    self.GetFilterMethod = function() {
        self.Url.query['responsible'] = _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName;
        if (Modernizr.history) {
            window.history.pushState({}, "responsible", self.Url.toString());
        }

        return { ResponsibleName: self.Url.query['responsible'] };
    };
    self.load = function() {

        if (self.QueryString['responsible']) {
            self.SelectedResponsible({ UserName: self.QueryString['responsible'] });
        }
        self.SelectedResponsible.subscribe(self.filter);
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);