declare var require: any

import { signalrPath } from "./../config"

import * as jQuery from "jquery"
(window as any).$ = (window as any).jQuery = jQuery

import "ms-signalr-client";

import * as $script from "scriptjs"

$script(signalrPath, function() {
  // ... HERE is SignalR available to configuration
  // $.signalr.hub
});
