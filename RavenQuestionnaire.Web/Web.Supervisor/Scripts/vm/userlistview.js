UserListViewModel = function (lockUrl, listViewUrl) {
    var self = this;

    self.LockUrl = lockUrl;
    self.ListView = new ListViewModel(listViewUrl);

    self.lock = function () {
        var user = this;
        $.post(self.LockUrl, { UserId: user.UserId, IsLocked: user.IsLocked }, null, "json")
            .done(function(o) {
                user.IsLocked(!user.IsLocked());
            });
    };

    self.load = function() {
        self.ListView.search();
    };
};