import http from "k6/http";
import { check, group } from "k6";
import { uuid, base64, template, assign } from "./libs/index.js";

const interviewJson = open("./data/interview.json")
const interviewTemplate = template(interviewJson)

const config = JSON.parse(open("./config.json"))

const data = {
    interid: null,
    superid: null,
    dunumber: null,
    interviewId: null,
    interviewKey: null
}

// authToken will be passed here
let httpParams = null;

export default () => {
    
    group("interviewer flow", () => {
        group("authorization", () => {
            loginIfRequired();

            var current = get("users/current");

            check(current, {
                "can login with authToken": (r) => r.status === 200
            });

            const currentUserData = JSON.parse(current.body)

            data.interid = currentUserData.Id;
            data.superid = currentUserData.SupervisorId;
            
        })

        group("interviews", () => {
            var interviewsR = get("interviews")

            check(interviewsR, {
                "can get list of interviews": (r) => r.status == 200
            })
        })

        group("Sending interviews", () => {
            data.interviewId = uuid();
            data.dunumber = Math.floor(Math.random() * 1000);
            data.interviewKey = Math.floor(Math.random() * 1000000000);

            var r = post("interviews/" + data.interviewId, interviewTemplate(data), {
                "interview" : "upload"
            })

            check(r, {
                "send interview": (r) => r.status == 200
            })
        })
    })
};

const uri = config.baseUri + "/" + config.interviewerApi + "/"

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
            userName: config.interviewer,
            password: config.interviewer_pass
        });

        check(loginResult, {
            "authorized": (r) => r.status === 200
        });

        httpParams = {
            headers: {
                "Authorization": "AuthToken " + base64(config.interviewer + ":" + JSON.parse(loginResult.body)),
                "Content-Type": "application/json"
            }
        }
    }
}