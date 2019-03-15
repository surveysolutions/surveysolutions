Supervisor.VM.ControlPanel.SystemLog = function (dataUrl, systemLogUrl) {
    Supervisor.VM.ControlPanel.SystemLog.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.Url = new Url(systemLogUrl);

    self.GetFilterMethod = function () {
        
        if (Modernizr.history) {
            window.history.pushState({}, "Syslog", self.Url.toString());
        }

        return {};
    };

    self.load = function () {
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.SystemLog, Supervisor.VM.ListView);
