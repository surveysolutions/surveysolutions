module.exports = function(grunt) {
    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),

        uglify: {
            options: {
                sourceMap: true,
                sourceMapName: "console-shim.map"
            },
            main: {
                files: {
                    "console-shim.min.js": [ "console-shim.js" ]
                }
            }
        },
        
        clean: [
            "console-shim.min.js",
            "console-shim.map"
        ]
    });

    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-clean");
    
    grunt.registerTask("default", [ "uglify" ]);
};
