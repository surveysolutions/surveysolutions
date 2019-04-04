(function () {
    angular.module('designerApp').factory('userService', 
        function ($http) {
            var urlBase = '../../api/users';
            var users = {};

            users.getCurrentUserName = function() {
                var url = urlBase + '/CurrentLogin';
                return $http.get(url);
            };
            return users;
        }
    );
})();