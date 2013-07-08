UserListViewModel = function (lockUrl) {
    var self = this;

    self.LockUrl = lockUrl;

    self.lock = function () {
        var user = this;
        $.post(self.LockUrl, { UserId: user.UserId, IsLocked: user.IsLocked }, null, "json")
            .done(function(o) {
                user.IsLocked(!user.IsLocked());
            });
    };
};