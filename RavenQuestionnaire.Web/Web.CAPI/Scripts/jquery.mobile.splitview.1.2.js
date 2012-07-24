// TODO: reinstate replaceBackBtn - to include the case where people actually really want the back btn
(function ($, window, undefined) {
    $(window.document).bind('mobileinit', function () {
        //some class for css to detect touchscreens
        if ($.support.touch) {
            $('html').addClass('touch');
        }
        var $query = $.mobile.media('screen and (min-width: 480px)') && ($.mobile.media('(-webkit-max-device-pixel-ratio: 1.2)') || $.mobile.media('(max--moz-device-pixel-ratio: 1.2)'));
        $.support.splitview = ($query || ($.mobile.browser.ie && $(this).width() >= 480)) && $.mobile.ajaxEnabled;
        if ($.support.splitview) {
            $('html').addClass('splitview');

            //on window.ready() execution:
            $(function () {

                $('div:jqmData(role="panel")').addClass('ui-mobile-viewport ui-panel');

                var $pages = $(":jqmData(role='page'), :jqmData(role='dialog')");

                // if no pages are found, create one with body's inner html
                if (!$pages.length) {
                    $pages = $("body").wrapInner("<div data-" + $.mobile.ns + "role='page'></div>").children(0);
                }

                // add dialogs, set data-url attrs
                $pages.each(function () {
                    var $this = $(this);

                    // unless the data url is already set set it to the pathname
                    if (!$this.jqmData("url")) {
                        $this.attr("data-" + $.mobile.ns + "url", $this.attr("id") || location.pathname + location.search);
                    }
                });

                // define first page in dom case one backs out to the directory root (not always the first page visited, but defined as fallback)
                $.mobile.firstPage = $pages.first();

                // define page container
                $.mobile.pageContainer = $pages.first().parent().addClass("ui-mobile-viewport");

                // alert listeners that the pagecontainer has been determined for binding
                // to events triggered on it
                $window.trigger("pagecontainercreate");

                // cue page loading message
                $.mobile.showPageLoadingMsg();


                //$(document).unbind('.toolbar');
                //$('.ui-page').die('.toolbar');

                var firstPageMain = $('div:jqmData(id="main") > div:jqmData(role="page"):first');
                if (!$.mobile.hashListeningEnabled || !$.mobile.path.stripHash(location.hash)) {
                    var $container = $('div:jqmData(id="main")');
                    $.mobile.firstPage = firstPageMain;
                    $.mobile.pageContainer = firstPageMain.parents('.ui-mobile-viewport');
                    $.mobile.changePage(firstPageMain, { transition: 'none', changeHash: false, pageContainer: $container });
                    $.mobile.activePage = undefined;
                } //no need to trigger a hashchange here cause the other page is handled by core.

                // setup the layout for splitview and jquerymobile will handle first page init
                $(window).trigger('orientationchange');
                setTimeout(function () {
                    $.mobile.firstPage = firstPageMain;
                }, 100);
            }); //end window.ready()

            //----------------------------------------------------------------------------------
            //Main event bindings: click, form submits, hashchange and orientationchange/resize(popover)
            //----------------------------------------------------------------------------------
            //existing base tag?
            var $window = $(window),
          $html = $('html'),
          $head = $('head'),
          $base = $head.children("base"),
            //tuck away the original document URL minus any fragment.
          documentUrl = $.mobile.path.parseUrl(location.href),

            //if the document has an embedded base tag, documentBase is set to its
            //initial value. If a base tag does not exist, then we default to the documentUrl.
          documentBase = $base.length ? $.mobile.path.parseUrl($.mobile.path.makeUrlAbsolute($base.attr("href"), documentUrl.href)) : documentUrl;

            function findClosestLink(ele) {
                while (ele) {
                    if (ele.nodeName.toLowerCase() == "a") {
                        break;
                    }
                    ele = ele.parentNode;
                }
                return ele;
            }

            // The base URL for any given element depends on the page it resides in.
            function getClosestBaseUrl(ele) {
                // Find the closest page and extract out its url.
                var url = $(ele).closest(".ui-page").jqmData("url"),
          base = documentBase.hrefNoHash;

                if (!url || !$.mobile.path.isPath(url)) {
                    url = base;
                }

                return $.mobile.path.makeUrlAbsolute(url, base);
            }

            //simply set the active page's minimum height to screen height, depending on orientation
            function getScreenHeight() {
                var orientation = jQuery.event.special.orientationchange.orientation(),
          port = orientation === "portrait",
          winMin = port ? 480 : 320,
          screenHeight = port ? screen.availHeight : screen.availWidth,
          winHeight = Math.max(winMin, $(window).height()),
          pageMin = Math.min(screenHeight, winHeight);

                return pageMin;
            }

            function newResetActivePageHeight() {
                var page = $("." + $.mobile.activePageClass);
                page.each(function () {
                    if ($(this).closest(".panel-popover").length != 1) {
                        //$(this).css("min-height", getScreenHeight());
                    }
                    else {
                        $(this).css("min-height", "100%")
                    }
                });

            }

            //The following event bindings should be bound after mobileinit has been triggered
            //the following deferred is resolved in the init file
            $.mobile.navreadyDeferred = $.Deferred();
            $.mobile.navreadyDeferred.done(function () {
                //bind to form submit events, handle with Ajax
                $(document).delegate("form", "submit", function (event) {
                    var $this = $(this);

                    if (!$.mobile.ajaxEnabled ||
                    // test that the form is, itself, ajax false
					$this.is(":jqmData(ajax='false')") ||
                    // test that $.mobile.ignoreContentEnabled is set and
                    // the form or one of it's parents is ajax=false
					!$this.jqmHijackable().length) {
                        return;
                    }

                    var type = $this.attr("method"),
				    target = $this.attr("target"),
				    url = $this.attr("action");

                    // If no action is specified, browsers default to using the
                    // URL of the document containing the form. Since we dynamically
                    // pull in pages from external documents, the form should submit
                    // to the URL for the source document of the page containing
                    // the form.
                    if (!url) {
                        // Get the @data-url for the page containing the form.
                        url = getClosestBaseUrl($this);
                        if (url === documentBase.hrefNoHash) {
                            // The url we got back matches the document base,
                            // which means the page must be an internal/embedded page,
                            // so default to using the actual document url as a browser
                            // would.
                            url = documentUrl.hrefNoSearch;
                        }
                    }

                    url = $.mobile.path.makeUrlAbsolute(url, getClosestBaseUrl($this));

                    if (($.mobile.path.isExternal(url) && !$.mobile.path.isPermittedCrossDomainRequest(documentUrl, url)) || target) {
                        return;
                    }

                    $.mobile.changePage(
				url,
				{
				    type: type && type.length && type.toLowerCase() || "get",
				    data: $this.serialize(),
				    transition: $this.jqmData("transition"),
				    reverse: $this.jqmData("direction") === "reverse",
				    reloadPage: true
				}
			);
                    event.preventDefault();
                });

                //add active state on vclick
                $(document).bind("vclick", function (event) {
                    // if this isn't a left click we don't care. Its important to note
                    // that when the virtual event is generated it will create the which attr
                    if (event.which > 1 || !$.mobile.linkBindingEnabled) {
                        return;
                    }

                    var link = findClosestLink(event.target);

                    // split from the previous return logic to avoid find closest where possible
                    // TODO teach $.mobile.hijackable to operate on raw dom elements so the link wrapping
                    // can be avoided
                    if (!$(link).jqmHijackable().length) {
                        return;
                    }

                    if (link) {
                        if ($.mobile.path.parseUrl(link.getAttribute("href") || "#").hash !== "#") {
                            //removeActiveLinkClass(true);
                            //$activeClickedLink = $(link).closest(".ui-btn").not(".ui-disabled");
                            //$activeClickedLink.addClass($.mobile.activeBtnClass);

                            $(link).closest(".ui-btn").not(".ui-disabled").addClass($.mobile.activeBtnClass);
                            $("." + $.mobile.activePageClass + " .ui-btn").not(link).blur();
                        }
                    }
                });

                // click routing - direct to HTTP or Ajax, accordingly
                $(document).bind("click", function (event) {
                    if (!$.mobile.linkBindingEnabled) {
                        return;
                    }

                    var link = findClosestLink(event.target), $link = $(link), httpCleanup;

                    // If there is no link associated with the click or its not a left
                    // click we want to ignore the click
                    // TODO teach $.mobile.hijackable to operate on raw dom elements so the link wrapping
                    // can be avoided
                    if (!link || event.which > 1 || !$link.jqmHijackable().length) {
                        return;
                    }

                    //remove active link class if external (then it won't be there if you come back)
                    httpCleanup = function () {
                        window.setTimeout(function () { /*removeActiveLinkClass(true);*/ }, 200);
                    };

                    //if there's a data-rel=back attr, go back in history
                    if ($link.is(":jqmData(rel='back')")) {
                        window.history.back();
                        return false;
                    }

                    var baseUrl = getClosestBaseUrl($link),

                    //get href, if defined, otherwise default to empty hash
				href = $.mobile.path.makeUrlAbsolute($link.attr("href") || "#", baseUrl);

                    //if ajax is disabled, exit early
                    if (!$.mobile.ajaxEnabled && !$.mobile.path.isEmbeddedPage(href)) {
                        httpCleanup();
                        //use default click handling
                        return;
                    }

                    // XXX_jblas: Ideally links to application pages should be specified as
                    //            an url to the application document with a hash that is either
                    //            the site relative path or id to the page. But some of the
                    //            internal code that dynamically generates sub-pages for nested
                    //            lists and select dialogs, just write a hash in the link they
                    //            create. This means the actual URL path is based on whatever
                    //            the current value of the base tag is at the time this code
                    //            is called. For now we are just assuming that any url with a
                    //            hash in it is an application page reference.
                    if (href.search("#") !== -1) {
                        href = href.replace(/[^#]*#/, "");
                        if (!href) {
                            //link was an empty hash meant purely
                            //for interaction, so we ignore it.
                            event.preventDefault();
                            return;
                        } else if ($.mobile.path.isPath(href)) {
                            //we have apath so make it the href we want to load.
                            href = $.mobile.path.makeUrlAbsolute(href, baseUrl);
                        } else {
                            //we have a simple id so use the documentUrl as its base.
                            href = $.mobile.path.makeUrlAbsolute("#" + href, documentUrl.hrefNoHash);
                        }
                    }

                    // Should we handle this link, or let the browser deal with it?
                    var useDefaultUrlHandling = $link.is("[rel='external']") || $link.is(":jqmData(ajax='false')") || $link.is("[target]"),

                    // Some embedded browsers, like the web view in Phone Gap, allow cross-domain XHR
                    // requests if the document doing the request was loaded via the file:// protocol.
                    // This is usually to allow the application to "phone home" and fetch app specific
                    // data. We normally let the browser handle external/cross-domain urls, but if the
                    // allowCrossDomainPages option is true, we will allow cross-domain http/https
                    // requests to go through our page loading logic.

                    //check for protocol or rel and its not an embedded page
                    //TODO overlap in logic from isExternal, rel=external check should be
                    //     moved into more comprehensive isExternalLink
				isExternal = useDefaultUrlHandling || ($.mobile.path.isExternal(href) && !$.mobile.path.isPermittedCrossDomainRequest(documentUrl, href));

                    var isRefresh = $link.jqmData('refresh'),
                  $targetPanel = $link.jqmData('panel'),
                  $targetContainer = $('div:jqmData(id="' + $targetPanel + '")'),
                  $targetPanelActivePage = $targetContainer.children('div.' + $.mobile.activePageClass),
                  $currPanel = $link.parents('div:jqmData(role="panel")'),
                    //not sure we need this. if you want the container of the element that triggered this event, $currPanel 
                  $currContainer = $.mobile.pageContainer,
                  $currPanelActivePage = $currPanel.children('div.' + $.mobile.activePageClass),
                  url = $.mobile.path.stripHash($link.attr("href")),
                  from = null;

                    //still need this hack apparently:
                    $('a.nav-link.ui-btn.' + $.mobile.activeBtnClass).removeClass($.mobile.activeBtnClass);
                    $('a.nav-link').parents('li').removeClass($.mobile.activeBtnClass);
                    $activeClickedLink = $link.closest(".ui-btn").addClass($.mobile.activeBtnClass);


                    if (isExternal) {
                        httpCleanup();
                        //use default click handling
                        return;
                    }

                    //use ajax
                    var transition = $link.jqmData("transition"),
                         direction = $link.jqmData("direction"),
				           reverse = $link.jqmData("direction") === "reverse" ||
                    // deprecated - remove by 1.0
							$link.jqmData("back"),

                    //this may need to be more specific as we use data-rel more
				role = $link.attr("data-" + $.mobile.ns + "rel") || undefined;
                    var hash = $currPanel.jqmData('hash');

                    if (role === "popup") {
                        $.mobile.activePage = $currPanelActivePage;
                        //$.mobile.pageContainer = $currPanelActivePage;
                        $.mobile.popup.handleLink($link);
                    }
                    else {
                        if ($targetPanelActivePage.attr('data-url') == url || $currPanelActivePage.attr('data-url') == url) {
                            if (isRefresh) { //then changePage below because it's a pageRefresh request
                                $.mobile.changePage(href, { role: role, fromPage: from, transition: 'fade', reverse: reverse, changeHash: false, pageContainer: $targetContainer, reloadPage: isRefresh });
                            }
                            else { //else preventDefault and return
                                event.preventDefault();
                                return;
                            }
                        }
                        else if ($targetPanel && $targetPanel != $link.parents('div:jqmData(role="panel")')) {
                            var from = $targetPanelActivePage;
                            $.mobile.pageContainer = $targetContainer;
                            $.mobile.changePage(href, { role: role, fromPage: from, transition: transition, reverse: reverse, pageContainer: $targetContainer });
                        }
                        else {
                            var from = $currPanelActivePage;
                            $.mobile.pageContainer = $currPanel;
                            var hashChange = (hash == 'false' || hash == 'crumbs') ? false : true;
                            $.mobile.changePage(href, { role: role, fromPage: from, transition: transition, reverse: reverse, changeHash: hashChange, pageContainer: $currPanel });
                            //active page must always point to the active page in main - for history purposes.
                            $.mobile.activePage = $('div:jqmData(id="main") > div.' + $.mobile.activePageClass);
                        }
                        //$.mobile.changePage(href, { transition: transition, reverse: reverse, role: role });
                    }
                    event.preventDefault();
                });

                //prefetch pages when anchors with data-prefetch are encountered
                $(document).delegate(".ui-page", "pageshow.prefetch", function () {
                    var urls = [];
                    var $thisPageContainer = $(this).parents('div:jqmData(role="panel")');
                    $(this).find("a:jqmData(prefetch)").each(function () {
                        var $link = $(this),
                            panel = $(this).jqmData('panel'),
                            container = panel.length ? $('div:jqmData(id="' + panel + '")') : $thisPageContainer,
					        url = $link.attr("href");

                        if (url && $.inArray(url, urls) === -1) {
                            urls.push(url);

                            $.mobile.loadPage(url, { role: $link.attr("data-" + $.mobile.ns + "rel"), pageContainer: container });
                        }
                    });
                });

                $.mobile._handleHashChange = function (hash) {
                    //find first page via hash
                    var to = $.mobile.path.stripHash(hash),
                    //transition is false if it's the first page, undefined otherwise (and may be overridden by default)
				transition = $.mobile.urlHistory.stack.length === 0 ? "none" : undefined,

                    // "navigate" event fired to allow others to take advantage of the more robust hashchange handling
				navEvent = new $.Event("navigate"),

                    // default options for the changPage calls made after examining the current state
                    // of the page and the hash
				changePageOptions = {
				    transition: transition,
				    changeHash: false,
				    fromHashChange: true
				};

                    if (0 === $.mobile.urlHistory.stack.length) {
                        $.mobile.urlHistory.initialDst = to;
                    }

                    // We should probably fire the "navigate" event from those places that make calls to _handleHashChange,
                    // and have _handleHashChange hook into the "navigate" event instead of triggering it here
                    $.mobile.pageContainer.trigger(navEvent);
                    if (navEvent.isDefaultPrevented()) {
                        return;
                    }

                    //if listening is disabled (either globally or temporarily), or it's a dialog hash
                    if (!$.mobile.hashListeningEnabled || $.mobile.urlHistory.ignoreNextHashChange) {
                        $.mobile.urlHistory.ignoreNextHashChange = false;
                        return;
                    }

                    // special case for dialogs
                    if ($.mobile.urlHistory.stack.length > 1 && to.indexOf($.mobile.dialogHashKey) > -1 && $.mobile.urlHistory.initialDst !== to) {

                        // If current active page is not a dialog skip the dialog and continue
                        // in the same direction
                        if (!$.mobile.activePage.is(".ui-dialog")) {
                            //determine if we're heading forward or backward and continue accordingly past
                            //the current dialog
                            $.mobile.urlHistory.directHashChange({
                                currentUrl: to,
                                isBack: function () { window.history.back(); },
                                isForward: function () { window.history.forward(); }
                            });

                            // prevent changePage()
                            return;
                        } else {
                            // if the current active page is a dialog and we're navigating
                            // to a dialog use the dialog objected saved in the stack
                            $.mobile.urlHistory.directHashChange({
                                currentUrl: to,

                                // regardless of the direction of the history change
                                // do the following
                                either: function (isBack) {
                                    var active = $.mobile.urlHistory.getActive();

                                    to = active.pageUrl;

                                    // make sure to set the role, transition and reversal
                                    // as most of this is lost by the domCache cleaning
                                    $.extend(changePageOptions, {
                                        role: active.role,
                                        transition: active.transition,
                                        reverse: isBack
                                    });
                                }
                            });
                        }
                    }

                    //if to is defined, load it
                    if (to) {
                        // At this point, 'to' can be one of 3 things, a cached page element from
                        // a history stack entry, an id, or site-relative/absolute URL. If 'to' is
                        // an id, we need to resolve it against the documentBase, not the location.href,
                        // since the hashchange could've been the result of a forward/backward navigation
                        // that crosses from an external page/dialog to an internal page/dialog.
                        to = (typeof to === "string" && !$.mobile.path.isPath(to)) ? ($.mobile.path.makeUrlAbsolute('#' + to, documentBase)) : to;
                        $.mobile.changePage(to, changePageOptions);
                    } else {
                        //there's no hash, go to the first page in the dom
                        $.mobile.changePage($.mobile.firstPage, changePageOptions);
                    }
                };

                //hashchange event handler
                $window.bind("hashchange", function (e, triggered) {
                    $.mobile._handleHashChange(location.hash);
                });

                //set page min-heights to be device specific
                $(document).bind("pageshow", newResetActivePageHeight);
                $(window).bind("throttledresize", newResetActivePageHeight);

            }); //navreadyDeferred done callback



            //DONE: bind orientationchange and resize - the popover
            _orientationHandler = function (event) {
                var $menu = $('div:jqmData(id="menu")'),
            $main = $('div:jqmData(id="main")'),
            $mainHeader = $main.find('div.' + $.mobile.activePageClass + '> div:jqmData(role="header")'),
            $window = $(window);

                function popoverBtn(header) {
                    if (!header.children('.popover-btn').length) {
                        if (header.children('a.ui-btn-left').length) {
                            header.children('a.ui-btn-left').replaceWith('<a class="popover-btn">Menu</a>');
                            header.children('a.popover-btn').addClass('ui-btn-left').buttonMarkup();
                        }
                        else {
                            header.prepend('<a class="popover-btn">Menu</a>');
                            header.children('a.popover-btn').addClass('ui-btn-left').buttonMarkup()
                        }
                    }
                }

                function replaceBackBtn(header) {
                    if ($.mobile.urlHistory.stack.length > 1 && !header.children('a:jqmData(rel="back")').length && header.jqmData('backbtn') != false) {
                        header.prepend("<a href='#' class='ui-btn-left' data-" + $.mobile.ns + "rel='back' data-" + $.mobile.ns + "icon='arrow-l'>Back</a>");
                        header.children('a:jqmData(rel="back")').buttonMarkup();
                    }
                };

                function popover() {
                    $menu.addClass('panel-popover')
               .removeClass('ui-panel-left')
               .css({ 'width': '25%', 'min-width': '250px', 'display': '', 'overflow-x': 'visible' });
                    if (!$menu.children('.popover_triangle').length) {
                        $menu.prepend('<div class="popover_triangle"></div>');
                    }
                    $menu.children('.' + $.activePageClass).css('min-height', '100%');
                    $main.removeClass('ui-panel-right')
               .css('width', '');
                    popoverBtn($mainHeader);

                    $main.undelegate('div:jqmData(role="page")', 'pagebeforeshow.splitview');
                    $main.delegate('div:jqmData(role="page")', 'pagebeforeshow.popover', function () {
                        var $thisHeader = $(this).children('div:jqmData(role="header")');
                        popoverBtn($thisHeader);
                    });
                    // TODO: unbind resetActivePageHeight for popover pages

                };

                function splitView() {
                    $menu.removeClass('panel-popover')
               .addClass('ui-panel-left')
               .css({ 'width': '25%', 'min-width': '250px', 'display': '' });
                    $menu.children('.popover_triangle').remove();
                    $main.addClass('ui-panel-right')
               .width(function () {
                   return $(window).width() - $('div:jqmData(id="menu")').width();
               });
                    $mainHeader.children('.popover-btn').remove();

                    // replaceBackBtn($mainHeader);

                    $main.undelegate('div:jqmData(role="page")', 'pagebeforeshow.popover');
                    $main.delegate('div:jqmData(role="page")', 'pagebeforeshow.splitview', function () {
                        var $thisHeader = $(this).children('div:jqmData(role="header")');
                        $thisHeader.children('.popover-btn').remove();
                        // replaceBackBtn($thisHeader);
                    });

                }

                if (event.orientation) {
                    if (event.orientation == 'portrait') {
                        popover();
                    }
                    else if (event.orientation == 'landscape') {
                        splitView();
                    }
                }
                else if ($window.width() < 768 && $window.width() > 480) {
                    popover();
                }
                else if ($window.width() > 768) {
                    splitView();
                }
            };

            $(window).bind('orientationchange', _orientationHandler);
            $(window).bind('throttledresize', _orientationHandler);

            //popover button click handler - from http://www.cagintranet.com/archive/create-an-ipad-like-dropdown-popover/
            $('.popover-btn').live('click', function (e) {
                e.preventDefault();
                $('.panel-popover').fadeToggle('fast');
                if ($('.popover-btn').hasClass($.mobile.activeBtnClass)) {
                    $('.popover-btn').removeClass($.mobile.activeBtnClass);
                } else {
                    $('.popover-btn').addClass($.mobile.activeBtnClass);
                }
            });

            if ($.fn.controlgroup) {
                $(document).bind("pagecreate create", function (e) {
                    $(":jqmData(role='controlgroup')", e.target)
					.jqmEnhanceable()
					.controlgroup({ excludeInvisible: false });
                });
            }
        }
        else {
            //removes all panels so the page behaves like a single panel jqm
            $(function () {
                $('div:jqmData(role="panel")').each(function() {
                    var $this = $(this);
                    $this.replaceWith($this.html());
                });
            });
        }
    });
})(jQuery, window);