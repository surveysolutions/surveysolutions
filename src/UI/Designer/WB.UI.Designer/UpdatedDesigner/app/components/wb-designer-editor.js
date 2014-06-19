//(function() {
//  jQuery(function() {
//    return window.DesignerEditorController.get();
//  });

//  this.DesignerEditorController = (function() {
//    var PrivateClass, instance;

//    function DesignerEditorController() {}

//    instance = null;

//    DesignerEditorController.get = function() {
//      return instance != null ? instance : instance = new PrivateClass;
//    };

//    PrivateClass = (function() {
//      function PrivateClass() {
//        if ($('#designer-editor.container').length > 0) {
//          this.init();
//          console.log('init control panel');
//        }
//      }

//      PrivateClass.prototype.init = function() {
//        this.root = $('#designer-editor.container');
//        console.log(this.root);
//        this.init_chapter_panel();
//        return this.init_scrollbars();
//      };

//      PrivateClass.prototype.init_chapter_panel = function() {
//          var chapter_panel;
//          console.log('init_chapter_panel');
//          chapter_panel = this.root.find('.chapter-panel');
//          console.log(chapter_panel);
//          chapter_panel.click(
//              function () {
//                console.log('unfolded111');
//            if (!chapter_panel.hasClass('unfolded')) {
//                return chapter_panel.addClass('unfolded');
//            }
//          }
//        );
//        return chapter_panel.find('.foldback-button').click((function(_this) {
//          return function() {
//            return _this.fold_chapter_panel();
//          };
//        })(this));
//      };

//      PrivateClass.prototype.unfold_chapter_panel = function() {
//        console.log('unfolded');
//        return this.root.find('.chapter-panel').addClass('unfolded');
//      };

//      PrivateClass.prototype.fold_chapter_panel = function() {
//        console.log('fold');
//        console.log(this.root.find('.chapter-panel'));
//        this.root.find('.chapter-panel').removeClass('unfolded');
//        return false;
//      };

//      PrivateClass.prototype.init_scrollbars = function() {
//        console.log(this.root.find('.question-list'));
//        return this.root.find('.question-list').mCustomScrollbar({
//          scrollButtons: {
//            enable: false
//          },
//          theme: "wb-dark-thick",
//          scrollInertia: 0,
//          mouseWheelPixels: 30
//        });
//      };

//      return PrivateClass;

//    })();

//    return DesignerEditorController;

//  })();

//}).call(this);
