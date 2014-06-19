(function() {
    angular.module('designerApp')
        .factory('errorReportingInterceptor', [
            function () {
                return {
                    response: function (response) {
                        if (response.data.IsSuccess === false) {
                            alert(response.data.Error);
                        }
                        return response;
                    }
            }
        }]);
})();