import PluginError from "plugin-error";
import xmldoc from "xmldoc";
import through2 from "through2";
import { Buffer } from "buffer";

const PLUGIN_NAME = "gulp-resx2json";

module.exports = function(opt) {
  opt = opt || {};

  // Convert XML to JSON
  function doConvert(file) {
    var xml = file.contents;
    var doc = new xmldoc.XmlDocument(xml.toString());

    var resourceObject = {};
    var valueNodes = doc.childrenNamed("data");

    valueNodes.forEach(function(element) {
      var name = element.attr.name;
      const value = element.valueWithPath('value')
      resourceObject[name] = value;
    });

    return JSON.stringify(resourceObject);
  }

  return through2.obj((file, enc, cb) => {
    if (file.isStream()) {
       return cb(new PluginError(PLUGIN_NAME, "Streaming not supported"));
    }

    if (file.isBuffer()) {
      file.contents = Buffer.from(doConvert(file));
    }

    return cb(null, file);
  });
};
