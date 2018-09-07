import "./components";
import "./components/questions";
import "./components/questions/parts";
import "./directives";

import "jquery-mask-plugin";
import "./misc/htmlPoly.js";

import Vue from "vue";
import Idle from "./IdleTimeout";

Vue.component("IdleTimeoutService", Idle);
