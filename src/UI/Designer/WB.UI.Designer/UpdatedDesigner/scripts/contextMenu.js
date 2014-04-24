(function () {
    jQuery(function () {
        return window.ContextMenuController.get();
    });

    this.ContextMenuController = (function () {
        var PrivateClass, instance;

        function ContextMenuController() { }

        instance = null;

        ContextMenuController.get = function () {
            return instance != null ? instance : instance = new PrivateClass;
        };

        PrivateClass = (function () {
            function PrivateClass() {
                if ($('*[data-action="context-menu"]').length > 0) {
                    this.init();
                }
            }

            PrivateClass.prototype.init = function () {
                this.opened_menu = null;
                $.each($('*[data-action="context-menu"]'), (function (_this) {
                    return function (index, el) {
                        return _this.bind(el);
                    };
                })(this));
                return $('body').click((function (_this) {
                    return function () {
                        return _this.hide_active_context_menu();
                    };
                })(this));
            };

            PrivateClass.prototype.bind = function (el) {
                return $(el).click((function (_this) {
                    return function (event) {
                        return _this.show_context_menu(event);
                    };
                })(this));
            };

            PrivateClass.prototype.show_context_menu = function (event) {
                var btn, menu;
                event.stopPropagation();
                btn = $(event.target);
                menu = $('#' + btn.data('target')).clone().appendTo('body');
                if (menu.length > 0) {
                    menu.css({
                        left: btn.offset().left,
                        top: btn.offset().top + btn.height() + 10
                    });
                    menu.stop().fadeIn();
                    this.hide_active_context_menu();
                    return this.opened_menu = menu;
                }
            };

            PrivateClass.prototype.hide_context_menu = function (event) {
                var btn, menu;
                btn = $(event.target);
                menu = $('#' + btn.data('target'));
                return menu.stop().fadeOut();
            };

            PrivateClass.prototype.hide_active_context_menu = function () {
                var om;
                if (this.opened_menu) {
                    om = this.opened_menu;
                    om.stop().fadeOut(function () {
                        return om.remove();
                    });
                    return this.opened_menu = null;
                }
            };

            return PrivateClass;

        })();

        return ContextMenuController;

    })();

}).call(this);