import { mande } from 'mande';
import { useBlockUIStore } from '../stores/blockUI';

const commandsApi = mande('/api/command' /*, globalOptions*/);

export function commandCall(commandType, command) {
    const blockUI = useBlockUIStore();
    if (commandType.indexOf('Move') < 0) {
        blockUI.start();
    }

    return commandsApi
        .post({
            type: commandType,
            command: JSON.stringify(command)
        })
        .then(response => {
            blockUI.stop();
            return response;
        });
}
