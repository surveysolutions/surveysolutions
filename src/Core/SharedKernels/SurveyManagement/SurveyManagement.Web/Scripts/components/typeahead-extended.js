!function ($) {
    "use strict";
    var ExtendedTypeahead = {
        displayItemsCount: 0,

        displayText: function (item) {
            if (item.loadMore) return item.name;

            return this.options.extendedDisplayText(item);
        },
        process: function (items) {
            var that = this;

            items = $.grep(items, function (item) {
                return that.matcher(item);
            });

            items = this.sorter(items);

            if (!items.length && !this.options.addItem) {
                return this.shown ? this.hide() : this;
            }

            if (items.length > 0) {
                this.$element.data('active', items[0]);
            } else {
                this.$element.data('active', null);
            }

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
                    if (items.length > this.displayItemsCount) {
                        var hiddenItemsCount = items.length - this.displayItemsCount;
                        var moreItemsCount = hiddenItemsCount > this.options.items ? this.options.items : hiddenItemsCount;
                        itemsToRender.push({ loadMore: true, name: "[load more (+" + moreItemsCount + ")]" });
                    } else {
                        this.displayItemsCount = this.options.items;
                    }
                }

                return this.render(itemsToRender).show();
            }
        },
        focus: function (e) {
            this.focused = true;

            if (!this.mousedover && this.options.showHintOnFocus) {
                this.lookup();
            }
        },
        click: function (e) {
            e.stopPropagation();
            e.preventDefault();
            this.extendedSelect();
            this.$element.focus();
        },
        extendedSelect: function () {
            if (this.shouldLoadMoreItems()) {
                this.displayItemsCount += this.options.items;
                this.lookup();
            } else {
                this.select();
            }
        },
        keyup: function (e) {
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

        hide: function () {
            this.$menu.hide();
            this.shown = false;
            this.displayItemsCount = 0;
            return this;
        },

        show: function () {
            var pos = $.extend({}, this.$element.position(), {
                height: this.$element[0].offsetHeight
            }), scrollHeight;

            scrollHeight = typeof this.options.scrollHeight == 'function' ?
                this.options.scrollHeight.call() :
                this.options.scrollHeight;

            (this.$appendTo ? this.$menu.appendTo(this.$appendTo) : this.$menu.insertAfter(this.$element))
              .css({
                  top: pos.top + pos.height + scrollHeight,
                  left: pos.left,
                  width: this.$element.parent().width()
              })
              .show();

            this.shown = true;
            return this;
        },

        shouldLoadMoreItems: function () {
            return this.$menu.find('.active').data('value').loadMore;
        },
    };
    $.extend($.fn.typeahead.Constructor.prototype, ExtendedTypeahead);
}(window.jQuery);