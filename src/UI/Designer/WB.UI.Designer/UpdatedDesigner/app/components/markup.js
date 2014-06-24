(function() {
  jQuery(function() {
    return window.DesignerEditorController.get();
  });

  this.DesignerEditorController = (function() {
    var PrivateClass, instance;

    function DesignerEditorController() {}

    instance = null;

    DesignerEditorController.get = function() {
      return instance != null ? instance : instance = new PrivateClass;
    };

    PrivateClass = (function() {
      function PrivateClass() {
        if ($('#designer-editor.container').length > 0) {
          this.init();
        }
      }

      PrivateClass.prototype.init = function() {
        this.root = $('#designer-editor.container');
        this.init_chapter_panel();
        console.log('test');
        this.init_checkboxes();
        return this.init_scrollbars();
      };

      PrivateClass.prototype.init_chapter_panel = function() {
        console.log('test2');
        var chapter_panel;
        chapter_panel = this.root.find('.chapter-panel');
        chapter_panel.click((function(_this) {
          return function() {
            if (!chapter_panel.hasClass('unfolded')) {
              return _this.unfold_chapter_panel();
            }
          };
        })(this));
        return chapter_panel.find('.foldback-button').click((function(_this) {
          return function() {
            return _this.fold_chapter_panel();
          };
        })(this));
      };

      PrivateClass.prototype.unfold_chapter_panel = function() {
        return this.root.find('.chapter-panel').addClass('unfolded');
        console.log('test3');
      };

      PrivateClass.prototype.fold_chapter_panel = function() {
        console.log('fold');
        console.log(this.root.find('.chapter-panel'));
        this.root.find('.chapter-panel').removeClass('unfolded');
        return false;
      };

      PrivateClass.prototype.init_scrollbars = function() {
        return this.root.find('.question-list').mCustomScrollbar({
          scrollButtons: {
            enable: false
          },
          theme: "wb-dark-thick",
          scrollInertia: 0,
          mouseWheelPixels: 30
        });
      };

      PrivateClass.prototype.init_checkboxes = function() {
          return $.each($('input.wb-checkbox'), function(i, el) {
              return $(el).prettyCheckable();
          });
      };

      return PrivateClass;

    })();

    return DesignerEditorController;

  })();

}).call(this);
(function() {


}).call(this);
