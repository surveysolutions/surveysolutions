angular.module('designerApp')
  .directive('treeItem', function () {
    return {
      templateUrl: function(element, attr) {
  		return 'tree' + attr.itemType;
      },
      restrict: 'E'
    };
  });
