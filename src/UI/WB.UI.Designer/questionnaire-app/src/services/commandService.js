import axios from 'axios';

axios.defaults.headers.common['Content-Type'] = 'application/json';

class CommandService {
  urlBase = '../../api/';
  urlCommands = urlBase + 'command';

  commandCall(type, command) {
    if (type.indexOf('Move') < 0) {
        blockUI.start();
    }
    return $http({
        method: 'POST',
        url: urlCommands,
        data: {
            "type": type,
            "command": JSON.stringify(command)
        },
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
    }).then(function (response) {
        blockUI.stop();
        return response;
    }, function (response) {
        blockUI.stop();
        return $q.reject(response);
    });
}
}

export default CommandService
