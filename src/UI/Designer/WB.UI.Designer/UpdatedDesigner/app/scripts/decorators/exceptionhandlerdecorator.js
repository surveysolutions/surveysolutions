angular.module('designerApp')
  .config(function ($provide) {
      $provide.decorator('$exceptionHandler', function ($delegate, $injector) {
          return function (exception, cause) {
              var notificationService = $injector.get("notificationService");
              notificationService.error(exception);
              $delegate(exception, cause);
          };
      });
  });
