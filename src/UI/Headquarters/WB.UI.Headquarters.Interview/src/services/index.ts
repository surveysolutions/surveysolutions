declare var require: any

import { signalrPath } from "./../config"

import * as jQuery from "jquery"
(window as any).$ = (window as any).jQuery = jQuery

import "signalr";

import * as $script from "scriptjs"

$script(signalrPath, function () {
    // ... HERE is SignalR available to configuration
    // $.signalr.hub
    // jQuery.connection.hub.start().done(function () {
    // }).fail((e) => {
    //     console.log(e);
    // });
});
