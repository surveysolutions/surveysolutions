declare class InterviewEntity {
    id: string
    type: string
}

declare var require: {
    (path: string): any;
    (paths: string[], callback: (...modules: any[]) => void): void;
    ensure: (paths: string[], callback: (require: (path: string) => any) => void, chunk: string) => void;
};
