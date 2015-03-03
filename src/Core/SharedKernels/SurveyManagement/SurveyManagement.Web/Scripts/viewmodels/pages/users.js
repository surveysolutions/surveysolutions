Supervisor.VM.Users = function (listViewUrl) {
    Supervisor.VM.Users.superclass.constructor.apply(this, arguments);
    
    var self = this;

    self.load = function() {
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Users, Supervisor.VM.ListView);