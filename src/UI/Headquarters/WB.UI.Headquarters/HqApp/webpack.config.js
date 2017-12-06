var buildFactory = require("./.build/webpack.common")

const config = {
    hq: {
        entry: "./src/hqapp/main.js",
        locales: ["Details", "Pages", "WebInterviewUI", "WebInterview", "DataTables", "Common", "Users", "Assignments", "Strings", "Reports", "DevicesInterviewers"]
    },
    webinterview: {
        entry: "./src/webinterview/main.js",
        locales: ["WebInterviewUI", "WebInterview", "Common"]
    }
};

module.exports = buildFactory(config);
