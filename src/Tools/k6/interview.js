import http from "k6/http";
import { check, group } from "k6";
import { uuid, base64, template, assign } from "./libs/index.js";
import { Trend, Counter } from "k6/metrics";

const interviewJson = open("./data/interview.json")
const interviewTemplate = template(interviewJson)

const config = JSON.parse(open("./config.json"))

const interviewer = config.interviewers[Math.floor(Math.random() * config.interviewers.length)];

config["interviewerApi"] = "api/interviewer/v2";
config["assignmentsApi"] = "api/v1/assignments";

const data = {
    interid: null,
    superid: null,
    dunumber: null,
    interviewId: null,
    interviewKey: null,
    assignmentId: interviewer.assignment
}

// authToken will be passed here
let httpParams = null;

const uri = config.baseUri + "/" + config.interviewerApi + "/"

const statsPackageUpload = new Trend("interview_upload_duration");
const statsPackageUploaded = new Counter("interview_upload_count");

export default () => {

    group("interviewer flow", () => {
        group("authorization", () => {
            if (data.interid == null || data.superid == null) {
                loginIfRequired();

                var current = get("users/current");

                check(current, {
                    "can login with authToken": (r) => r.status === 200
                });

                const currentUserData = JSON.parse(current.body)

                data.interid = currentUserData.Id;
                data.superid = currentUserData.SupervisorId;
            }
        })

        group("Sending interviews", () => {
            data.interviewId = uuid();
            data.dunumber = Math.floor(Math.random() * 1000);
            data.interviewKey = Math.floor(Math.random() * 1000000000);

            var r = post("interviews/" + data.interviewId, interviewTemplate(data), {
                "interview": "upload"
            })

            statsPackageUpload.add(r.timings.duration);

            if (r.status == 200) {
                statsPackageUploaded.add(1);
            }

            check(r, {
                "send interview": (r) => r.status == 200
            })
        })
    })
};

function get(action) {
    return http.get(uri + action, httpParams);
}

function post(action, data, tag) {
    const parameters = assign(httpParams, {
        tags: { tag }
    })
    return http.post(uri + action, data, httpParams);
}

function loginIfRequired() {
    if (httpParams == null) {

        // aquire auth token
        var loginResult = post("users/login", {
            userName: interviewer.name,
            password: interviewer.pass
        });

        const checks = {}

        checks["authorized as " + interviewer.name] = (r) => r.status === 200;

        check(loginResult, checks);

        httpParams = {
            headers: {
                "Authorization": "AuthToken " + base64(interviewer.name + ":" + JSON.parse(loginResult.body)),
                "Content-Type": "application/json"
            }
        }
    }
}

function asQueryParams(obj) {
    var str = [];
    for (var p in obj)
        if (obj.hasOwnProperty(p)) {
            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    return str.join("&");
}
