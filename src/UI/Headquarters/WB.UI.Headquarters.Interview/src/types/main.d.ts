declare class InterviewEntity {
    id: string
    type: string
}

declare class SignalrHubChange {
    public oldState: number
    public newState: number
}

declare class HubChangedEvent {
    public state: SignalrHubChange
    public title: string

    constructor(state: SignalrHubChange, title: string)
}
