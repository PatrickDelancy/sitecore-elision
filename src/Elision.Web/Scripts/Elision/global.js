define("modules/moduleLoader",["exports","module"],function(a,b){"use strict";var c={init:function(){var a=Array.prototype.slice.call(document.querySelectorAll("[data-module]"));a.forEach(this.loadModule)},loadModule:function(a){var b=a.getAttribute("data-module");require([b],function(a){console.log(a)})}};b.exports=c}),define("global",["exports","modules/moduleLoader"],function(a,b){"use strict";var c=function(a){return a&&a.__esModule?a["default"]:a},d=c(b);d.init()});