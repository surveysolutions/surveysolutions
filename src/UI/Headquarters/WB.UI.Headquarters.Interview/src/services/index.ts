declare var require: any

import { signalrPath } from "./../config"

(window as any).$ = (window as any).jQuery = require("jquery");

require('ms-signalr-client');

var $script = require("scriptjs");
$script(signalrPath, function() {
  // ... HERE is SignalR available to configuration
  // $.signalr.hub
});
