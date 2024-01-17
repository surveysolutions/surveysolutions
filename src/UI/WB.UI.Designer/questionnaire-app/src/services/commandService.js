import { post } from '../services/apiService';

export function commandCall(commandType, command) {
    return post('/api/command', {type: commandType, command: JSON.stringify(command)});
}
