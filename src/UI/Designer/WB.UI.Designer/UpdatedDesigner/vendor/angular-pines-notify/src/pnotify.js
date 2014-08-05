angular.module('ui.notify', []).
  provider('notificationService', [ function() {

    var settings = {
      styling: 'bootstrap3' // or 'jqueryui' or 'bootstrap2'
    };

    this.setDefaults = function(defaults) { settings = defaults };

    this.$get = [ function() {

      return {

        /* ========== SETTINGS RELATED METHODS =============*/

        getSettings: function() {
          return settings;
        },

        /* ============== NOTIFICATION METHODS ==============*/

        notice: function(content) {
          var hash = angular.copy(settings);
          hash.type = 'notice';
          hash.text = content;
          return this.notify(hash);
        },

        info: function(content) {
          var hash = angular.copy(settings);
          hash.type = 'info';
          hash.text = content;
          return this.notify(hash);
        },

        success: function(content) {
          var hash = angular.copy(settings);
          hash.type = 'success';
          hash.text = content;
          return this.notify(hash);
        },

        error: function(content) {
          var hash = angular.copy(settings);
          hash.type = 'error';
          hash.text = content;
          return this.notify(hash);
        },

        notify: function(hash) {
          return new PNotify(hash);
        }

      };

    }];

  }]);
