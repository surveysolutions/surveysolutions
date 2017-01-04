declare var require: any

import { signalrPath } from "./../config"

require("jquery")

var $script = require("scriptjs");
$script(signalrPath, function() {
  // ... HERE is SignalR available to configuration
  // $.signalr.hub
  
});