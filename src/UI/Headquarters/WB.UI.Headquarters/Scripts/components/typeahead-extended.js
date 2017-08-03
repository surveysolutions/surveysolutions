!function ($) {
    "use strict";
    var ExtendedTypeahead = {
        displayItemsCount: 0,
        loadMoreTemplate: "<div><strong><i>load more (+$moreItemsCount)</i></strong><div>",

        displayText: function(item) {
            if (!item) return "";

            if (item.loadMore) return item.name;

            return this.options.extendedDisplayText(item);
        },
        process: function (items, totalItemsCount) {
            var that = this;

            items = $.grep(items, function(item) {
                return that.matcher(item);
            });

            items = this.sorter(items);

            if (!items.length && !this.options.addItem) {
                return this.shown ? this.hide() : this;
            }

            if (items.length > 0 && this.displayItemsCount === 0) {
                this.$element.data('active', items[0]);
            } else {
                this.$element.data('active', null);
            }

            var itemsCount = _.isUndefined(totalItemsCount) ? items.length : totalItemsCount;

            this.displayItemsCount = this.displayItemsCount === 0 ? this.options.items : this.displayItemsCount;

            var itemsToRender = items.slice(0, this.displayItemsCount);
            // Add item
            if (this.options.addItem) {
                itemsToRender.insertAt(0, this.options.addItem);
            }

            if (this.options.items == 'all') {
                return this.render(items).show();
            } else {

                if (this.options.showLoadMore) {
                    if (itemsCount > this.displayItemsCount) {
                        var hiddenItemsCount = itemsCount - this.displayItemsCount;
                        var moreItemsCount = hiddenItemsCount > this.options.items ? this.options.items : hiddenItemsCount;
                        itemsToRender.push({ loadMore: true, name: this.loadMoreTemplate.replace("$moreItemsCount", moreItemsCount) });
                    } else {
                        this.displayItemsCount = this.options.items;
                    }
                }

                return this.render(itemsToRender).show();
            }
        },
        lookup: function (query) {
            var items;
            if (typeof (query) != 'undefined' && query !== null) {
                this.query = query;
            } else {
                this.query = this.$element.val() || '';
            }

            if (this.query.length < this.options.minLength) {
                return this.shown ? this.hide() : this;
            }

            var worker = $.proxy(function () {

                if ($.isFunction(this.source)) this.source(this.query, $.proxy(this.process, this), this.displayItemsCount === 0 ? this.options.items : this.displayItemsCount);
                else if (this.source) {
                    this.process(this.source);
                }
            }, this);

            clearTimeout(this.lookupWorker);
            this.lookupWorker = setTimeout(worker, this.delay);
        },
        render: function (items) {
            var that = this;
            var self = this;
            var activeFound = false;
            items = $(items).map(function (i, item) {
                var text = self.displayText(item);
                i = $(that.options.item).data('value', item);

                var linkContent = item.name;
                if (_.isUndefined(item.loadMore)) {
                    linkContent = that.highlighter(text);
                    var iconClass = item.iconClass || item.IconClass;
                    if (iconClass) {
                        linkContent = "<span class='" + iconClass + "'>" + linkContent + "</span>";
                    }
                }

                i.find('a').html(linkContent);
                if (text == self.$element.val()) {
                    i.addClass('active');
                    self.$element.data('active', item);
                    activeFound = true;
                }
                return i[0];
            });

            if (this.autoSelect && !activeFound) {
                items.first().addClass('active');
                this.$element.data('active', items.first().data('value'));
            }
            this.$menu.html(items);
            return this;
        },
        focus: function (e) {
            if (!this.focused) {
                this.focused = true;

                if (!this.mousedover && this.options.showHintOnFocus) {
                    this.lookup();
                }
            }
        },
        click: function(e) {
            e.stopPropagation();
            e.preventDefault();
            this.extendedSelect();
            this.$element.focus();
        },
        extendedSelect: function() {
            if (this.shouldLoadMoreItems()) {
                this.displayItemsCount += this.options.items;
                this.lookup();
            } else {
                this.select();
            }
        },
        keyup: function(e) {
            switch (e.keyCode) {
            case 40: // down arrow
            case 38: // up arrow
            case 16: // shift
            case 17: // ctrl
            case 18: // alt
                break;

            case 9: // tab
            case 13: // enter
                if (!this.shown) return;
                this.extendedSelect();
                break;

            case 27: // escape
                if (!this.shown) return;
                this.hide();
                break;
            default:
                this.lookup();
            }

            e.stopPropagation();
            e.preventDefault();
        },

        hide: function() {
            this.$menu.hide();
            this.shown = false;
            this.displayItemsCount = 0;
            return this;
        },

        show: function() {
            var pos = $.extend({}, this.$element.position(), {
                    height: this.$element[0].offsetHeight
                });

            var scrollHeight = typeof this.options.scrollHeight == 'function' ?
                this.options.scrollHeight.call() :
                this.options.scrollHeight;

            var shouldFitToParent = _.isUndefined(this.options.shouldFitToParent) ? true : this.options.shouldFitToParent;

            (this.$appendTo ? this.$menu.appendTo(this.$appendTo) : this.$menu.insertAfter(this.$element))
                .css({
                    top: pos.top + pos.height + scrollHeight,
                    left: pos.left,
                    width: shouldFitToParent ? this.$element.parent().width() : this.$element.width()
                })
                .show();

            this.$menu.scrollTop(this.$menu[0].scrollHeight);

            this.shown = true;
            return this;
        },

        shouldLoadMoreItems: function() {
            return this.$menu.find('.active').data('value').loadMore;
        },

        listen: function () {
            this.hacks();

            this.$element
              .on('focus', $.proxy(this.focus, this))
              .on('blur', $.proxy(this.blur, this))
              .on('keypress', $.proxy(this.keypress, this))
              .on('keyup', $.proxy(this.keyup, this));

            if (this.eventSupported('keydown')) {
                this.$element.on('keydown', $.proxy(this.keydown, this));
            }

            this.$menu
              .on('click', $.proxy(this.click, this))
              .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
              .on('mouseleave', 'li', $.proxy(this.mouseleave, this));
        },

        // here's where hacks get applied and we don't feel bad about it
        hacks: function () {
            var self = this;

            // if there's scrollable overflow, ie doesn't support
            // blur cancellations when the scrollbar is clicked
            //
            // #351: preventDefault won't cancel blurs in ie <= 8
            self.$element.on('blur', function($e) {
                var active = $(document.activeElement);
                var isActive = self.$menu.parent().is(active);

                var isMsie = function () {
                    var ua = window.navigator.userAgent;

                    var msie = ua.indexOf('MSIE ');
                    if (msie > 0) {
                        // IE 10 or older
                        return true;
                    }

                    var trident = ua.indexOf('Trident/');
                    if (trident > 0) {
                        // IE 11
                        return true;
                    }

                    var edge = ua.indexOf('Edge/');
                    if (edge > 0) {
                        // IE 12
                        return true;
                    }

                    // other browser
                    return false;
                };

                if (isMsie() && isActive) {
                    $e.preventDefault();
                    // stop immediate in order to prevent Input#_onBlur from
                    // getting exectued
                    $e.stopImmediatePropagation();
                    setTimeout(function () { self.$element.focus(); }, 0);
                }
            });

            // #351: prevents input blur due to clicks within menu
            this.$menu.on('mousedown', function($e) { $e.preventDefault(); });
        }
};
    $.extend($.fn.typeahead.Constructor.prototype, ExtendedTypeahead);
}(window.jQuery);