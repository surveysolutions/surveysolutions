const path = require('path')

var buildFactory = require("./.build/webpack.common")

const baseDir = path.resolve(__dirname, "../");
const join = path.join.bind(path, baseDir);

const config = {
    hq: {
        entry: "./src/hqapp/main.js",
        locales: ["Details", "Pages", "WebInterviewUI",
                  "WebInterview", "DataTables", "Common", "Users", "Assignments",
                   "Strings", "Report", "Reports", "DevicesInterviewers", "UploadUsers", 
                   "MainMenu", "WebInterviewSetup", "MapReport"]
    },
    webinterview: {
        entry: "./src/webinterview/main.js",
        locales: ["WebInterviewUI", "WebInterview", "Common"]
    },
    webtester: {
        entry: "./src/webinterview/main.js",
        locales: ["WebInterviewUI", "WebInterview", "Common"],
        assetsPath: '~/Content/Dist/',
        appRootPath: join("../../", 'WB.UI.WebTester')
    }
};

module.exports = buildFactory(config);
