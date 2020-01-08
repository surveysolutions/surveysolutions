import LoginToDesigner from "./LoginToDesigner"

export default class Template{
    get routes() {
        return [{
            path: '/Template/LoginToDesigner', component: LoginToDesigner
        }]
    }
}
